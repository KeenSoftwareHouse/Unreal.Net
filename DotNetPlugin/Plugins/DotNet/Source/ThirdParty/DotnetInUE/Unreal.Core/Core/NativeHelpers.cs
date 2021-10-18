// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Unreal.Core
{
    /// <summary>
    /// Collection of methods that are used to aid in binding objects between the native and managed worlds. 
    /// </summary>
    public static unsafe class NativeHelpers
    {
        private static delegate* unmanaged<char*, void*> m_entryGetter;

        internal static void Init(delegate* unmanaged<char*, void*> entryGetter)
        {
            m_entryGetter = entryGetter;
        }

        /// <summary>
        /// Get a function declared in the DotNet plugin.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static void* GetPluginFunction(string functionName)
        {
            fixed (char* pinnedName = functionName)
            {
                var entry = m_entryGetter(pinnedName);
                if (entry == null)
                    throw new MissingMethodException($"Could not locate entry point for function named '{functionName}'");
                return entry;
            }
        }

        public static string GetString(byte* utf8String)
        {
            return Marshal.PtrToStringAnsi(new IntPtr(utf8String)) ?? "";
        }
    }
}