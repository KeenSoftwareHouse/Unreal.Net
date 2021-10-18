// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.ErrorHandling;
using Unreal.NativeMetadata;

namespace Unreal.Metadata
{
    public partial class PropertyDefinition
    {
        

        public static PropertyDefinitionBuilder PrepareFromNative(TypeDefinition enclosingType, QualifiedTypeReference type, UEProperty property)
        {
            var builder = CreateBuilder(enclosingType, property.Name);

            if (property.Meta.TryGetValue("Comment", out var doc))
                builder.WithDocumentationFromNative(doc);

            builder.WithType(type)
                .AppendComment($"PropType = {property.PropertyType}")
                .AppendComment($"Flags = {property.Flags}")
                .WithManagedAttribute($"FieldOffset({property.Offset})");

            return builder;
        }
    }
}