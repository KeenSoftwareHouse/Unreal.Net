// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public class NativeTypeAttributeData
    {
        /// <summary>
        /// The name of the native type this mapping refers to.
        /// </summary>
        /// <remarks>For </remarks>
        public readonly string NativeTypeName;

        /// <summary>
        /// Native property type this marshalling applies to.
        /// </summary>
        public readonly string PropertyType;

        /// <summary>
        /// Memory kind of the type.
        /// </summary>
        public readonly TypeMemoryKind MemoryKind;

        /// <summary>
        /// Base intermediate result type after the native/managed representations are marshalled. 
        /// </summary>
        public readonly INamedTypeSymbol BaseIntermediateType;

        /// <summary>
        /// Transformation to apply to the intermediate type.
        /// </summary>
        public readonly TypeTransformation Transformation;

        public NativeTypeAttributeData(string nativeTypeName, string propertyType, TypeMemoryKind memoryKind,
            INamedTypeSymbol baseIntermediateType, TypeTransformation transformation)
        {
            NativeTypeName = nativeTypeName;
            PropertyType = propertyType;
            MemoryKind = memoryKind;
            BaseIntermediateType = baseIntermediateType;
            Transformation = transformation;
        }

        public ITypeInfo GetTransformedType(GenerationContext context)
        {
            var @base = context.TypeResolver.Resolve(BaseIntermediateType);

            return Transformation switch
            {
                TypeTransformation.None => @base,
                TypeTransformation.Pointer => PointerType.Get(@base),
                TypeTransformation.OpaquePointer => OpaquePointerType.Get(@base),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}