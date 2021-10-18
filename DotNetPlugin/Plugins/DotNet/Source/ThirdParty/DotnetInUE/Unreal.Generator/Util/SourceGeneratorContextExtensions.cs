// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Unreal.Util
{
    internal static class SourceGeneratorContextExtensions
    {
        private const string SourceItemGroupMetadata = "build_metadata.AdditionalFiles.SourceItemGroup";

        //[return: NotNullIfNotNull("defaultValue")]
        public static string? GetMsBuildProperty(
            this GeneratorExecutionContext context,
            string name,
            string? defaultValue = "")
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value);
            return value ?? defaultValue;
        }

        //[return: MaybeNull, NotNullIfNotNull("defaultValue")]
        public static T GetMsBuildProperty<T>(
            this GeneratorExecutionContext context,
            string name,
            T defaultValue = default)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                try
                {
                    var convertFromString = converter.ConvertFromString(value);
                    if (convertFromString != null)
                        return (T) convertFromString;
                }
                catch
                {
                    // TODO: Handle conversion errors better.
                    // Just return default.
                }

                return defaultValue;
            }

            return defaultValue;
        }

        public static string[] GetMsBuildItems(this GeneratorExecutionContext context, string name)
            => context
                .AdditionalFiles
                .Where(f => context.AnalyzerConfigOptions.GetOptions((AdditionalText) f)
                                .TryGetValue(SourceItemGroupMetadata, out var sourceItemGroup)
                            && sourceItemGroup == name)
                .Select(f => f.Path)
                .ToArray();
    }
}