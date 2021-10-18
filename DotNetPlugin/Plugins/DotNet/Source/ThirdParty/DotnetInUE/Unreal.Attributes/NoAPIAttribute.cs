// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    /// <summary>
    /// Attribute that marks a type as not having an exposed API.
    /// </summary>
    /// <remarks>Types with this attribute can be used but don;t expose any methods.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class NoAPIAttribute : Attribute
    { }
}