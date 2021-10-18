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
    /// <summary>
    /// Defines a two way mapping between a managed and unmanaged type.
    /// </summary>
    public interface ITypeInfo
    {
        /// <summary>
        /// Managed name of the type.
        /// </summary>
        string ManagedName { get; }

        /// <summary>
        /// Name used when generating managed source for this type.
        /// </summary>
        /// <remarks>This will generally be the same as ManagedFullName, except for primitive types where their type token is used instead.</remarks>
        string ManagedSourceName { get; }

        /// <summary>
        /// Native name of the type.
        /// </summary>
        string NativeName { get; }

        /// <summary>
        /// Parent type. 
        /// </summary>
        ITypeInfo? ParentType { get; }

        /// <summary>
        /// Managed type. Can be null for types in the active compilation unit.
        /// </summary>
        Type? ManagedType { get; }

        /// <summary>
        /// Type symbol. Can be null for types not defined in the active compilation unit.
        /// </summary>
        INamedTypeSymbol? TypeSymbol { get; }

        /// <summary>
        /// Whether this type represents a UObject with a managed implementation.
        /// </summary>
        bool IsManagedUObject { get; }

        /// <summary>
        /// Managed namespace where this type is defined.
        /// </summary>
        /// <remarks>Empty string means the type is one of the built in types and does not need explicit inclusion.</remarks>
        string Namespace { get; }

        /// <summary>
        /// Native header file where the native counterpart of this type is defined.
        /// </summary>
        /// <remarks>Empty string means the type is reasonably expected to be defined in any compilation unit.</remarks>
        string Header { get; }

        /// <summary>
        /// The shape of this type.
        /// </summary>
        TypeKind Kind { get; }

        /// <summary>
        /// Module where this type is defined.
        /// </summary>
        public Module Module { get; }

        /// <summary>
        /// Typical native argument passing format.
        /// </summary>
        NativeTransferType TypicalArgumentType { get; }

        /// <summary>
        /// The default marshaller for this type.
        /// </summary>
        /// <remarks>If this member is null then the type requires no marshalling.</remarks>
        ITypeMarshaller? DefaultMarshaller { get; }

        /// <summary>
        /// Module where the native representation of this type is defined.
        /// </summary>
        string NativeModule { get; }
        
        bool IsGenericType { get; }
    }

    /// <summary>
    /// Generic type info.
    /// </summary>
    public interface IGenericTypeInfo : ITypeInfo
    {
        /// <summary>
        /// Names of any generic arguments this type has (if it is a generic type definition). 
        /// </summary>
        ImmutableArray<string> GenericParameters { get; }

        /// <summary>
        /// Concrete generic arguments if this type is a concrete generic type.
        /// </summary>
        ImmutableArray<ITypeInfo> GenericArguments { get; }
    }
}