// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public enum UETypeKind : byte
    {
        None,
        UPackage,
        UObject,
        UInterface,
        UStruct,
        UEnum,
        UProperty,
        UFunction,
        UDelegate,
    };
}