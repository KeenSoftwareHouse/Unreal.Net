// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using JetBrains.Annotations;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
    public class UFunctionAttribute : MetaAttributeBase, IExportedFunctionAttribute
    {
        public override MetaType Type => MetaType.Function;
    }
}