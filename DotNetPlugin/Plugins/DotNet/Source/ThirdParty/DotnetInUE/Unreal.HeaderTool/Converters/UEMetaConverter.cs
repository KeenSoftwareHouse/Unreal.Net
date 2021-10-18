// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unreal.NativeMetadata;

namespace Unreal.Converters
{
    public class UEMetaConverter : JsonConverter<UEMeta>
    {
        public override UEMeta? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // make a copy of the reader, we'll come back to it.
            var readerCheckpoint = reader;

            // Read the start of the object and get the kind.
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            if (propertyName != nameof(UEMeta.Kind))
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            // Read the kind.
            UETypeKind typeDiscriminator =
                (UETypeKind) Enum.Parse(typeof(UETypeKind), reader.GetString() ?? nameof(UETypeKind.None));

            // Restore reader from copy
            reader = readerCheckpoint;
            
            // Deserialize the correct type, using the cached previous reader state.
            return typeDiscriminator switch
            {
                UETypeKind.UPackage => JsonSerializer.Deserialize<UEModule>(ref reader, options),
                UETypeKind.UObject => JsonSerializer.Deserialize<UEClass>(ref reader, options),
                UETypeKind.UInterface => JsonSerializer.Deserialize<UEClass>(ref reader, options),
                UETypeKind.UStruct => JsonSerializer.Deserialize<UEStruct>(ref reader, options),
                UETypeKind.UEnum => JsonSerializer.Deserialize<UEEnum>(ref reader, options),
                UETypeKind.UFunction => JsonSerializer.Deserialize<UEFunction>(ref reader, options),
                UETypeKind.UDelegate => JsonSerializer.Deserialize<UEDelegate>(ref reader, options),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void Write(Utf8JsonWriter writer, UEMeta meta, JsonSerializerOptions options)
        {
            switch (meta.Kind)
            {
                case UETypeKind.UPackage:
                    JsonSerializer.Serialize(writer, (UEModule) meta, options);
                    break;
                case UETypeKind.UObject:
                    JsonSerializer.Serialize(writer, (UEClass) meta, options);
                    break;
                case UETypeKind.UInterface:
                    JsonSerializer.Serialize(writer, (UEClass) meta, options);
                    break;
                case UETypeKind.UStruct:
                    JsonSerializer.Serialize(writer, (UEStruct) meta, options);
                    break;
                case UETypeKind.UEnum:
                    JsonSerializer.Serialize(writer, (UEEnum) meta, options);
                    break;
                case UETypeKind.UFunction:
                    JsonSerializer.Serialize(writer, (UEFunction) meta, options);
                    break;
                case UETypeKind.UDelegate:
                    JsonSerializer.Serialize(writer, (UEDelegate) meta, options);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}