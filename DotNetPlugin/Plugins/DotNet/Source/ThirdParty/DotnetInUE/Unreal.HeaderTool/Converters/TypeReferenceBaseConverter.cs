// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unreal.NativeMetadata;

namespace Unreal.Converters
{
    public class TypeReferenceBaseConverter : JsonConverter<TypeReferenceBase>
    {
        public override TypeReferenceBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            if (propertyName != nameof(TypeReferenceBase.FieldType))
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            // Read the kind.
            TypeReferenceKind typeDiscriminator = (TypeReferenceKind) Enum.Parse(typeof(TypeReferenceKind), reader.GetString()!);

            // Restore reader from copy
            reader = readerCheckpoint;
            
            // Deserialize the correct type, using the cached previous reader state.
            return typeDiscriminator switch
            {
                TypeReferenceKind.Property => JsonSerializer.Deserialize<TypePropertyReference>(ref reader, options),
                TypeReferenceKind.TypeName => JsonSerializer.Deserialize<TypeNameReference>(ref reader, options),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void Write(Utf8JsonWriter writer, TypeReferenceBase meta, JsonSerializerOptions options)
        {
            switch (meta.FieldType)
            {
                case TypeReferenceKind.Property:
                    JsonSerializer.Serialize(writer, (TypePropertyReference) meta, options);
                    break;
                case TypeReferenceKind.TypeName:
                    JsonSerializer.Serialize(writer, (TypeNameReference) meta, options);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}