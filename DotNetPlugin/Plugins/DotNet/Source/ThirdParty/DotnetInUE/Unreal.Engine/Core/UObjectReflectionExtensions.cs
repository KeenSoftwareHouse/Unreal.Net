// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.CoreUObject;

namespace Unreal.Core
{
    public static class UObjectReflectionExtensions
    {
        /// <summary>
        /// Get reflection data for an object instance.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static ReflectionData GetClassData(this UObjectReflection self, UClass instance)
        {
            return (ReflectionData) self.GetTypeData(UObjectUtil.GetNativeInstance(instance));
        }
        
        /// <summary>
        /// Get reflection data for an object instance.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static ReflectionData GetBestFitType(this UObjectReflection self, UClass instance)
        {
            return (ReflectionData) self.GetBestFitType(UObjectUtil.GetNativeInstance(instance));
        }

        /// <summary>
        /// Get reflection data for an object instance.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static ReflectionData GetTypeData(this UObjectReflection self, UObject instance)
        {
            return (ReflectionData) self.GetTypeData(UObjectUtil.GetUClass(instance));
        }

        /// <summary>
        /// Get reflection data for a managed type.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ReflectionData GetTypeData<TObject>(this UObjectReflection self)
            where TObject : UObject
        {
            return (ReflectionData) self.GetTypeData(typeof(TObject));
        }

        /// <summary>
        /// Get reflection data for the parent type.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ReflectionData? GetParent(this ReflectionDataBase self)
        {
            return (ReflectionData?) self.Parent;
        }
    }
}