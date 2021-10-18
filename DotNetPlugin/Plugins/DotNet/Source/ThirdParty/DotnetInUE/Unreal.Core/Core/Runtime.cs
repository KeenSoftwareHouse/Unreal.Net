// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Unreal.Core
{
    public static class Runtime
    {
        private static unsafe void Init(delegate * unmanaged<char*, void*> entryPointGetter)
        {
            NativeHelpers.Init(entryPointGetter);
        }

        [UnmanagedCallersOnly(EntryPoint = "Unreal_Core__Runtime__Init")]
        private static unsafe void InitAot(delegate * unmanaged<char*, void*> entryPointGetter)
        {
            Init(entryPointGetter);
        }
        
        /// <summary>
        /// Get the entry point for an exported function.
        /// </summary>
        /// <remarks>
        /// Only functions marked with <see cref="UnmanagedCallersOnlyAttribute"/> can be returned.
        /// This method is very slow and the queried function should be cached for later use.
        /// </remarks>
        /// <param name="assemblyCharPtr"></param>
        /// <param name="typeCharPtr"></param>
        /// <param name="functionCharPtr"></param>
        /// <returns></returns>
        public static unsafe void* GetFunctionNative(byte* assemblyCharPtr, byte* typeCharPtr, byte* functionCharPtr)
        {
            var assemblyName = new AssemblyName(NativeHelpers.GetString(assemblyCharPtr));
            var typeName = NativeHelpers.GetString(typeCharPtr);
            var functionName = NativeHelpers.GetString(functionCharPtr);

            try
            {
                // TODO: Probably cache all assemblies instead of calling load.
                // If this method will end up slowing down startup we might want to instead index all of these methods and load them manually. 
                
                var assembly = Assembly.Load(assemblyName);
                var type = assembly.GetType(typeName, true)!;
                var method = type.GetMethod(functionName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (method == null)
                    throw new EntryPointNotFoundException("The method could not be found.");

                return (void*) GetFunction(method);
            }
            catch (Exception e)
            {
                UeLog.Log(LogVerbosity.Error,
                    $"Could not load function {assemblyName}::{typeName}::{functionName}: {e}");
                return null;
            }
        }

        private static readonly ConcurrentDictionary<MethodInfo, IntPtr> CachedMethodPointers = new();

        internal static IntPtr GetFunction(MethodInfo method)
        {
            if (method.GetCustomAttribute<UnmanagedCallersOnlyAttribute>() == null)
                throw new MethodAccessException("Method is not annotated with the UnmanagedCallersOnly attribute.");

            if (CachedMethodPointers.TryGetValue(method, out var ptr))
                return ptr;

            var getter = new DynamicMethod("GetFunction", typeof(IntPtr), null);
            var gen = getter.GetILGenerator();
            gen.Emit(OpCodes.Ldftn, method);

            var ctor = typeof(IntPtr).GetConstructor(new[] {typeof(void*)})!;

            gen.Emit(OpCodes.Newobj, ctor);
            gen.Emit(OpCodes.Ret);

            var result = (IntPtr) getter.Invoke(null, null)!;

            // If two paths look up the same method they'll find the same result.
            CachedMethodPointers.TryAdd(method, result);

            return result;
        }
    }
}