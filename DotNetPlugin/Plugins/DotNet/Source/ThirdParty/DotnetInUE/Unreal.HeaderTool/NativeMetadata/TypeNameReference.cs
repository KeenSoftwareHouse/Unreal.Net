// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public class TypeNameReference : TypeReferenceBase
    {
        public TypeNameReference()
        {
            FieldType = TypeReferenceKind.TypeName;
        }

        public string Module { get; set; }

        public string Name { get; set; }

        public string CppName { get; set; }

        public override string GetPrettyType()
        {
            return CppName;
        }
    }
}