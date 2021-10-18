// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    public class TypeDefinitionBuilder<TBuilder> : MemberDefinitionBuilder<TBuilder>
        where TBuilder : TypeDefinitionBuilder<TBuilder>
    {
        public string? NativeName;
        public ITypeInfo? ParentType;
        public INamedTypeSymbol? TypeSymbol;
        public bool IsManagedUObject;
        public string Namespace;
        public string Header;
        public TypeKind Kind;
        public NativeTransferType TypicalArgumentType;
        public ITypeMarshaller? DefaultMarshaller;
        public string CosmeticName;
        public string NativeModule;

        public TypeDefinitionBuilder(Module module, string name, TypeDefinition? declaringType)
            : base(module, name, declaringType)
        {
            NativeModule = module.Name;
            Header = "";
            Kind = TypeKind.Class;
            TypicalArgumentType = NativeTransferType.ByValue;
            Namespace = "";
            CosmeticName = name;
        }
        
        public TBuilder WithNativeName(string nativeName)
        {
            NativeName = nativeName;
            return Get();
        }

        public TBuilder WithParentType(ITypeInfo? parentType)
        {
            ParentType = parentType;
            return Get();
        }

        public TBuilder WithTypeSymbol(INamedTypeSymbol? typeSymbol)
        {
            TypeSymbol = typeSymbol;
            return Get();
        }

        public TBuilder WithIsManagedUObject(bool isManagedUObject)
        {
            IsManagedUObject = isManagedUObject;
            return Get();
        }

        public TBuilder WithNamespace(string @namespace)
        {
            Namespace = @namespace;
            return Get();
        }

        public TBuilder WithHeader(string header)
        {
            Header = header;
            return Get();
        }

        public TBuilder WithKind(TypeKind kind)
        {
            Kind = kind;
            return Get();
        }

        public TBuilder WithTypicalArgumentType(NativeTransferType typicalArgumentType)
        {
            TypicalArgumentType = typicalArgumentType;
            return Get();
        }

        public TBuilder WithDefaultMarshaller(ITypeMarshaller? defaultMarshaller)
        {
            DefaultMarshaller = defaultMarshaller;
            return Get();
        }

        public TBuilder WithCosmeticName(string cosmeticName)
        {
            CosmeticName = cosmeticName;
            return Get();
        }

        public TBuilder WithNativeModule(string nativeModule)
        {
            NativeModule = nativeModule;
            return Get();
        }
    }

    public class TypeDefinitionBuilder : TypeDefinitionBuilder<TypeDefinitionBuilder>
    {
        public TypeDefinitionBuilder(Module module, string name, TypeDefinition? declaringType)
            : base(module, name, declaringType)
        { }

        public TypeDefinition Build()
        {
            // Use default name if native name is not set.
            NativeName ??= Name;
            
            return new(Name, Module, GetMetaAttributes(), Documentation, Comments, DeclaringType,
                Visibility, Attributes, ManagedAttributes.ToImmutableArray(), NativeName, ParentType, TypeSymbol, IsManagedUObject, Namespace, Header, Kind,
                TypicalArgumentType, DefaultMarshaller, CosmeticName, NativeModule);
        }
    }
}