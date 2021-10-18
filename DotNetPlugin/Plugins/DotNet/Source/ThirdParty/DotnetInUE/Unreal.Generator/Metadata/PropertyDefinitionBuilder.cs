// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Immutable;
using Unreal.Generation;

namespace Unreal.Metadata
{
    public class PropertyDefinitionBuilder<TBuilder> : MemberDefinitionBuilder<TBuilder>
        where TBuilder : PropertyDefinitionBuilder<TBuilder>
    {
        protected QualifiedTypeReference Type;

        protected string? Initializer;

        protected PropertyDefinitionBuilder(Module module, string name, TypeDefinition declaringType)
            : base(module, name, declaringType)
        { }

        public TBuilder WithType(ITypeInfo type,
            ManagedTransferType transfer = ManagedTransferType.ByValue)
        {
            Type = new QualifiedTypeReference(type, transfer);
            return Get();
        }

        public TBuilder WithType(QualifiedTypeReference type)
        {
            Type = type;
            return Get();
        }

        public TBuilder WithType<T>(ManagedTransferType transfer = ManagedTransferType.ByValue)
        {
            Type = new QualifiedTypeReference(ManagedTypeInfo.GetType<T>(), transfer);
            return Get();
        }

        public TBuilder WithInitializer(string initializer)
        {
            Initializer = initializer;
            return Get();
        }
    }

    public class PropertyDefinitionBuilder : PropertyDefinitionBuilder<PropertyDefinitionBuilder>
    {
        public PropertyDefinitionBuilder(Module module, string name, TypeDefinition declaringType)
            : base(module, name, declaringType)
        { }

        public PropertyDefinition Build()
        {
            return new(Name, Module, GetMetaAttributes(), Documentation, Comments, DeclaringType,
                Visibility, Attributes, ManagedAttributes.ToImmutableArray(), Type, Initializer);
        }
    }
}