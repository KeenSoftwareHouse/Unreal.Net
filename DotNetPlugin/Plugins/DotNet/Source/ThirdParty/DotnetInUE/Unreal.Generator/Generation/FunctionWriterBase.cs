// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public abstract class FunctionWriterBase : MemberWriter<FunctionDefinition>
    {
        /// <summary>
        /// Marshalling info for the function.
        /// </summary>
        protected readonly FunctionMarshalling Marshalling;

        protected bool IsStatic => Member.Attributes.HasAttribute(SymbolAttribute.Static);

        /// <summary>
        /// Custom function body, instead of just calling a matching function on the defining type.
        /// </summary>
        public string? CustomBody { get; set; }

        public FunctionWriterBase(FunctionDefinition member,
            MemberCodeComponentFlags components = MemberCodeComponentFlags.None)
            : base(member, components)
        {
            Marshalling = new FunctionMarshalling(member);
        }

        public override IEnumerable<ITypeInfo> GetTypeDependencies(Codespace space)
        {
            for (int i = 0; i < Member.Parameters.Length; ++i)
            {
                foreach (var dep in Member.Parameters[i].Type.TypeInfo.GetTypeDependencies(true))
                    yield return dep;
            }

            foreach (var dep in Member.Return.Type.TypeInfo.GetTypeDependencies(true))
                yield return dep;
        }

        protected void WriteNativeSignature(CodeWriter writer)
        {
            WriteComments(writer, Codespace.Native);

            WriteAnnotations(writer, Codespace.Native, "UFunction");

            // Note: Visibility will be handled by the class writer in C++.

            // Write attributes
            if (Member.Attributes.HasAttribute(SymbolAttribute.Static))
                writer.Write("static ");

            if (Member.Attributes.HasAttribute(SymbolAttribute.Virtual))
                writer.Write("virtual ");

            if (Member.SpecialMethod == MethodSpecialType.None)
                writer.Write($"{Member.Return.Type.FormatNative()} "); // "[static] type "

            WriteName(writer);

            using (writer.OpenParenthesis())
                writer.Write(FormatArgumentList(false, Codespace.Native));
            if (Member.Attributes.HasAttribute(SymbolAttribute.Override))
                writer.Write(" override");

            writer.WriteLine();
        }

        protected void WriteManagedSignature(CodeWriter writer)
        {
            WriteComments(writer, Codespace.Managed);

            WriteAnnotations(writer, Codespace.Managed, "UFunction");

            WriteManagedVisibilityAndAttributes(writer);

            if (Member.SpecialMethod == MethodSpecialType.None)
                writer.Write($"{Member.Return.Type.FormatManaged()} ");

            WriteName(writer);

            using (writer.OpenParenthesis("\n"))
                writer.Write(FormatArgumentList(false, Codespace.Managed));
        }

        /// <summary>
        /// Format argument list fo the standard method definition.
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="space"></param>
        /// <returns></returns>
        protected string FormatArgumentList(bool invocation, Codespace space)
        {
            StringBuilder sb = new();

            for (int i = 0; i < Member.Parameters.Length; ++i)
            {
                if (i > 0)
                    sb.Append(", ");

                ParameterDefinition p = Member.Parameters[i];

                if (!invocation)
                {
                    if (space == Codespace.Managed)
                        sb.Append(p.Type.FormatManaged());
                    else
                        sb.Append(p.Type.FormatNative());

                    sb.Append(" ");
                }
                else if (p.Type.TransferType != ManagedTransferType.ByValue && space == Codespace.Managed)
                {
                    if (p.Type.TransferType == ManagedTransferType.Out)
                        sb.Append("out ");
                    else if (p.Type.TransferType == ManagedTransferType.Ref)
                        sb.Append("ref ");
                }

                sb.Append(p.Name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format the argument list for a marshalled method call.
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="space"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        protected string FormatMarshalledArgumentList(bool invocation, Codespace space, MarshalOrder order)
        {
            bool callUnmarshalled = Marshalling.HasModifiedParameters && order != MarshalOrder.Marshalled;

            int start = callUnmarshalled && Marshalling.HasThis ? 1 : 0;
            int end = callUnmarshalled && Marshalling.HasModifiedReturn
                ? Marshalling.Parameters.Length - 1
                : Marshalling.Parameters.Length;

            StringBuilder sb = new();

            for (int i = start; i < end; ++i)
            {
                if (i > start)
                    sb.Append(", ");

                ref var p = ref Marshalling.Parameters[i];

                var type = p.GetType(order);

                if (!invocation)
                {
                    if (space == Codespace.Managed)
                        sb.Append(type.FormatManaged());
                    else
                        sb.Append(type.FormatNative());

                    sb.Append(" ");
                }
                else if (type.TransferType != ManagedTransferType.ByValue && space == Codespace.Managed)
                {
                    if (type.TransferType == ManagedTransferType.Out)
                        sb.Append("out ");
                    else if (type.TransferType == ManagedTransferType.Ref)
                        sb.Append("ref ");
                }

                sb.Append(p.GetName(space, order));
            }

            return sb.ToString();
        }

        protected void WriteBindingCall(CodeWriter writer, Codespace sourceSpace, Order order)
        {
            string varKeyword = sourceSpace.IsManaged() ? "var" : "auto";

            var marshalOrder = order == Order.Before ? MarshalOrder.Before : MarshalOrder.Marshalled;
            var nextOrder = marshalOrder.GetNext();

            bool hasOutArguments = Marshalling.HasAny(sourceSpace) && Marshalling.HasOutArguments;

            if (order == Order.Before)
            {
                if (Marshalling.HasThis)
                    writer.WriteLine(
                        $"{Member.EnclosingType.FormatName(sourceSpace)} {Marshalling.Parameters[0].Name} = this;");

                if (Marshalling.HasModifiedReturn)
                {
                    ref var ret = ref Marshalling.GetReturnParameter();
                    var varType = ret.OriginalType.TypeInfo.FormatName(sourceSpace);
                    writer.WriteLine($"{varType} {Marshalling.Return.Name};");
                }
            }

            // Marshal In Arguments.
            if (Marshalling.HasAny(sourceSpace))
            {
                var parameters = Marshalling.Parameters;
                for (int i = 0; i < parameters.Length; ++i)
                {
                    ref var p = ref parameters[i];
                    if (!p.IsMarshalled(sourceSpace))
                        continue;

                    var fromName = p.GetName(sourceSpace, marshalOrder);
                    var toName = p.GetName(sourceSpace, nextOrder);

                    p.Marshaller!.MarshalVariable(writer, p.OriginalType, fromName, toName, sourceSpace, order, false);
                }
            }

            if (order == Order.After && CustomBody != null)
            {
                writer.WriteLine(CustomBody);
                return;
            }

            bool returnMarshalled = false;
            bool returnLater = false;
            if (!Marshalling.Return.IsVoid)
            {
                if (Marshalling.Return.IsMarshalled(sourceSpace))
                {
                    writer.Write($"{varKeyword} {Marshalling.Return.MarshalledName} = ");
                    returnMarshalled = true;
                    returnLater = true;
                }
                else if (hasOutArguments)
                {
                    // Save return for later.
                    writer.Write($"{varKeyword} {Marshalling.Return.Name} = ");
                    returnLater = true;
                }
                else
                {
                    writer.Write("return ");
                }
            }
            else if (Marshalling.HasModifiedReturn && order == Order.After)
            {
                writer.Write($"{Marshalling.Return.Name} = ");
            }

            if (order == Order.Before)
            {
                writer.Write(Member.EntryPointName);
            }
            else
            {
                if (IsStatic)
                {
                    writer.Write(Member.EnclosingType.Name);
                    writer.Write(sourceSpace.IsManaged() ? "." : "::");
                }
                else
                {
                    writer.Write(Marshalling.Parameters[0].Name);
                    writer.Write(sourceSpace.IsManaged() ? "." : "->");
                }

                writer.Write(Member.Name);
            }

            using (writer.OpenParenthesis(";\n"))
                writer.Write(FormatMarshalledArgumentList(true, sourceSpace, nextOrder));

            if (hasOutArguments)
            {
                var marshalOutOrder = order.GetOpposite();

                var parameters = Marshalling.Parameters;
                for (int i = 0; i < parameters.Length; ++i)
                {
                    ref var p = ref parameters[i];
                    if (!p.IsMarshalled(sourceSpace) || !p.MarshalOut)
                        continue;

                    var fromName = p.GetName(sourceSpace, nextOrder);
                    var toName = p.GetName(sourceSpace, marshalOrder);

                    p.Marshaller!.MarshalVariable(writer, p.OriginalType, fromName, toName, sourceSpace,
                        marshalOutOrder, true);
                }
            }

            if (returnLater)
            {
                if (returnMarshalled)
                {
                    // unMarshal the returned value.
                    Marshalling.Return.Marshaller!.MarshalVariable(writer, Member.Return.Type,
                        Marshalling.Return.MarshalledName, Marshalling.Return.Name,
                        sourceSpace, order.GetOpposite(), false);
                }

                // For some types this will generate code that could be simplified, but at little cost to the actual performance so not bothering.
                writer.WriteLine($"return {Marshalling.Return.Name};");
            }
            else if (Marshalling.HasModifiedReturn && order == Order.Before)
            {
                writer.WriteLine($"return {Marshalling.GetReturnParameter().Name};");
            }
        }

        protected void WriteName(CodeWriter writer)
        {
            switch (Member.SpecialMethod)
            {
                case MethodSpecialType.None:
                    writer.Write(Member.Name);
                    break;
                case MethodSpecialType.Constructor:
                    writer.Write(Member.EnclosingType.NativeName);
                    break;
                case MethodSpecialType.Destructor:
                    writer.Write("~" + Member.EnclosingType.NativeName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected CodeWriterToken HandleAnyException(CodeWriter writer)
        {
            var token = writer.GetCurrentToken();

            writer.WriteLine("try");
            var blockToken = writer.OpenBlock();

            writer.PushActionTrail(x =>
            {
                // Pop the open block we added. This means the delegate must be allocated, let's try to work around that later.
                blockToken.Dispose();
                
                x.WriteLine("catch (Exception __ex)");
                using (x.OpenBlock())
                {
                    x.WriteLine("Unreal.Core.UeLog.Log(Unreal.Core.LogVerbosity.Error, __ex.ToString());");
                    x.WriteLine("throw;");
                }
            });

            return token;
        }
    }
}