// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Diagnostics;
using Unreal.Marshalling;
using Unreal.Util;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{Type}")]
    public class TransferableDefinition
    {
        public QualifiedTypeReference Type { get; }

        public ITypeMarshaller? Marshaller { get; }
        
        public bool IsVoid => Type.TypeInfo.IsVoid();

        public TransferableDefinition(ITypeInfo type,
            ManagedTransferType transferType = ManagedTransferType.ByValue,
            ITypeMarshaller? marshaller = null)
            : this(new QualifiedTypeReference(type, transferType), marshaller)
        { }

        public TransferableDefinition(QualifiedTypeReference type, ITypeMarshaller? marshaller = null)
        {
            marshaller ??= type.TypeInfo.DefaultMarshaller;
            if (type.TransferType != ManagedTransferType.ByValue && marshaller == null)
                marshaller = PassByReferenceMarshaller.Instance;

            Type = type;
            Marshaller = marshaller ?? type.TypeInfo.DefaultMarshaller;
        }

        public bool IsMarshalled(Codespace definition)
        {
            if (Marshaller == null)
                return false;

            return Marshaller.NeedsActiveMarshalling.HasSpace(definition);
        }

        public static readonly TransferableDefinition Void = new(ManagedTypeInfo.Void);
    }
}