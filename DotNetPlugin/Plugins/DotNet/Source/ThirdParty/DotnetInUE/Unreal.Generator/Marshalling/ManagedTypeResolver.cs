// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Resolve a managed type by it's attribute.
    /// </summary>
    public abstract class ManagedTypeResolver : TypeResolverBase
    {
        public abstract ITypeInfo? Resolve(ITypeSymbol managedType);
    }
}