// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Unreal.Metadata;
using Unreal.Util;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Handles property type resolution by their raw type. 
    /// </summary>
    public class CustomManagedTypeResolver : ManagedTypeResolver
    {
        private readonly Dictionary<ITypeSymbol, CustomTypeMapping> m_mappingPerType =
            new(SymbolEqualityComparer.Default);

        public override ITypeInfo? Resolve(ITypeSymbol managedType)
        {
            if (managedType is not INamedTypeSymbol namedSymbol)
                return null;

            var symbol = namedSymbol.IsGenericType ? namedSymbol.ConstructUnboundGenericType() : namedSymbol;

            if (!m_mappingPerType.TryGetValue(symbol, out var mapping))
                return null;

            // Always use concrete symbol here.
            return mapping.Get(namedSymbol);
        }

        public void AddMapping(CustomTypeMapping mapping)
        {
            if (m_mappingPerType.ContainsKey(mapping.TypeSymbol))
                throw new InvalidOperationException(
                    $"Another mapping is already registered for native type {mapping.TypeSymbol.GetFullName()}.");

            m_mappingPerType.Add(mapping.TypeSymbol, mapping);
        }
    }
}