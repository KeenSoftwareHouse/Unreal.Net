// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public enum BuildModuleType
    {
        Program,
        EngineRuntime,
        EngineUncooked,
        EngineDeveloper,
        EngineEditor,
        EngineThirdParty,
        GameRuntime,
        GameUncooked,
        GameDeveloper,
        GameEditor,
        GameThirdParty,
        // NOTE: If you add a new value, make sure to update the ToString() method below!

        Max
    };
}