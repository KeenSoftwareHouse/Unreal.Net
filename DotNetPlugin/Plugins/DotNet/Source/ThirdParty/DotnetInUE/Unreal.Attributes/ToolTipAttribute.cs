// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class ToolTipAttribute : MetaPropertyAttributeBase
    {
        public readonly string Value;

        public ToolTipAttribute(string value)
        {
            Value = value;
        }

        public override MetaTypeFlags ValidTargets => MetaTypeFlags.Enum
                                                      | MetaTypeFlags.EnumMeta
                                                      | MetaTypeFlags.Struct
                                                      | MetaTypeFlags.Class
                                                      | MetaTypeFlags.Function;
    }
}