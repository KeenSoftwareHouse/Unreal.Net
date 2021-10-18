// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Core
{
    /// <summary>
    /// Base interface for classes that represent a UObject.
    /// </summary>
    public interface IUObjectRegistration
    {
        void Register(IntPtr nativeInstance);
        void Unregister();
    }
}