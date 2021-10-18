// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Unreal.Generation;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public abstract class UObjectMarshaller : CustomTypeMarshallerBase
    {
        public override string? AdditionalHeader => null;
        public override string? AdditionalNamespace => "Unreal.Core";

        protected override string FormatMarshalled(QualifiedTypeReference type, Codespace space, Order order,
            string field)
        {
            if (space == Codespace.Native)
                return field;

            if (order == Order.Before)
                return $"UObjectUtil.GetNativeInstance({field})";
            return MarshalFromNativeAfter(type, field);
        }

        public abstract string MarshalFromNativeAfter(QualifiedTypeReference type, string argumentName);

        protected override ITypeInfo GetIntermediateType(ITypeInfo type) => OpaquePointerType.Get(type);
    }
}