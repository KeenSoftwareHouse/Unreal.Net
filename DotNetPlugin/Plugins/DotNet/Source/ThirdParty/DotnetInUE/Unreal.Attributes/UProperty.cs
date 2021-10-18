// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using JetBrains.Annotations;

namespace Unreal
{
    /// <summary>
    /// Defines a property the unreal GC can track.
    /// </summary>
    // TODO: Investigate how UE handles what we'd call "properties".
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), MeansImplicitUse]
    public class UPropertyAttribute : MetaAttributeBase
    {
        public override MetaType Type => MetaType.Property;
    }
}