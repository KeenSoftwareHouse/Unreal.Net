// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;

namespace Unreal.Core
{
    /// <summary>
    /// Utility functions for UObject.
    /// </summary>
    public static class UObjectUtil
    {
        // ReSharper disable InconsistentNaming
        private static readonly unsafe delegate* unmanaged<nint> UObject_GetFieldOffset_UClass =
            (delegate* unmanaged<nint>) NativeHelpers.GetPluginFunction("UObject_GetFieldOffset_UClass");

        private static readonly unsafe nint UClass_FieldOffset = UObject_GetFieldOffset_UClass();

        private static readonly unsafe delegate* unmanaged<IntPtr, IntPtr, IntPtr> NativeHelper_CreateUObject =
            (delegate* unmanaged<IntPtr, IntPtr, IntPtr>) NativeHelpers.GetPluginFunction("NativeHelper_CreateUObject");
        
        private static readonly unsafe delegate* unmanaged<IntPtr, IntPtr> UClass_GetSuperClass =
            (delegate* unmanaged<IntPtr, IntPtr>) NativeHelpers.GetPluginFunction("UClass_GetSuperClass");
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Get a pointer to the native counterpart of a managed UObject instance.
        /// </summary>
        /// <param name="uObjectInstance"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetNativeInstance(UObjectBase uObjectInstance)
        {
            return uObjectInstance?.NativeObject ?? IntPtr.Zero;
        }
        
        /// <summary>
        /// Get the native class reference from a UObject.
        /// </summary>
        /// <param name="nativeInstance"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetUClass(IntPtr nativeInstance)
            => *(IntPtr*) ((long) nativeInstance + UClass_FieldOffset);

        /// <summary>
        /// Get the native class reference from a UObject.
        /// </summary>
        /// <param name="managedInstance"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetUClass(UObjectBase managedInstance) => GetUClass(managedInstance.NativeObject);
        
        /// <summary>
        /// Get the parent class handled for a given UClass.
        /// </summary>
        /// <param name="classHandle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetSuperClass(IntPtr classHandle) => UClass_GetSuperClass(classHandle);

        /// <summary>
        /// Create a new object given type and outer instance.
        /// </summary>
        /// <param name="nativeUClassHandle">Handle to the native class for the target type.</param>
        /// <param name="implementation">The implementation of the class, use <see cref="TypeImplementation.Native"/> when not known.</param>
        /// <param name="outerUObject">The instance that will contain the newly created object.</param>
        /// <typeparam name="TObject">The type of the returned object reference.</typeparam>
        /// <returns></returns>
        public static unsafe TObject Create<TObject>(IntPtr nativeUClassHandle, TypeImplementation implementation,
            IntPtr outerUObject)
            where TObject : UObjectBase
        {
            var nativeInstance = NativeHelper_CreateUObject(nativeUClassHandle, outerUObject);

            if (implementation == TypeImplementation.Managed)
                return UObjectBase.GetManaged<TObject>(nativeInstance);
            else
                return UObjectBase.GetOrCreateNative<TObject>(nativeInstance);
        }
    }
}