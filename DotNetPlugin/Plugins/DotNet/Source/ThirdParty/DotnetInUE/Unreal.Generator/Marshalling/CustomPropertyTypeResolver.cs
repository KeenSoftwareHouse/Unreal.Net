// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Unreal.Metadata;
using Unreal.NativeMetadata;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Handles property type resolution by their raw type. 
    /// </summary>
    public class CustomPropertyTypeResolver : PropertyTypeResolver
    {
        private readonly Dictionary<string, CustomTypeMapping> m_perTypeFactory = new();

        public override ITypeInfo? Resolve(UEProperty property)
        {
            if (m_perTypeFactory.TryGetValue(property.GetCleanRawType(), out var factory))
                return factory.Get(property);

            return null;
        }

        public void AddMapping(CustomTypeMapping mapping)
        {
            if (m_perTypeFactory.ContainsKey(mapping.NativeTypeName))
                throw new InvalidOperationException(
                    $"Another mapping is already registered for native type {mapping.NativeTypeName}.");

            m_perTypeFactory.Add(mapping.NativeTypeName, mapping);
        }
    }
}