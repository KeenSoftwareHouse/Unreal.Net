// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Unreal.Attributes.Meta;
using Unreal.ErrorHandling;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class GenerationContext
    {
        /// <summary> Types by Native name. </summary>
        private readonly Dictionary<QualifiedNativeTypeName, ITypeInfo> m_infoByNativeName = new();

        private readonly Dictionary<Type, ITypeInfo> m_infoPerManagedType = new();

        private readonly Dictionary<Type, INamedTypeSymbol> m_symbolPerManagedType = new();

        private readonly Dictionary<ITypeSymbol, ITypeInfo> m_infoPerSymbol = new(SymbolEqualityComparer.Default);

        public readonly AttributeIndex MetaAttributes;

        public readonly Compilation Compilation;

        #pragma warning disable RS1024 // Compare symbols correctly
        private readonly Dictionary<ITypeSymbol, PropertyAttributeInfo> m_knownMetaAttributes =
            new(SymbolEqualityComparer.Default);
        #pragma warning restore RS1024 // Compare symbols correctly

        public readonly INamedTypeSymbol UClassAttribute;
        public readonly INamedTypeSymbol UEnumAttribute;
        public readonly INamedTypeSymbol UInterfaceAttribute;
        public readonly INamedTypeSymbol UStructAttribute;
        public readonly INamedTypeSymbol UFunctionAttribute;
        public readonly INamedTypeSymbol UPropertyAttribute;

        public readonly INamedTypeSymbol ManagedTypeAttribute;

        public readonly INamedTypeSymbol ConstructorAttribute;
        public readonly INamedTypeSymbol DestructorAttribute;

        public readonly INamedTypeSymbol ExportedFunctionAttribute;

        public readonly INamedTypeSymbol NativeTypeAttribute;
        public readonly INamedTypeSymbol MarshalFormatsAttribute;
        public readonly INamedTypeSymbol NativeModulesAttribute;
        public readonly INamedTypeSymbol NativeTypeMappingsAttribute;

        /// <summary>
        /// Modules containing native types.
        /// </summary>
        private Dictionary<string, Module> m_nativeModules = new();

        public readonly TypeResolver TypeResolver;

        public GenerationContext(GeneratorExecutionContext executionContext)
        {
            Compilation = executionContext.Compilation;

            try
            {
                MetaAttributes = new AttributeIndex();
            }
            catch (AttributeFormatException ate)
            {
                // Pass message onwards.
                throw new MetadataException(ate.Message);
            }

            // Load meta properties.
            foreach (var value in MetaAttributes.MetaAttributeProperties.Values)
            {
                var symbol = LoadSymbol(value.AttributeType);

                m_knownMetaAttributes[symbol] = value;
            }

            // Load attributes.
            UClassAttribute = LoadSymbol(MetaAttributes.UClassAttributeType.MetaAttributeType);
            UEnumAttribute = LoadSymbol(MetaAttributes.UEnumAttributeType.MetaAttributeType);
            UInterfaceAttribute = LoadSymbol(MetaAttributes.UInterfaceAttributeType.MetaAttributeType);
            UStructAttribute = LoadSymbol(MetaAttributes.UStructAttributeType.MetaAttributeType);
            UFunctionAttribute = LoadSymbol(MetaAttributes.UFunctionAttributeType.MetaAttributeType);
            UPropertyAttribute = LoadSymbol(MetaAttributes.UPropertyAttributeType.MetaAttributeType);

            ManagedTypeAttribute = LoadSymbol(typeof(ManagedTypeAttribute));
            
            ConstructorAttribute = LoadSymbol(typeof(ConstructorAttribute));
            DestructorAttribute = LoadSymbol(typeof(DestructorAttribute));

            ExportedFunctionAttribute = LoadSymbol(typeof(IExportedFunctionAttribute));

            NativeTypeAttribute = LoadSymbol(typeof(NativeTypeAttribute));
            MarshalFormatsAttribute = LoadSymbol(typeof(MarshalFormatsAttribute));
            NativeModulesAttribute = LoadSymbol(typeof(NativeModulesAttribute));
            NativeTypeMappingsAttribute = LoadSymbol(typeof(NativeTypeMappingsAttribute));

            // Load attribute types.
            foreach (var type in MetaAttributes.AllAttributes)
                m_symbolPerManagedType[type] = LoadSymbol(type);

            // Load built in types.
            foreach (var type in ManagedTypeInfo.BuiltInTypes)
            {
                var symbol = LoadSymbol(type.Key);
                RegisterType(type.Value);

                // Manually register symbol
                m_infoPerSymbol[symbol] = type.Value;
            }

            // Prepare type resolver.
            TypeResolver = new TypeResolver(this);

            // Register the Unreal resolver for all built-in attributes.
            var ueTypeResolver = new UnrealTypeResolver();
            TypeResolver.RegisterManagedResolver(UClassAttribute, ueTypeResolver);
            TypeResolver.RegisterManagedResolver(UEnumAttribute, ueTypeResolver);
            TypeResolver.RegisterManagedResolver(UInterfaceAttribute, ueTypeResolver);
            TypeResolver.RegisterManagedResolver(UStructAttribute, ueTypeResolver);
        }

        public INamedTypeSymbol LoadSymbol(Type type)
        {
            return LoadSymbol(type.FullName!);
        }

        public INamedTypeSymbol LoadSymbol(string metadataName)
        {
            var symbol = Compilation.GetTypeByMetadataName(metadataName);
            if (symbol == null)
                throw new MetadataException($"Metadata symbol for type {metadataName} could not be found.");

            return symbol;
        }

        public INamedTypeSymbol GetSymbol(Type type)
        {
            if (!m_symbolPerManagedType.TryGetValue(type, out var symbol))
                m_symbolPerManagedType[type] = symbol = LoadSymbol(type);

            return symbol;
        }

        public INamedTypeSymbol GetSymbol<T>()
        {
            return GetSymbol(typeof(T));
        }

        public bool TryGetSymbolTypeInfo(ITypeSymbol type, out ITypeInfo? info)
        {
            return m_infoPerSymbol.TryGetValue(type, out info);
        }
        
        public bool TryGetNativeTypeInfo(QualifiedNativeTypeName name, out ITypeInfo? info)
        {
            return m_infoByNativeName.TryGetValue(name, out info);
        }

        public bool TryGetManagedTypeInfo(Type type, out ITypeInfo? info)
        {
            return m_infoPerManagedType.TryGetValue(type, out info);
        }

        /// <summary>
        /// Get type info for a type or throw exception if the type is unknown.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="MissingSymbolException">If the type cannot be found.</exception>
        public ITypeInfo GetNativeTypeInfo(QualifiedNativeTypeName type)
        {
            if (!TryGetNativeTypeInfo(type, out var typeInfo))
                throw new MissingSymbolException(type.ToString());

            return typeInfo!;
        }

        /// <summary>
        /// Get type info for a type or throw exception if the type is unknown.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="MissingSymbolException">If the type cannot be found.</exception>
        [Obsolete("Type info by name should no longer be used.")]
        public ITypeInfo GetMetadataTypeInfo(string typeName)
        {
            var symbol = LoadSymbol(typeName);

            return TypeResolver.Resolve(symbol);
        }

        /// <summary>
        /// Get type info for a type or throw exception if the type is unknown.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="MissingSymbolException">If the type cannot be found.</exception>
        public ITypeInfo GetManagedTypeInfo(Type type)
        {
            if (!TryGetManagedTypeInfo(type, out var typeInfo))
                throw new MissingSymbolException(type.FullName!);

            return typeInfo!;
        }

        /// <summary>
        /// Get type info for a type or throw exception if the type is unknown.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MissingSymbolException">If the type cannot be found.</exception>
        public ITypeInfo GetManagedTypeInfo<T>()
        {
            if (!TryGetManagedTypeInfo(typeof(T), out var typeInfo))
                throw new MissingSymbolException(typeof(T).FullName!);

            return typeInfo!;
        }

        internal bool TryGetAttribute(ITypeSymbol symbol, out PropertyAttributeInfo attribute)
        {
            return m_knownMetaAttributes.TryGetValue(symbol, out attribute);
        }

        public void RegisterType(ITypeInfo type)
        {
            m_infoByNativeName.Add(type.GetNativeFullName(), type);

            if (type.ManagedType != null)
                m_infoPerManagedType.Add(type.ManagedType, type);

            if (type.TypeSymbol != null)
                m_infoPerSymbol.Add(type.TypeSymbol, type);
        }

        /// <summary>
        /// Get type info for a symbol or throw exception if the type is unknown.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="SourceException"></exception>
        public ITypeInfo ResolveSymbol(SyntaxNode? source, ISymbol symbol)
        {
            if (symbol is not ITypeSymbol typeSymbol)
                throw new SourceException("Symbol is not a type.", source);

            // Caching is handled by the resolver, just let it be.
            return TypeResolver.Resolve(typeSymbol, source);
        }

        /// <summary>
        /// Construct a meta attribute from an expected name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="MetadataException"></exception>
        public MetaAttribute GetMetaAttribute(string name, object? value = null)
        {
            if (!MetaAttributes.MetaAttributeProperties.TryGetValue(name, out var info))
                throw new MetadataException("No meta property attribute named '{name}' is known.");

            if (value != null)
            {
                if (!info.HasArgument)
                    throw new MetadataException("Attribute '{name}' does not accept arguments.");

                if (!info.ArgumentType!.IsInstanceOfType(value))
                    throw new MetadataException(
                        $"Attribute '{name}': argument of type {value.GetType().FullName} is not compatible with the expected argument type {info.ArgumentType.FullName}.");
            }

            return new MetaAttribute(name, value, info.IsMeta);
        }

        private static class TypeCache<TKey, TValue>
            where TKey : notnull
            where TValue : notnull
        {
            private static Dictionary<TKey, TValue> m_index = new();

            public static TValue GetOrAdd(TKey key, Func<TKey, TValue> creator)
            {
                if (!m_index.TryGetValue(key, out var value))
                    m_index[key] = value = creator(key);

                return value;
            }

            public static void EnsureCached(TKey key, TValue value)
            {
                if (!m_index.TryGetValue(key, out var existing))
                {
                    m_index[key] = value;
                }
                else if (!value.Equals(existing))
                {
                    throw new InvalidOperationException(
                        $"Value '{value}' under key '{key}' is not the same as the already cached value '{existing}'.");
                }
            }
        }

        /// <summary>
        /// Get or create a value with caching.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="creator">A function used to create instances of <typeparamref name="TValue"/> when not cached.</param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public TValue GetCached<TKey, TValue>(TKey key, Func<TKey, TValue> creator)
            where TKey : notnull
            where TValue : notnull
        {
            return TypeCache<TKey, TValue>.GetOrAdd(key, creator);
        }

        /// <summary>
        /// Ensure a value is cached.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value for the key.</param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <exception cref="InvalidOperationException">When the value is already cached and is not the same as the provided.</exception>
        public void EnsureCached<TKey, TValue>(TKey key, TValue value)
            where TKey : notnull
            where TValue : notnull
        {
            TypeCache<TKey, TValue>.EnsureCached(key, value);
        }
    }
}