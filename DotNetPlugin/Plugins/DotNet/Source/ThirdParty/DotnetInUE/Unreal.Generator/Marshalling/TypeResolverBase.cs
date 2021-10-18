// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Marshalling
{
    public abstract class TypeResolverBase
    {
        public TypeResolver Container { get; private set; } = null!;

        public virtual void Register(TypeResolver resolver)
        {
            Container = resolver;
        }
    }
}