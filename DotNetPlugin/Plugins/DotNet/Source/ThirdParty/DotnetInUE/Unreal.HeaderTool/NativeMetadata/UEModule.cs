// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public class UEModule : UEMeta
    {
        public string LongName { get; set; } = "";

        public string Folder { get; set; } = "";

        public string File { get; set; } = "";

        public BuildModuleType PackageType { get; set; }
    }
}