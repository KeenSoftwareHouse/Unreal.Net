// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Struct
                    | AttributeTargets.Interface
                    | AttributeTargets.Enum)]
    public class ManagedTypeAttribute : MetaPropertyAttributeBase
    {
        public override MetaTypeFlags ValidTargets => MetaTypeFlags.Class | MetaTypeFlags.Struct;

        public override bool IsMeta => true;
    }
}