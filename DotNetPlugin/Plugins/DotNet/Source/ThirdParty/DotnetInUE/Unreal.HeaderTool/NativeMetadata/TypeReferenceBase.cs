// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public abstract class TypeReferenceBase
    {
        public TypeReferenceKind FieldType { get; set; }

        public abstract string GetPrettyType();
    }
}