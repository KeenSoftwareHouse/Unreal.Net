// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Linq;
using Microsoft.CodeAnalysis;
using Unreal.Metadata;
using Unreal.NativeMetadata;

namespace Unreal.Marshalling
{
    internal class GenericTypeMapping : CustomTypeMapping
    {
        private readonly TypeResolver m_resolver;
        private readonly INamedTypeSymbol m_type;
        private readonly NativeTypeAttributeData m_data;
        private readonly MarshalFormats m_formats;

        public GenericTypeMapping(TypeResolver resolver, INamedTypeSymbol type,
            NativeTypeAttributeData data, MarshalFormatsAttribute formats)
            : base(type, data)
        {
            m_resolver = resolver;
            m_type = type;
            m_data = data;
            m_formats = new MarshalFormats(formats);
        }

        public override ITypeInfo Get(UEProperty property)
        {
            // Ignoring attributes of nested properties.
            var types = property.GenericTypeParameters.Select(x => m_resolver.Resolve(x).TypeInfo).ToArray();

            return CreateTypeInfo(types);
        }

        public override ITypeInfo Get(INamedTypeSymbol symbol)
        {
            var types = symbol.TypeArguments.Select(x => m_resolver.Resolve(x)).ToArray();

            return CreateTypeInfo(types, symbol);
        }

        private CustomGenericTypeInfo CreateTypeInfo(ITypeInfo[] types, INamedTypeSymbol? concreteType = null)
        {
            var typeInfo =
                new CustomGenericTypeInfo(m_resolver.Context, m_type, m_data, m_formats, types, concreteType);

            // Register type so it's cached.
            if (concreteType != null)
                m_resolver.Context.RegisterType(typeInfo);

            return typeInfo;
        }
    }
}