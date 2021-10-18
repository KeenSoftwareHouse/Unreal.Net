// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Metadata
{
    /// <summary>
    /// The shape of a type.
    /// </summary>
    /// <remarks>
    /// The shape defines a shared native and managed contract for the definitions of various types. 
    /// </remarks>
    public enum TypeKind
    {
        /// <summary>Heap only complex object</summary>
        Class,

        /// <summary>Lightweight stack based object.</summary>
        Struct,

        /// <summary>Interface.</summary>
        Interface,

        /// <summary>Enum</summary>
        Enum
    }
}