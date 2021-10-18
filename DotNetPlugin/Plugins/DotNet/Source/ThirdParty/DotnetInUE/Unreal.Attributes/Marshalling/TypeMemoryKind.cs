// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Marshalling
{
    public enum TypeMemoryKind
    {
        /// <summary>
        /// A value type, respects type transfer semantics of the call site.
        /// </summary>
        /// <remarks>The managed representation of the type must be a value type.</remarks>
        ValueType,

        /// <summary>
        /// A larger or more complex value type. Always pass by value, regardless of call site specification.
        /// </summary>
        /// <remarks>The managed representation of the type must be a value type.</remarks>
        ValueTypeByReference,

        /// <summary>
        /// A reference type, always passed as a pointer.
        /// </summary>
        /// <remarks>The managed representation of the type must be a reference type.</remarks>
        ReferenceType
    }
}