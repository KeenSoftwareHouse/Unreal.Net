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
    /// <summary>
    /// Represents a pointer to a type that is known to native code but not to managed code.
    /// </summary>
    /// <remarks>In managed code it generates as the correct type, but to native it simply appears as <see cref="System.IntPtr"/>.</remarks>
    [DebuggerDisplay("{NativeName}")]
    public class OpaquePointerType : ITypeInfo
    {
        public string ManagedName => "IntPtr";

        public string ManagedSourceName => "IntPtr";

        public string NativeName { get; }

        public ITypeInfo? ParentType => null;

        public Type? ManagedType => null;

        public INamedTypeSymbol? TypeSymbol => null;

        public bool IsManagedUObject => false;

        public string Namespace => "System";

        public string Header { get; }

        public TypeKind Kind => TypeKind.Struct;

        public Module Module { get; }

        public NativeTransferType TypicalArgumentType => NativeTransferType.ByValue;

        public ITypeMarshaller? DefaultMarshaller => null;
        public string NativeModule => Module.Name;
        
        public bool IsGenericType => false;

        /// <summary>
        /// Define a new obscured type from it's native name and header.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="nativeName"></param>
        /// <param name="header"></param>
        public OpaquePointerType(Module module, string nativeName, string header)
        {
            Module = module;
            NativeName = nativeName + "*";
            Header = header;
        }

        /// <summary>
        /// Define a new obscured type from a native type.
        /// </summary>
        /// <param name="nativeType"></param>
        public OpaquePointerType(ITypeInfo nativeType)
        {
            Module = nativeType.Module;
            NativeName = nativeType.NativeName + "*";
            Header = nativeType.Header;
        }

        public static OpaquePointerType Get(ITypeInfo elementType)
        {
            return PointerTypes.GetValue(elementType, type => new OpaquePointerType(type));
        }

        private static readonly ConditionalWeakTable<ITypeInfo, OpaquePointerType> PointerTypes = new();
    }
}