// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Unreal.Metadata;
using Unreal.NativeMetadata;

namespace Unreal.Marshalling
{
    public class DefaultPropertyTypeResolver : PropertyTypeResolver
    {
        public static readonly DefaultPropertyTypeResolver Instance = new();

        public override ITypeInfo? Resolve(UEProperty property)
        {
            QualifiedNativeTypeName typeName;
            if (property.Type is { } t)
                typeName = new QualifiedNativeTypeName(t.Module, t.CppName);
            else
                typeName = new QualifiedNativeTypeName("", property.RawType);

            Container.Context.TryGetNativeTypeInfo(typeName, out var typeInfo);
            return typeInfo;
        }
    }
}