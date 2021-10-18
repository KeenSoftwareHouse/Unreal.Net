// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;

namespace Unreal
{
    /// <summary>
    /// Attribute used to state what native modules are implemented in a given native assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Module)]
    public class NativeModulesAttribute : Attribute
    {
        public readonly ImmutableArray<string> Modules;

        public NativeModulesAttribute(params string[] modules)
        {
            Modules = modules.ToImmutableArray();
        }
    }
}