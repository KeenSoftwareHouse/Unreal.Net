// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class ShortToolTip : UnrealAttribute
    {
        public readonly string Value;

        public ShortToolTip(string value)
        {
            Value = value;
        }
    }
}