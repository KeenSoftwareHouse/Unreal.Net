// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Marshalling;
using Unreal.Util;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{Kind} {ManagedName}")]
    public class SymbolTypeInfo : ITypeInfo
    {
        public string ManagedName { get; }

        public string ManagedSourceName { get; }

        public string NativeName { get; }

        public ITypeInfo? ParentType { get; }

        public Type? ManagedType => null;

        public INamedTypeSymbol? TypeSymbol { get; }

        public bool IsManagedUObject { get; }

        public string Namespace { get; }

        public string Header { get; }

        public TypeKind Kind { get; }
        public Module Module { get; }

        public NativeTransferType TypicalArgumentType { get; }

        public ITypeMarshaller? DefaultMarshaller { get; }

        public string NativeModule { get; } = "";

        public bool IsGenericType => false;

        public SymbolTypeInfo(INamedTypeSymbol symbol, GenerationContext context)
        {
            TypeSymbol = symbol;

            ManagedName = ManagedSourceName = NativeName = symbol.Name;

            Kind = symbol.GetKind();

            Namespace = symbol.ContainingNamespace.GetFullMetadataName();

            var attributes = symbol.GetAttributes();

            TypicalArgumentType = Kind == TypeKind.Struct || Kind == TypeKind.Enum
                ? NativeTransferType.ByValue
                : NativeTransferType.ByPointer;

            var headerAttributeType = context.GetSymbol<HeaderAttribute>();
            var moduleAttributeType = context.GetSymbol<ModuleAttribute>();
            var uClassAttributeType = context.GetSymbol<UClassAttribute>();

            var managedTypeAttributeType = context.GetSymbol<ManagedTypeAttribute>();

            Header = "";
            IsManagedUObject = false;

            if (symbol.BaseType != null
                && !SymbolEqualityComparer.Default.Equals(symbol.BaseType, context.GetSymbol<object>())) // Ignore System.Object.
            {
                ParentType = context.TypeResolver.Resolve(symbol.BaseType);
                IsManagedUObject = ParentType.IsManagedUObject; // Copy from parent.
            }

            foreach (var attr in attributes)
            {
                if (AreEqual(attr.AttributeClass!, headerAttributeType))
                {
                    // The only legal way to declare a header attribute is with a valid string argument.
                    Header = (string) attr.ConstructorArguments[0].Value!;
                }

                if (AreEqual(attr.AttributeClass!, moduleAttributeType))
                {
                    // The only legal way to declare a module attribute is with a valid string argument.
                    NativeModule = (string) attr.ConstructorArguments[0].Value!;
                }
                else if (AreEqual(attr.AttributeClass!, uClassAttributeType))
                {
                    DefaultMarshaller = NativeUObjectMarshaller.Instance;
                }
                else if (!Kind.IsValueType() && AreEqual(attr.AttributeClass!, managedTypeAttributeType))
                {
                    DefaultMarshaller = ManagedUObjectMarshaller.Instance;
                    IsManagedUObject = true;
                }
            }

            Module = context.GetModule(symbol.ContainingModule);
        }

        private static bool AreEqual(ITypeSymbol lhs, ITypeSymbol rhs)
        {
            return SymbolEqualityComparer.Default.Equals(lhs, rhs);
        }
    }
}