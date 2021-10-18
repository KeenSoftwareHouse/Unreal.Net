// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Attributes.Meta
{
    /// <summary>
    /// Exception describing why an attribute defined in this assembly is not valid.
    /// </summary>
    public class AttributeFormatException : Exception
    {
        public AttributeFormatException(string? message)
            : base(message)
        { }
    }
}