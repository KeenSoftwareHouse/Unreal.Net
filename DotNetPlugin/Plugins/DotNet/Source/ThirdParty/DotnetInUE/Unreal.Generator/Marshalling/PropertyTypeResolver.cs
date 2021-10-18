// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Unreal.Metadata;
using Unreal.NativeMetadata;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Interface used to resolve native property metadata into a <see cref="ITypeInfo"/> instance. 
    /// </summary>
    public abstract class PropertyTypeResolver : TypeResolverBase
    {
        public abstract ITypeInfo? Resolve(UEProperty property);
    }
}