// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public class ManagedUObjectMarshaller : UObjectMarshaller
    {
        public static readonly ManagedUObjectMarshaller Instance = new();

        private ManagedUObjectMarshaller()
        { }

        public override string MarshalFromNativeAfter(QualifiedTypeReference type, string argumentName)
        {
            return $"GetManaged<{type.TypeInfo.ManagedName}>({argumentName})";
        }
    }
}