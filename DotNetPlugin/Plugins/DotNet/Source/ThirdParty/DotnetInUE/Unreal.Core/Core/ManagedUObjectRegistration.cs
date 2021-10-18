// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Unreal.Core
{
    /// <summary>
    /// Interface that identifies UObject classes that have a managed implementation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ManagedUObjectRegistration
    {
        #region PInvokes

        // ReSharper disable InconsistentNaming
        private static readonly unsafe delegate* unmanaged<void*, void*> NativeHelper_Cast_UObject_IManagedObject
            = (delegate* unmanaged<void*, void*>) NativeHelpers.GetPluginFunction(
                "NativeHelper_Cast_UObject_IManagedObject");

        private static readonly unsafe delegate* unmanaged<nuint> IManagedObject_GetFieldOffset_Handle
            = (delegate* unmanaged<nuint>) NativeHelpers.GetPluginFunction("IManagedObject_GetFieldOffset_Handle");
        // ReSharper restore InconsistentNaming

        #endregion

        private static unsafe readonly nuint ManagedPointerFieldOffset = IManagedObject_GetFieldOffset_Handle();

        private static unsafe ref IntPtr GetNativeHandle(IntPtr uobject)
        {
            var iManagedObject = NativeHelper_Cast_UObject_IManagedObject(uobject.ToPointer());
            if (iManagedObject == null)
                throw new InvalidCastException(
                    "Native instance was expected to implement IManagedObject, but that was not the case.");

            return ref *(IntPtr*) ((ulong) iManagedObject + ManagedPointerFieldOffset);
        }

        public static void Unregister(UObjectBase uObject)
        {
            IntPtr handleValue;
            {
                // Ensure reference leaves scope.
                ref nint handle = ref GetNativeHandle(uObject.NativeObject);
                handleValue = Interlocked.Exchange(ref handle, IntPtr.Zero);
            }

            if (handleValue != IntPtr.Zero)
                GCHandle.FromIntPtr(handleValue).Free();
        }

        /// <summary>
        /// Get the managed object that maps 
        /// </summary>
        /// <param name="nativeInstance"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static TObject? GetUObject<TObject>(IntPtr nativeInstance)
            where TObject : UObjectBase
        {
            if (nativeInstance == IntPtr.Zero)
                return null;

            var handle = GetNativeHandle(nativeInstance);
            if (handle == IntPtr.Zero)
                return null;

            var gcHandle = GCHandle.FromIntPtr(handle);
            return (TObject?) gcHandle.Target;
        }
    }
}