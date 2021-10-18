// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public class UEEnumValue
    {
        public UEEnumValue(string name, long value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public long Value { get; set; }
    }
}