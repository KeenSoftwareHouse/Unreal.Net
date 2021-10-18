// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unreal.Converters;
using Unreal.NativeMetadata;

namespace Unreal
{
    public class MetadataCollector
    {
        public List<UEField> Types = new();

        public List<UEModule> Modules = new();

        /// <summary>
        /// Collect all files in a given directory.
        /// </summary>
        /// <param name="path"></param>
        public MetadataCollector(string path)
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.umeta", SearchOption.AllDirectories))
            {
                var meta = LoadFromString(File.ReadAllText(file));

                if (meta is UEModule module)
                    Modules.Add(module);
                else
                {
                    Types.Add((UEField) meta);

                    if (meta is not UEStruct s)
                        continue;

                    foreach (var prop in s.Properties)
                        prop.Struct = s;

                    if (meta is not UEClass c)
                        continue;

                    foreach (var f in c.Functions)
                    {
                        f.Class = c;

                        foreach (var prop in f.Parameters)
                        {
                            prop.Struct = c;
                            prop.Function = f;
                        }
                    }
                }
            }
        }

        public IEnumerable<UEProperty> GetAllProperties()
        {
            foreach (var type in Types)
            {
                if (type is not UEStruct s)
                    continue;

                foreach (var p in s.Properties)
                    yield return p;

                if (s is not UEClass c)
                    continue;

                foreach (var f in c.Functions)
                {
                    foreach (var arg in f.Parameters)
                        yield return arg;

                    yield return f.GetReturn();
                }
            }
        }

        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new UEMetaConverter(),
                new TypeReferenceBaseConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            WriteIndented = true,
        };

        public static UEMeta LoadFromString(string rawMeta)
        {
            return JsonSerializer.Deserialize<UEMeta>(rawMeta, JsonOptions) ??
                   throw new InvalidOperationException("Json string was empty.");
        }
    }
}