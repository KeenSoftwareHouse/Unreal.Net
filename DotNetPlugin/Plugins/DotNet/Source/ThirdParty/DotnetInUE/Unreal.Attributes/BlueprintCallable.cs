// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BlueprintCallableAttribute : MetaPropertyAttributeBase
    {
        public override MetaTypeFlags ValidTargets => MetaTypeFlags.Function;
    }
}