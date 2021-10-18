// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Attributes.Meta
{
    public readonly struct MetaAttributeInfo
    {
        public readonly Type MetaAttributeType;

        public readonly MetaType MetaType;

        public string UE4Name => MetaType.GetNativeName();

        public MetaAttributeInfo(Type type)
        {
            MetaAttributeType = type;

            var attr = (MetaAttributeBase) Activator.CreateInstance(type)!;
            MetaType = attr.Type;
        }
    }
}