// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal
{
    public static class MetaTypeExtensions
    {
        public static string GetNativeName(this MetaType type)
        {
            switch (type)
            {
                case MetaType.Class:
                    return "UCLASS";
                case MetaType.Enum:
                    return "UENUM";
                case MetaType.EnumValue:
                    return "UMETA";
                case MetaType.Interface:
                    return "UINTERFACE";
                case MetaType.Struct:
                    return "USTRUCT";
                case MetaType.Function:
                    return "UFUNCTION";
                case MetaType.Property:
                    return "UPROPERTY";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}