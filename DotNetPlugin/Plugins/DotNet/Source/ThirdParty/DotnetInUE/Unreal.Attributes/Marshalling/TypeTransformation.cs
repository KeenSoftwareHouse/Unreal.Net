// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Marshalling
{
    /// <summary>
    /// A transformation to apply to a type.
    /// </summary>
    public enum TypeTransformation
    {
        /// <summary>No transformation.</summary>
        None,

        /// <summary>Convert to a pointer type.</summary>
        Pointer,

        /// <summary>Convert to an opaque pointer.</summary>
        OpaquePointer
    }
}