// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Marshalling
{
    /// <summary>
    /// An intermediate type used by marshalling.
    /// </summary>
    public readonly struct IntermediateType
    {
        /// <summary>
        /// Base managed type.
        /// </summary>
        public readonly Type BaseType;

        /// <summary>
        /// A transformation to apply to the type.
        /// </summary>
        public readonly TypeTransformation Transformation;

        public IntermediateType(Type baseType, TypeTransformation transformation)
        {
            BaseType = baseType;
            Transformation = transformation;
        }
    }
}