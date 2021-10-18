// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    /// <summary>
    /// Tells the generator that instances of the annotated struct, parameter or return value should be transferred by value in native to managed marshalling.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class PassByValueAttribute : Attribute
    { }
}