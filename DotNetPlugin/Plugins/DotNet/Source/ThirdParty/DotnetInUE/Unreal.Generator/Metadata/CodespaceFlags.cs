// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Metadata
{
    /// <summary>
    /// Environment where a given symbol is implemented.
    /// </summary>
    [Flags]
    public enum CodespaceFlags
    {
        /// <summary>Native machine code.</summary>
        /// <remarks>In our case this is usually sourced from C++.</remarks>
        Native = 1 << Codespace.Native,

        /// <summary>Managed CLR code.</summary>
        /// <remarks>In our case this is usually sourced from C#.</remarks> 
        Managed = 1 << Codespace.Managed,
        
        All = Native | Managed
    }
}