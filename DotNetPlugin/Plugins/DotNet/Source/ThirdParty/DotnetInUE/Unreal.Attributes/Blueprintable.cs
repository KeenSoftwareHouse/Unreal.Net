// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BlueprintableAttribute : MetaPropertyAttributeBase
    {
        public override MetaTypeFlags ValidTargets => MetaTypeFlags.Class;
    }
}