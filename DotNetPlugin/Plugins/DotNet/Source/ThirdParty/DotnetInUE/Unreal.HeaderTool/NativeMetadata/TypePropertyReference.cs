// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.NativeMetadata
{
    public class TypePropertyReference : TypeReferenceBase
    {
        public TypePropertyReference()
        {
            FieldType = TypeReferenceKind.Property;
        }

        public UEProperty Property { get; set; }

        public override string GetPrettyType()
        {
            return Property.GetPrettyType();
        }
    }
}