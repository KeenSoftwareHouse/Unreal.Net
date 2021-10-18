// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public class PassByReferenceMarshaller : ITypeMarshaller
    {
        public static readonly ITypeMarshaller Instance = new PassByReferenceMarshaller();

        private PassByReferenceMarshaller()
        { }

        public CodespaceFlags NeedsActiveMarshalling => CodespaceFlags.All;
        public bool NeedsReturnValueInversion => true;
        public string? AdditionalHeader => "";
        public string? AdditionalNamespace => "";

        public void MarshalVariable(CodeWriter writer, QualifiedTypeReference type, string name, string outputName,
            Codespace space, Order order, bool afterCall)
        {
            if (afterCall)
                return;

            if (space == Codespace.Managed)
            {
                if (order == Order.Before)
                {
                    if (type.TransferType == ManagedTransferType.ByValue)
                    {
                        writer.WriteLine($"{type.TypeInfo.ManagedName}* {outputName} = &{name};");
                    }
                    else
                    {
                        writer.WriteLine($"fixed({type.TypeInfo.ManagedName}* {outputName} = &{name})");
                        writer.OpenBlock();
                    }
                }
                else
                {
                    writer.WriteLine($"var ref {outputName} = ref *{name};");
                }
            }
            else
            {
                if (order == Order.Before)
                {
                    writer.WriteLine($"{type.TypeInfo.NativeName}* {outputName} = &{name};");
                }
                else
                {
                    writer.WriteLine($"{type.TypeInfo.NativeName}& {outputName} = *{name};");
                }
            }
        }

        public ITypeInfo GetIntermediateType(QualifiedTypeReference type)
        {
            return PointerType.Get(type.TypeInfo);
        }
    }
}