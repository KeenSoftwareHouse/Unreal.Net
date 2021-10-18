// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.Metadata
{
    public static class TypeInfoExtensions
    {
        public static bool IsVoid(this ITypeInfo info)
        {
            return ManagedTypeInfo.Void.Equals(info);
        }

        /// <summary>
        /// Full name of the type.
        /// </summary>
        public static string GetManagedFullName(this ITypeInfo info)
        {
            return $"{info.Namespace}.{info.ManagedName}";
        }

        public static QualifiedNativeTypeName GetNativeFullName(this ITypeInfo info)
        {
            return new(info.NativeModule, info.NativeName);
        }
        
        /// <summary>
        /// Full name of the type.
        /// </summary>
        public static string FormatName(this ITypeInfo info, Codespace space)
        {
            return space == Codespace.Native ? info.TypicalArgumentType.FormatType(info.NativeName) : info.ManagedSourceName;
        }

        /// <summary>
        /// Get type dependencies of a given type.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="includeSelf"></param>
        /// <returns></returns>
        public static IEnumerable<ITypeInfo> GetTypeDependencies(this ITypeInfo self, bool includeSelf = false)
        {
            if (includeSelf)
                yield return self;
            
            if (!self.IsGenericType)
                yield break;

            foreach (var arg in ((IGenericTypeInfo) self).GenericArguments)
                yield return arg;
        }
    }
}