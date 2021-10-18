// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using JetBrains.Annotations;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public class UClassAttribute : MetaAttributeBase
    {
        public override MetaType Type => MetaType.Class;
    }
}