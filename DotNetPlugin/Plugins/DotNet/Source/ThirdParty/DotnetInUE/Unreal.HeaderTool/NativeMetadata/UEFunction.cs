// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Unreal.NativeMetadata
{
    public class UEFunction : UEField
    {
        public FunctionFlags Flags { get; set; }
        public UEProperty? Return { get; set; }
        public List<UEProperty> Parameters { get; set; } = new();

        [JsonIgnore]
        public UEClass Class { get; set; }

        public UEProperty GetReturn() => Return ?? UEProperty.Void;
    }
}