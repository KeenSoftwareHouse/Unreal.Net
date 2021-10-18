// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    /// <summary>
    /// UE4 Metadata types.
    /// </summary>
    [Flags]
    public enum MetaTypeFlags
    {
        Class = 1 << MetaType.Class,
        Enum = 1 << MetaType.Enum,
        EnumMeta = 1 << MetaType.EnumValue,
        Interface = 1 << MetaType.Interface,
        Struct = 1 << MetaType.Struct,
        Function = 1 << MetaType.Function,
        Property = 1 << MetaType.Property
    }
}