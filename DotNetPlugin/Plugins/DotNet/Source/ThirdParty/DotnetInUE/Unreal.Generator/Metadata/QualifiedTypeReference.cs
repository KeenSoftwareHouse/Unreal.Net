// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Unreal.Marshalling;
using Unreal.Util;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{ToString()}")]
    public readonly struct QualifiedTypeReference
    {
        public readonly ITypeInfo TypeInfo;

        public readonly ManagedTransferType TransferType;

        public bool IsVoid => TypeInfo.IsVoid();

        public QualifiedTypeReference(ITypeInfo typeInfo,
            ManagedTransferType transferType = ManagedTransferType.ByValue)
        {
            TypeInfo = typeInfo;
            TransferType = transferType;
        }

        public QualifiedTypeReference WithTransfer(ManagedTransferType transfer)
        {
            return new(TypeInfo, transfer);
        }

        public string Format(Codespace space) => space == Codespace.Native ? FormatNative() : FormatManaged();

        public string FormatNative()
        {
            var transferFormat = NativeTransferType.ByValue; 
            // If the specified transfer is not by value and the native transfer is not by pointer we collapse to by ref.
            
            if (TransferType != ManagedTransferType.ByValue)
                transferFormat = NativeTransferType.ByReference;

            if (TransferType == ManagedTransferType.In)
                transferFormat = transferFormat.MakeConst();

            return transferFormat.FormatType(TypeInfo.FormatName(Codespace.Native));
        }

        public string FormatManaged()
        {
            switch (TransferType)
            {
                case ManagedTransferType.ByValue:
                    return TypeInfo.ManagedSourceName;
                case ManagedTransferType.In:
                    return "in " + TypeInfo.ManagedSourceName;
                case ManagedTransferType.Out:
                    return "out " + TypeInfo.ManagedSourceName;
                case ManagedTransferType.Ref:
                    return "ref " + TypeInfo.ManagedSourceName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsMarshalled => TypeInfo.DefaultMarshaller != null;

        public ITypeMarshaller? Marshaller => TypeInfo.DefaultMarshaller;

        public override string ToString()
        {
            return FormatManaged();
        }
    }
}