// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal
{
    /// <summary>
    /// Base class for the primary attributes defining UE4 metadata.
    /// </summary>
    public abstract class MetaAttributeBase : UnrealAttribute
    {
        public abstract MetaType Type { get; }
    }
}