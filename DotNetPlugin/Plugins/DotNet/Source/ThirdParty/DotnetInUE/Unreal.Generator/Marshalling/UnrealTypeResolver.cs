// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Type resolver for Unreal types 
    /// </summary>
    public class UnrealTypeResolver : ManagedTypeResolver
    {
        public override ITypeInfo? Resolve(ITypeSymbol managedType)
        {
            if (managedType is not INamedTypeSymbol namedSymbol)
                return null;

            // Attributes are what selects this resolver, so no need to check.
            return new SymbolTypeInfo(namedSymbol, Container.Context);
        }
    }
}