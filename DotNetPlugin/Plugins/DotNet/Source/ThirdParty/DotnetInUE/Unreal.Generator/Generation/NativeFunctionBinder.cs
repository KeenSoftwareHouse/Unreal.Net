// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class NativeFunctionBinder : FunctionWriterBase
    {
        public NativeFunctionBinder(FunctionDefinition function)
            : base(function, MemberCodeComponentFlags.ManagedPart | MemberCodeComponentFlags.NativeImplementation)
        {
            AdditionalHeaders.Add("DotNet.h");
        }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            switch (component)
            {
                case MemberCodeComponent.NativeImplementation:
                    WriteNativeThunk(writer);
                    break;
                case MemberCodeComponent.ManagedPart:
                    WriteManagedMethod(writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(component), component, null);
            }
        }

        private void WriteNativeDelegateBinding(CodeWriter writer)
        {
            var functionPtrType = Marshalling.MakeIntermediateFunctionPointerSignature();
            writer.WriteLine(@$"private static unsafe {functionPtrType} {Member.EntryPointName} =
    ({functionPtrType})ModuleHelper.GetFunction(""{Member.EntryPointName}"");");
        }

        void WriteManagedMethod(CodeWriter writer)
        {
            WriteNativeDelegateBinding(writer);

            WriteManagedSignature(writer);

            using (writer.OpenBlock())
            {
                using (HandleAnyException(writer))
                    WriteBindingCall(writer, Codespace.Managed, Order.Before);
            }
        }

        void WriteNativeThunk(CodeWriter writer)
        {
            writer.Write("extern \"C\" ");

            writer.Write($"{Module.ModuleApi} ");

            writer.Write(Marshalling.Return.IntermediateType.FormatName(Codespace.Native));

            writer.Write($" {Member.EntryPointName}");

            using (writer.OpenParenthesis("\n"))
                writer.Write(FormatMarshalledArgumentList(false, Codespace.Native, MarshalOrder.Marshalled));

            using (writer.OpenBlock())
                WriteBindingCall(writer, Codespace.Native, Order.After);
        }
    }
}