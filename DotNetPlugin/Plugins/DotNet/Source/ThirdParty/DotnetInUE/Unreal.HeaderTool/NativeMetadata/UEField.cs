// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.NativeMetadata
{
    public class UEField : UEMeta
    {
        public Dictionary<string, string> Meta { get; set; } = new();
        
        public string Module { get; set; } = "";
    }
}