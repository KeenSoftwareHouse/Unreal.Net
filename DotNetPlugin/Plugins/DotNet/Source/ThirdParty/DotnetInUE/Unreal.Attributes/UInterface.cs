// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using JetBrains.Annotations;

namespace Unreal
{
    /// <summary>
    /// Describes a class used as an interface.
    /// </summary>
    /// <remarks>UE4 interfaces are actually classes leveraging C++'s multiple inheritance.</remarks>
    // TODO: Add a mechanism to handle that, including methods with bodies and handling of non-abstract methods from implemented interfaces.
    [AttributeUsage(AttributeTargets.Interface), MeansImplicitUse]
    public class UInterfaceAttribute : MetaAttributeBase
    {
        public override MetaType Type => MetaType.Interface;
    }
}