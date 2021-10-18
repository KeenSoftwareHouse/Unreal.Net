// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.NativeMetadata
{
    public class UEClass : UEStruct
    {
        public List<string> Interfaces { get; set;  } = new();
        public List<UEFunction> Functions { get; set;  } = new();

        public ClassFlags Flags { get; set; } = 0;
    }
}