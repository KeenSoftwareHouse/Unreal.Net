// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.Metadata;
using Unreal.NativeMetadata;

namespace Unreal.Marshalling
{
    public abstract class CustomTypeMapping
    {
        public readonly ITypeSymbol TypeSymbol;

        public readonly string NativeTypeName;

        protected CustomTypeMapping(ITypeSymbol typeSymbol, NativeTypeAttributeData attributeData)
        {
            TypeSymbol = typeSymbol;
            NativeTypeName = attributeData.NativeTypeName;
        }

        public abstract ITypeInfo Get(UEProperty property);
        
        public abstract ITypeInfo Get(INamedTypeSymbol symbol);
    }
}