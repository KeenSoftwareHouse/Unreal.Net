// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Unreal.Core;

namespace Unreal.CoreUObject
{
    public partial class UObject : UObjectBase
    {
        public static TClass NewObject<TClass>(UClass uClass, UObject outer)
            where TClass : UObject
        {
            return UObjectUtil.Create<TClass>(UObjectUtil.GetNativeInstance(uClass), uClass.Implementation,
                UObjectUtil.GetNativeInstance(outer));
        }

        public static TClass NewObject<TClass>(UObject outer)
            where TClass : UObject
        {
            var typeData = UObjectReflection.Instance.GetTypeData(typeof(TClass));

            return UObjectUtil.Create<TClass>(typeData.NativeUClass, typeData.Implementation,
                UObjectUtil.GetNativeInstance(outer));
        }
    }
}