// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;

namespace Unreal
{
    /// <summary>
    /// Attribute used to list the marshallers that are defined in a module.
    /// </summary>
    /// <remarks>This attribute is added by the generator to modules that define custom marshallers to speed up their lookup.</remarks>
    [AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
    public class NativeTypeMappingsAttribute : Attribute
    {
        public readonly ImmutableArray<Type> Types;

        public NativeTypeMappingsAttribute(params Type[] types)
        {
            Types = types.ToImmutableArray();
        }
    }
}