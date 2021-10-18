// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using JetBrains.Annotations;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Enum), MeansImplicitUse]
    public class UEnumAttribute : MetaAttributeBase
    {
        public override MetaType Type => MetaType.Enum;
    }
}