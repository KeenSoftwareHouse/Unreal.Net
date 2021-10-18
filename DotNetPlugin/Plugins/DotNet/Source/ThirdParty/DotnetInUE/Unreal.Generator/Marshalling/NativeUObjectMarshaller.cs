// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public class NativeUObjectMarshaller : UObjectMarshaller
    {
        public static readonly NativeUObjectMarshaller Instance = new();

        private NativeUObjectMarshaller()
        { }

        public override string MarshalFromNativeAfter(QualifiedTypeReference type, string argumentName)
        {
            return $"GetOrCreateNative<{type.TypeInfo.ManagedName}>({argumentName})";
        }
    }
}