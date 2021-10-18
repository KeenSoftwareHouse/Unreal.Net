// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Unreal.Marshalling;
using Unreal.Metadata;
using Unreal.Util;

namespace Unreal.Generation
{
    public class ManagedFunctionBinder : FunctionWriterBase
    {
        public ManagedFunctionBinder(FunctionDefinition function)
            : base(function, MemberCodeComponentFlags.All)
        {
            AdditionalNamespaces.Add("System.ComponentModel");
            AdditionalNamespaces.Add("System.Runtime.InteropServices");
        }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            switch (component)
            {
                case MemberCodeComponent.NativeFunctionDeclaration:
                    WriteEntryPointDeclaration(writer);
                    break;
                case MemberCodeComponent.NativeClassDeclaration:
                    WriteNativeMethod(writer);
                    break;
                case MemberCodeComponent.NativeImplementation:
                    WriteEntryPointImplementation(writer);
                    break;
                case MemberCodeComponent.ManagedPart:
                    WriteManagedMethod(writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(component), component, null);
            }
        }

        private void WriteManagedMethod(CodeWriter writer)
        {
            writer.WriteLine($"[EditorBrowsable(EditorBrowsableState.Never)]");
            writer.WriteLine($"[UnmanagedCallersOnly(EntryPoint = \"{Member.EntryPointName}\")]");

            writer.Write("private static unsafe "); // "private static "

            writer.Write(Marshalling.Return.IntermediateType.FormatName(Codespace.Managed));
            writer.Write(" "); // "private static Type "

            writer.Write(Member.EntryPointName); // "private static Type EntryPoint"

            using (writer.OpenParenthesis("\n"))
                writer.Write(FormatMarshalledArgumentList(false, Codespace.Managed, MarshalOrder.Marshalled));

            using (writer.OpenBlock())
            {
                using(HandleAnyException(writer))
                    WriteBindingCall(writer, Codespace.Managed, Order.After);
            }
        }

        private void WriteNativeMethod(CodeWriter writer)
        {
            WriteNativeSignature(writer);

            using (writer.OpenBlock())
                WriteBindingCall(writer, Codespace.Native, Order.Before);
        }

        /// <summary>
        /// Declaration of the entry point for the managed function in native code.
        /// </summary>
        /// <param name="writer"></param>
        private void WriteEntryPointDeclaration(CodeWriter writer)
        {
            writer.Write("extern \"C\" ");

            writer.Write(Module.ModuleExport);
            writer.Write(" ");

            writer.Write(Marshalling.Return.OriginalType.FormatNative());
            writer.Write(" ");

            writer.Write(Member.EntryPointName);

            using (writer.OpenParenthesis())
                writer.Write(FormatMarshalledArgumentList(false, Codespace.Native, MarshalOrder.Marshalled));

            writer.WriteLine(";");
        }

        /// <summary>
        /// Native implementation for the entry point ( required for JIT builds).
        /// </summary>
        /// <param name="writer"></param>
        private void WriteEntryPointImplementation(CodeWriter writer)
        {
            var itemId = ++writer.GetVariable<int>("GeneratedStubId");

            var funcType = $"func_type_{itemId}";

            var parameters = new
            {
                FuncTypeDeclaration = Marshalling.MakeNativeIntermediateTypeSignature(funcType),
                Return = Marshalling.Return.OriginalType.FormatNative(),
                FirstCall = $"FirstCall{itemId}",
                Arguments = FormatMarshalledArgumentList(false, Codespace.Native, MarshalOrder.Marshalled),
                FuncType = funcType,
                FuncStorage = $"func_storage_{itemId}",
                ReturnIfNeeded = Marshalling.Return.IntermediateType.IsVoid() ? "" : "return ",
                ArgumentsTransfer = FormatMarshalledArgumentList(true, Codespace.Native, MarshalOrder.Marshalled),
                ModuleName = Module.Name,
                EnclosingTypeFullName = Member.EnclosingType!.GetManagedFullName(),
                Member.EntryPointName,
            };

            writer.WriteLine(TemplateWriter.WriteTemplate(EntryPointTemplate, parameters));
        }

        private const string EntryPointTemplate =
            @"#if defined(BUILD_JIT)
typedef {FuncTypeDeclaration};

static {Return} {FirstCall} ({Arguments});

static {FuncType} {FuncStorage} = {FirstCall};

static {Return} {FirstCall} ({Arguments})
{
    auto __function__ = ({FuncType}) FDotNetModule::Get()->GetManagedEntryPoint(
                            ""{ModuleName}"", ""{EnclosingTypeFullName}"",
                            ""{EntryPointName}"");
    if (!__function__)
        abort();

    {FuncStorage} = __function__;
    {ReturnIfNeeded}__function__({ArgumentsTransfer});
}

extern ""C"" {Return} {EntryPointName} ({Arguments})
{
    {ReturnIfNeeded}{FuncStorage}({ArgumentsTransfer});
} 
#endif
";
    }
}