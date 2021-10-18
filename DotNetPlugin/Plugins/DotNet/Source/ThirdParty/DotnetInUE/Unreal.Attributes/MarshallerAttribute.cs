// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter)]
    public class MarshallerAttribute : Attribute
    {
        public Type Marshaller { get; }

        public MarshallerAttribute(Type marshaller)
        {
            Marshaller = marshaller;
        }
    }
}