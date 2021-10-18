// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    /// <summary>
    /// Attribute that records the native header file where a native type is declared. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Struct
                    | AttributeTargets.Interface
                    | AttributeTargets.Enum)]
    public class HeaderAttribute : Attribute
    {
        public readonly string Path;

        public HeaderAttribute(string path)
        {
            Path = path;
        }
    }
}