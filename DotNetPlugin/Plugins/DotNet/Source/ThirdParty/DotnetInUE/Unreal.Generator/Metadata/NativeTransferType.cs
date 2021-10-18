// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Metadata
{
    /// <summary>
    /// Encodes a simplified native type transfer type.
    /// </summary>
    public enum NativeTransferType
    {
        ByValue = 0,
        ByReference = 1,
        ByPointer = 2,

        /// <summary>Flag indicating that the type is const.</summary>
        Const = 1 << 2
    }
}