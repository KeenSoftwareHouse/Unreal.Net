// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Marks a managed type as the representation of a native type.
    /// </summary>
    /// <remarks>
    /// Types marked with this attribute should nearly always also define a <see cref="MarshalFormatsAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NativeTypeAttribute : Attribute
    {
        /// <summary>
        /// The name of the native type this mapping refers to.
        /// </summary>
        /// <remarks>For </remarks>
        public string NativeTypeName;

        /// <summary>
        /// Native property type this marshalling applies to.
        /// </summary>
        public string PropertyType;

        /// <summary>
        /// Memory kind of the type.
        /// </summary>
        public readonly TypeMemoryKind MemoryKind;

        /// <summary>
        /// Intermediate result type after the native/managed representations are marshalled. 
        /// </summary>
        public IntermediateType IntermediateType;

        public NativeTypeAttribute(string nativeTypeName, string propertyType, TypeMemoryKind memoryKind,
            Type baseIntermediateType, TypeTransformation transformation = TypeTransformation.None)
        {
            NativeTypeName = nativeTypeName;
            PropertyType = propertyType;
            MemoryKind = memoryKind;
            IntermediateType = new IntermediateType(baseIntermediateType, transformation);
        }
    }
}