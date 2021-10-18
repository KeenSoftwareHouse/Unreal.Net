// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal
{
    public abstract class MetaPropertyAttributeBase : UnrealAttribute
    {
        public abstract MetaTypeFlags ValidTargets { get; }

        /// <summary>
        /// Whether this attribute is a meta tag (specified in UE via 'meta=(tags)')
        /// </summary>
        public virtual bool IsMeta => false;
    }
}