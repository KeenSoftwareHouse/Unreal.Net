// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    /// <summary>
    /// Native only type with no managed representation.
    /// </summary>
    [DebuggerDisplay("{NativeName}/{Header}")]
    public class NativeOnlyType : ITypeInfo
    {
        public string ManagedName => "";

        public string ManagedSourceName => "";

        public string NativeName { get; }

        public ITypeInfo? ParentType => null;

        public Type? ManagedType => null;

        public INamedTypeSymbol? TypeSymbol => null;

        public bool IsManagedUObject => false;

        public string Namespace => "";

        public string Header { get; }

        public TypeKind Kind => TypeKind.Struct;
        
        public Module Module { get; }

        public NativeTransferType TypicalArgumentType { get; }

        public ITypeMarshaller? DefaultMarshaller => null;
        public string NativeModule => Module.Name;
        
        public bool IsGenericType => false;

        /// <summary>
        /// Define a new obscured type from it's native name and header.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="nativeName"></param>
        /// <param name="header"></param>
        /// <param name="typicalArgumentType"></param>
        public NativeOnlyType(Module module, string nativeName, string header = "", NativeTransferType typicalArgumentType = NativeTransferType.ByPointer)
        {
            Module = module;
            NativeName = nativeName;
            Header = header;
            TypicalArgumentType = typicalArgumentType;
        }
    }
}