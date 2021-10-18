// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public abstract class CustomTypeMarshallerBase : ITypeMarshaller
    {
        public CodespaceFlags NeedsActiveMarshalling => CodespaceFlags.All;
        public bool NeedsReturnValueInversion => false;
        public abstract string? AdditionalHeader { get; }
        public abstract string? AdditionalNamespace { get; }

        public void MarshalVariable(CodeWriter writer, QualifiedTypeReference type, string name, string outputName,
            Codespace space, Order order, bool afterCall)
        {
            var intermediateType = GetIntermediateType(type.TypeInfo);
            var toMarshal = new QualifiedTypeReference(intermediateType);

            // Handle after calls
            if (afterCall)
            {
                string partialName = name;
                if (type.TransferType != ManagedTransferType.ByValue)
                    partialName = order == Order.Before ? $"{outputName}__partial" : $"{name}__partial";

                if (order == Order.Before)
                    writer.WriteLine($"{partialName} = {Marshal(name)};");
                else
                    writer.WriteLine($"{outputName} = {Marshal(partialName)};");
                return;
            }

            if (order == Order.Before)
            {
                var partialName = outputName;
                if (type.TransferType != ManagedTransferType.ByValue)
                    partialName = $"{outputName}__partial";

                if (type.TransferType != ManagedTransferType.Out)
                    writer.WriteLine($"{intermediateType.FormatName(space)} {partialName} = {Marshal(name)};");
                else
                    writer.WriteLine($"{intermediateType.FormatName(space)} {partialName};");

                MarshalReference(partialName, outputName);
            }
            else
            {
                var partialName = name;
                if (type.TransferType != ManagedTransferType.ByValue)
                    partialName = $"{name}__partial";

                MarshalReference(name, partialName);

                if (type.TransferType != ManagedTransferType.Out)
                    writer.WriteLine($"{type.TypeInfo.FormatName(space)} {outputName} = {Marshal(partialName)};");
                else
                    writer.WriteLine($"{type.TypeInfo.FormatName(space)} {outputName};");
            }

            void MarshalReference(string from, string to)
            {
                if (type.TransferType != ManagedTransferType.ByValue)
                {
                    PassByReferenceMarshaller.Instance.MarshalVariable(writer, toMarshal, @from,
                        to, space, order, afterCall);
                }
            }

            string Marshal(string field) => FormatMarshalled(type, space, order, field);
        }

        protected abstract ITypeInfo GetIntermediateType(ITypeInfo type);

        protected abstract string FormatMarshalled(QualifiedTypeReference type, Codespace space, Order order,
            string field);

        public ITypeInfo GetIntermediateType(QualifiedTypeReference type)
        {
            // Make ptr if in/ref/out
            if (type.TransferType != ManagedTransferType.ByValue)
                return PointerType.Get(GetIntermediateType(type.TypeInfo));

            return GetIntermediateType(type.TypeInfo);
        }
    }
}