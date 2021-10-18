// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.NativeMetadata
{
    public class UEEnum : UEField
    {
        public UEnumCppForm Form { get; set; }
        public string CppName { get; set; } = "";
        
        public bool IsFlags { get; set; }
        public long MaxValue { get; set; }
        public List<UEEnumValue> Values { get; set; } = new();
    }
}