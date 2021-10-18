// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Metadata;
using Unreal.NativeMetadata;

namespace Unreal.Marshalling
{
    internal class ConcreteTypeMapping : CustomTypeMapping
    {
        private readonly CustomTypeInfo m_customType;

        public ConcreteTypeMapping(GenerationContext context, INamedTypeSymbol type, NativeTypeAttributeData data,
            MarshalFormatsAttribute formats)
            : base(type, data)
        {
            m_customType = new CustomTypeInfo(context, type, data, new MarshalFormats(formats));

            context.RegisterType(m_customType);
        }

        public override ITypeInfo Get(UEProperty property)
        {
            return m_customType;
        }

        public override ITypeInfo Get(INamedTypeSymbol symbol)
        {
            return m_customType;
        }
    }
}