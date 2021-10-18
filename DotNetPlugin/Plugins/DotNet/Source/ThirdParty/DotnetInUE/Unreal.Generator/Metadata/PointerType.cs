// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{ManagedName}")]
    public class PointerType : ITypeInfo
    {
        /// <inheritdoc />
        public string ManagedName => ElementType.ManagedName + "*";

        /// <inheritdoc />
        public string ManagedSourceName => ElementType.ManagedSourceName + "*";

        /// <inheritdoc />
        public string NativeName => ElementType.NativeName + "*";

        /// <inheritdoc />
        public ITypeInfo? ParentType => null; // ValueType?

        /// <inheritdoc />
        public Type? ManagedType => null; // make pointer type?

        /// <inheritdoc />
        public INamedTypeSymbol? TypeSymbol => null;

        /// <inheritdoc />
        public bool IsManagedUObject => false;

        /// <inheritdoc />
        public string Namespace => ElementType.Namespace;

        /// <inheritdoc />
        public string Header => ElementType.Header;

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.Struct;

        public Module Module => ElementType.Module;

        /// <inheritdoc />
        public NativeTransferType TypicalArgumentType => NativeTransferType.ByValue;

        /// <inheritdoc />
        public ITypeMarshaller? DefaultMarshaller => null;

        public string NativeModule => Module.Name;
        
        public bool IsGenericType => false;

        public ITypeInfo ElementType { get; }

        public PointerType(ITypeInfo elementType)
        {
            if (!elementType.Kind.IsValueType())
                throw new InvalidOperationException("Pointer types cannot be constructed from reference types.");

            ElementType = elementType;
        }

        public static PointerType Get(ITypeInfo elementType)
        {
            return PointerTypes.GetValue(elementType, type => new PointerType(type));
        }

        private static readonly ConditionalWeakTable<ITypeInfo, PointerType> PointerTypes = new();
    }
}