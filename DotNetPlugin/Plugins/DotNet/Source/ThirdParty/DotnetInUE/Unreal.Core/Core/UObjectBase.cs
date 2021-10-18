// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Reflection;

namespace Unreal.Core
{
    /// <summary>
    /// Base class for UObject that contains additional fields and helpers only relevant to managed code.
    /// </summary>
    [UClass]
    public class UObjectBase
    {
        /// <summary>
        /// Reference to the native object that matches this type.
        /// </summary>
        internal IntPtr NativeObject;

        // Implemented so the compiler lets you write a class without errors before we insert the actual ctor.
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected UObjectBase()
        {
            throw new NotImplementedException();
        }

        protected UObjectBase(IntPtr nativeInstance)
        {
            NativeObject = nativeInstance;
            NativeUObjectRegistration.Register(this);
        }

        protected virtual void Unregister()
        {
            NativeUObjectRegistration.Unregister(this);
        }

        #region UClass Reflection System

        private static TObject CreateManaged<TObject>(IntPtr nativeInstance)
            where TObject : UObjectBase
        {
            var uClass = UObjectUtil.GetUClass(nativeInstance);

            var type = UObjectReflection.Instance.GetBestFitType(uClass).ManagedType;

            if (!typeof(TObject).IsAssignableFrom(type))
                throw new TypeLoadException(
                    "Best fit managed class for object instance is not assignable to the current expected type.");

            return (TObject) Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] {nativeInstance}, null)!;
        }

        #endregion

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TObject? GetManaged<TObject>(IntPtr nativeInstance)
            where TObject : UObjectBase
        {
            if (nativeInstance == IntPtr.Zero)
                return null;

            var instance = ManagedUObjectRegistration.GetUObject<TObject>(nativeInstance);
            if (instance == null)
                throw new InvalidOperationException(
                    "Managed UObject are always instantiated on creation, your object is either already dead or corrupted.");

            return instance;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TObject? GetOrCreateNative<TObject>(IntPtr nativeInstance)
            where TObject : UObjectBase
        {
            if (nativeInstance == IntPtr.Zero)
                return null;

            // TODO: This needs a lock.
            var instance = NativeUObjectRegistration.GetUObject<TObject>(nativeInstance);
            if (instance == null)
                return CreateManaged<TObject>(nativeInstance);

            return instance;
        }
    }
}