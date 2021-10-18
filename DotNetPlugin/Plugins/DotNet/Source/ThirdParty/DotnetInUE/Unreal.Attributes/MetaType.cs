// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal
{
    /// <summary>
    /// UE4 Metadata types.
    /// </summary>
    public enum MetaType
    {
        Class,
        Enum,
        EnumValue, // In UE4 this is just UMETA, but it's really only used for enum values.
        Interface,
        Struct,
        Function,
        Property,
    }
}