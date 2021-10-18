// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class CategoryAttribute : MetaPropertyAttributeBase
    {
        public readonly string Value;

        public CategoryAttribute(string value)
        {
            Value = value;
        }

        public override MetaTypeFlags ValidTargets => MetaTypeFlags.Function;
    }
}