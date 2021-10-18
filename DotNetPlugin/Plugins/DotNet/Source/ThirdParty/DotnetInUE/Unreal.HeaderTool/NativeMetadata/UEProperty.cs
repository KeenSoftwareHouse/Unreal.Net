// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Unreal.NativeMetadata
{
    public class UEProperty
    {
        public UEProperty()
        { }

        public UEProperty(string name, string rawType)
        {
            Name = name;
            RawType = rawType;
        }

        [JsonIgnore]
        public UEStruct Struct { get; set; }

        [JsonIgnore]
        public UEFunction? Function { get; set; }

        /// <summary>
        /// Meta tags of the property.
        /// </summary>
        public Dictionary<string, string> Meta { get; set; } = new();

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Raw CPP property type.
        /// </summary>
        public string RawType { get; set; }

        // Remove template arguments for generic raw types.
        public string GetCleanRawType()
        {
            if (GenericTypeParameters.Count <= 0)
                return RawType;

            var index = RawType.IndexOf("<", StringComparison.Ordinal);
            if (index >= 0)
                return RawType.Substring(0, index);

            return RawType;
        }

        /// <summary>
        /// Get a nice string representation of the type.
        /// </summary>
        /// <returns></returns>
        public string GetPrettyType()
        {
            if (GenericTypeParameters.Count <= 0)
                return RawType;

            var index = RawType.IndexOf("<", StringComparison.Ordinal);
            if (index < 0)
                return
                    $"{RawType}<{string.Join(",", GenericTypeParameters.Select(x => x.GetPrettyType()))}>";

            return RawType;
        }

        /// <summary>
        /// Byte offset of this property.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Property flags.
        /// </summary>
        public PropertyFlags Flags { get; set; }

        /// <summary>
        /// Type of the FProperty.
        /// </summary>
        public string PropertyType { get; set; } = "";

        /// <summary>
        /// Number of array elements in the property.
        /// </summary>
        public int ArrayDim { get; set; }

        /// <summary>
        /// Direct type reference when available.
        /// </summary>
        public TypeNameReference? Type { get; set; }

        /// <summary>
        /// Generic type parameters.
        /// </summary>
        public List<TypeReferenceBase> GenericTypeParameters { get; set; } = new();

        /// <summary>
        /// The metadata type of this property is not known to the DotNetBinder tool.
        /// </summary>
        public bool IsUnknown { get; set; }

        public static UEProperty Void = new("", "Void");
    }
}