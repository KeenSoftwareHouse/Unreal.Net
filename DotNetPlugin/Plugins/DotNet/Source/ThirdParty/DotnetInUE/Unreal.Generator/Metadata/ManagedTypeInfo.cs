// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Unreal.Marshalling;
using Module = Unreal.Generation.Module;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{Kind} {ManagedName}")]
    public class ManagedTypeInfo : ITypeInfo, IEquatable<ManagedTypeInfo>
    {
        private readonly Type m_type;

        public string ManagedName { get; }

        public string ManagedSourceName { get; }

        public string NativeName { get; }

        public ITypeInfo? ParentType { get; }

        public Type? ManagedType => m_type;
        
        public INamedTypeSymbol? TypeSymbol => null;

        public bool IsManagedUObject => false;

        public string Namespace => m_type.Namespace!;

        public string Header { get; }

        public TypeKind Kind { get; }
        
        public Module Module { get; }

        public NativeTransferType TypicalArgumentType { get; }

        public ITypeMarshaller? DefaultMarshaller { get; }
        public string NativeModule => "";

        public bool IsGenericType => false;

        private ManagedTypeInfo(Type type)
        {
            m_type = type;

            // TODO: Handle name conversion, non-U* objects should not be translatable at all, except for primitive types.

            // TODO: Probably redo this with a special overload instead.
            // Built-in type specifics.
            if (CompatiblePrimitiveTypes.TryGetValue(type, out var names))
            {
                ManagedName = names.ManagedName;
                NativeName = names.NativeName;
                ManagedSourceName = ManagedName;
            }
            else
            {
                ManagedSourceName = ManagedName = NativeName = type.Name;
            }

            // Collect type kind.
            if (type.IsValueType)
                Kind = type.IsEnum ? TypeKind.Enum : TypeKind.Struct;
            else
                Kind = type.IsInterface ? TypeKind.Interface : TypeKind.Class;

            // Base class.
            if (!type.IsValueType && type.BaseType != null && type.BaseType != typeof(object))
                ParentType = GetType(type.BaseType);

            // Marshaller
            if (type.GetCustomAttribute<MarshallerAttribute>() is { } ms)
                DefaultMarshaller = (ITypeMarshaller?) Activator.CreateInstance(ms.Marshaller);

            // Header.
            if (type.GetCustomAttribute<HeaderAttribute>() is { } header)
                Header = header.Path;
            else
                Header = "";

            if (type.IsValueType)
                TypicalArgumentType = NativeTransferType.ByValue;
            else
                TypicalArgumentType = NativeTransferType.ByPointer;

            Module = Module.GetModule(type.Module);
        }

        #region Static

        /// <summary>
        /// Special names for built-in types.
        /// </summary>
        private static readonly ImmutableDictionary<Type, (string ManagedName, string NativeName)> CompatiblePrimitiveTypes;

        /// <summary>
        /// All known types.
        /// </summary>
        private static readonly Dictionary<Type, ManagedTypeInfo> Types = new();

        /// <summary>
        /// Built in types.
        /// </summary>
        public static readonly ImmutableDictionary<Type, ManagedTypeInfo> BuiltInTypes;

        /// <summary>
        /// Void type info.
        /// </summary>
        public static readonly ManagedTypeInfo Void;

        static ManagedTypeInfo()
        {
            // Make built-in type mappings.
            CompatiblePrimitiveTypes = new Dictionary<Type, (string ManagedName, string NativeName)>()
            {
                {typeof(void), ("void", "void")},

                {typeof(bool), ("bool", "bool")},

                // wchar_t can be anywhere from 1 to 4 bytes, whereas char is always 2 bytes.
                // {typeof(char), ("char", "UCS2CHAR")}, // Not supported.

                {typeof(sbyte), ("sbyte", "int8")},
                {typeof(byte), ("byte", "uint8")},
                {typeof(short), ("short", "int16")},
                {typeof(ushort), ("ushort", "uint16")},
                {typeof(int), ("int", "int32")},
                {typeof(uint), ("uint", "uint32")},
                {typeof(long), ("long", "int64")},
                {typeof(ulong), ("ulong", "uint64")},

                {typeof(double), ("double", "double")},
                {typeof(float), ("float", "float")},
                {typeof(IntPtr), ("IntPtr", "void*")},
                {typeof(UIntPtr), ("IntPtr", "size_t")}, 
                //{typeof(decimal), ("decimal", "decimal")}, // Not supported.

                // {typeof(string), ("string", "UCS2CHAR*")}, // Not supported
            }.ToImmutableDictionary();

            Void = new(typeof(void));

            // Make cached built-in types.
            BuiltInTypes = CompatiblePrimitiveTypes.Keys
                .Select(x => new ManagedTypeInfo(x))
                .ToImmutableDictionary(x => x.ManagedType!);
        }

        /// <summary>
        /// Get type information for a managed type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ITypeInfo GetType(Type type)
        {
            if (!Types.TryGetValue(type, out var info))
                Types[type] = info = new ManagedTypeInfo(type);

            return info;
        }

        /// <summary>
        /// Get type information for a managed type.
        /// </summary>
        /// <returns></returns>
        public static ITypeInfo GetType<T>()
        {
            return GetType(typeof(T));
        }

        #endregion

        #region Equality

        public bool Equals(ManagedTypeInfo? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return m_type == other.m_type;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((ManagedTypeInfo) obj);
        }

        public override int GetHashCode()
        {
            return m_type.GetHashCode();
        }

        public static bool operator ==(ManagedTypeInfo? left, ManagedTypeInfo? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ManagedTypeInfo? left, ManagedTypeInfo? right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}