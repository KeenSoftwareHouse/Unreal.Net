// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Core
{
    /// <summary>
    /// Provides methods for interacting with the UE Logging system.
    /// </summary>
    public static class UeLog
    {
        #region PInvoke

        // ReSharper disable InconsistentNaming
        private static readonly unsafe delegate * unmanaged<byte, char*, void> UeLog_Log =
            (delegate * unmanaged<byte, char*, void>) NativeHelpers.GetPluginFunction("UeLog_Log");
        // ReSharper restore InconsistentNaming

        #endregion

        /// <summary>
        /// Write a message to the engine's log.
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="message"></param>
        public static unsafe void Log(LogVerbosity verbosity, string message)
        {
            fixed (char* chars = message)
                UeLog_Log((byte) verbosity, chars);
        }
    }
}