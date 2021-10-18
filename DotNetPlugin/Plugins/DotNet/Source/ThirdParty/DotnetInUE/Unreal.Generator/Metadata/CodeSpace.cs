// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Metadata
{
    /// <summary>
    /// Environment where a given symbol is implemented.
    /// </summary>
    public enum Codespace
    {
        /// <summary>Native machine code.</summary>
        /// <remarks>In our case this is usually sourced from C++.</remarks>
        Native,

        /// <summary>Managed CLR code.</summary>
        /// <remarks>In our case this is usually sourced from C#.</remarks> 
        Managed
    }
}