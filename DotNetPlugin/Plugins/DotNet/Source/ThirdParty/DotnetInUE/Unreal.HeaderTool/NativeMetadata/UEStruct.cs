// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.NativeMetadata
{
    public class UEStruct : UEField
    {
        public TypeNameReference? Parent { get; set; }
        public string CppName { get; set; } = "";

        /// <summary>
        /// Properties of the struct.
        /// </summary>
        public List<UEProperty> Properties { get; set; } = new();

        /// <summary>
        /// Byte size of the struct.
        /// </summary>
        public int Size { get; set; }
    }
}