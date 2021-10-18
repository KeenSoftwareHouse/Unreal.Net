// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Unreal.Core;
using Unreal.CoreUObject;

namespace Unreal.Engine
{
    public partial class UEngine : UObject
    {
        #region PInvoke

        // ReSharper disable InconsistentNaming
        private static readonly unsafe delegate * unmanaged <void**> UEngine_Get_GEngine =
            (delegate * unmanaged <void**>) NativeHelpers.GetPluginFunction("UEngine_Get_GEngine");

        private static readonly unsafe delegate * unmanaged <void*, int, float, uint, char*, void>
            UEngine_AddOnScreenDebugMessage =
                (delegate * unmanaged <void*, int, float, uint, char*, void>) NativeHelpers.GetPluginFunction(
                    "UEngine_AddOnScreenDebugMessage");
        // ReSharper restore InconsistentNaming

        #endregion

        private static readonly unsafe void** EngineInstance = UEngine_Get_GEngine();

        private static UEngine? m_cachedInstance;

        public static unsafe UEngine Instance
        {
            get
            {
                var handle = new IntPtr(*EngineInstance);
                if (m_cachedInstance == null || UObjectUtil.GetNativeInstance(m_cachedInstance) != handle)
                {
                    m_cachedInstance = GetOrCreateNative<UEngine>(handle)!;
                }

                return m_cachedInstance;
            }
        }

        /// <summary>
        /// Print a debug message to the game's screen.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="duration"></param>
        /// <param name="color"></param>
        /// <param name="message"></param>
        public unsafe void PrintMessage(int key, float duration, Color color, string message)
        {
            fixed (char* chars = message)
                UEngine_AddOnScreenDebugMessage(UObjectUtil.GetNativeInstance(this).ToPointer(), key, duration,
                    color.GetValue(), chars);
        }
    }
}