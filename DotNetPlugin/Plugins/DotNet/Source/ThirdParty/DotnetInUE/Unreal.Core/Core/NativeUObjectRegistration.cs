// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Unreal.Core
{
    /// <summary>
    /// Registrar for native UObjects
    /// </summary>
    internal static class NativeUObjectRegistration
    {
        private static readonly ConcurrentDictionary<IntPtr, UObjectBase> Objects = new();
        
        public static void Register(UObjectBase uObject)
        {
            var result = Objects.TryAdd(uObject.NativeObject, uObject);
#pragma warning disable 618
            if (!result)
                throw new ExecutionEngineException("Another managed object was bound to the same native object.");
#pragma warning restore 618
        }

        public static void Unregister(UObjectBase uObject)
        {
            var removed = Objects.TryRemove((uObject).NativeObject, out _);
            Debug.Assert(removed, "UObject was already unregistered.");
        }
        
        /// <summary>
        /// Get the managed object that maps 
        /// </summary>
        /// <param name="nativeInstance"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        internal static TObject? GetUObject<TObject>(IntPtr nativeInstance)
            where TObject : UObjectBase
        {
            if (Objects.TryGetValue(nativeInstance, out var managed))
                return (TObject?) managed;

            return null;
        }
    }
}