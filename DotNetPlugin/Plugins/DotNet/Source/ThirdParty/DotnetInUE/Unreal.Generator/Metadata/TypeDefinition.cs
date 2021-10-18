// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{Kind} {Name}")]
    public partial class TypeDefinition : MemberDefinition, ITypeInfo
    {
        public string ManagedName => Name;

        public string ManagedSourceName => Name;
        public string NativeName { get; }

        public ITypeInfo? ParentType { get; }
        public Type? ManagedType => null;
        public INamedTypeSymbol? TypeSymbol { get; }
        public bool IsManagedUObject { get; }
        public string Namespace { get; }
        public string Header { get; }
        public TypeKind Kind { get; }

        Module ITypeInfo.Module => Module;
        public NativeTransferType TypicalArgumentType { get; }
        public ITypeMarshaller? DefaultMarshaller { get; }

        /// <summary>
        /// Name without the unreal prefix (U/A/E/F/S)
        /// </summary>
        public string CosmeticName { get; }

        /// <summary>
        /// Module where the native representation of this type is defined.
        /// </summary>
        public string NativeModule { get; }
        
        // NOTE: Currently building generic types is not supported.
        public bool IsGenericType => false;

        public TypeDefinition(string name, Module module, ImmutableList<MetaAttribute>? metaAttributes,
            Documentation? documentation, string comments, TypeDefinition? enclosingType, SymbolVisibility visibility,
            SymbolAttributeFlags attributes, ImmutableArray<string> managedAttributes, string nativeName,
            ITypeInfo? parentType,
            INamedTypeSymbol? typeSymbol, bool isManagedUObject,
            string ns, string header, TypeKind kind, NativeTransferType typicalArgumentType,
            ITypeMarshaller? defaultMarshaller, string cosmeticName, string nativeModule)
            : base(name, module, metaAttributes, documentation, comments, enclosingType, visibility, attributes,
                managedAttributes)
        {
            NativeName = nativeName;
            ParentType = parentType;
            TypeSymbol = typeSymbol;
            IsManagedUObject = isManagedUObject;
            Namespace = ns;
            Header = header;
            Kind = kind;
            TypicalArgumentType = typicalArgumentType;
            DefaultMarshaller = defaultMarshaller;
            CosmeticName = cosmeticName;
            NativeModule = nativeModule;
        }

        public new static TypeDefinitionBuilder CreateBuilder(Module module, string name,
            TypeDefinition? declaringType = null)
        {
            return new(module, name, declaringType);
        }
    }
}