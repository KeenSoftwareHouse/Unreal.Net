// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Default managed type resolver.
    /// </summary>
    /// <remarks>
    /// Allows resolution of allowed primitive types only.
    /// </remarks>
    public class DefaultManagedTypeResolver : ManagedTypeResolver
    {
        // TODO: Re-evaluate this type, currently we cache all built-in types at the start, so there is no real chance we'd get here.
        // Technically we can just return null.
        public override ITypeInfo? Resolve(ITypeSymbol managedType)
        {
            if (managedType is not INamedTypeSymbol nameSymbol)
                return null; // Non-named types (such as arrays) are not allowed on the boundary.

            if (!Container.Context.TryGetSymbolTypeInfo(nameSymbol, out var info) // Get type.
                || info!.ManagedType == null // Ensure it's managed
                || !ManagedTypeInfo.BuiltInTypes.ContainsKey(info.ManagedType)) // Ensure it's in the primitive table.
                return null;
            
            return info;
        }
    }
}