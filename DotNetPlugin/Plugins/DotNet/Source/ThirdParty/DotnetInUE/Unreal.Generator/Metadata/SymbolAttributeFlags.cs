// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Metadata
{
    [Flags]
    public enum SymbolAttributeFlags
    {
        Readonly = 1 << SymbolAttribute.Readonly,
        New = 1 << SymbolAttribute.New,
        Unsafe = 1 << SymbolAttribute.Unsafe,
        Static = 1 << SymbolAttribute.Static,
        Virtual = 1 << SymbolAttribute.Virtual,
        Override = 1 << SymbolAttribute.Override,
        Final = 1 << SymbolAttribute.Final,
    }
}