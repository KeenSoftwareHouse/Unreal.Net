// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using JetBrains.Annotations;

namespace Unreal
{
    // TODO: Investigate UStructs, in C++ structs can inherit, but not in C#.
    [AttributeUsage(AttributeTargets.Struct), MeansImplicitUse]
    public class UStructAttribute : MetaAttributeBase
    {
        public override MetaType Type => MetaType.Struct;
    }
}