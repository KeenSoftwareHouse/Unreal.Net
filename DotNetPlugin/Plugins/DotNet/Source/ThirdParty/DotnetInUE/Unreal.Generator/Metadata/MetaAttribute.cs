// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.ErrorHandling;
using Unreal.Generation;

namespace Unreal.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct MetaAttribute
    {
        public readonly string Name;

        public readonly object? Value;

        public readonly bool IsMeta;

        public MetaAttribute(string name, object? value, bool isMeta)
        {
            Name = name;
            Value = value;
            IsMeta = isMeta;
        }

        public string FormatNative()
        {
            return Value switch
            {
                string s => $"{Name}=\"{s}\"",
                { } => $"{Name}={Value}",
                _ => Name
            };
        }

        public string FormatManaged()
        {
            return Value switch
            {
                string s => $"{Name}(\"{s}\")",
                { } => $"{Name}({Value})",
                _ => Name
            };
        }

        public static IEnumerable<MetaAttribute> ExtractAttributes(GenerationContext context,
            SyntaxList<AttributeListSyntax> attrLists, SemanticModel model)
        {
            var attrs = attrLists.SelectMany(x => x.Attributes);

            var list = new List<MetaAttribute>();

            foreach (var attr in attrs)
            {
                var attrType = model.GetTypeInfo(attr).Type;

                // Check if the attribute is relevant to us.
                if (attrType == null || !context.TryGetAttribute(attrType, out var attrInfo))
                    continue;

                object? arg = null;
                if (attrInfo.HasArgument)
                {
                    // There should be one, and only one attribute value.
                    var argExpression = attr.ArgumentList!.Arguments.First().Expression;

                    var argument = model.GetConstantValue(argExpression);
                    if (argument.HasValue)
                        arg = argument.Value;
                    else
                        throw new SourceException("Argument to attribute must be a compile time constant.",
                            argExpression);
                }

                list.Add(new MetaAttribute(attrInfo.UE4Name, arg, attrInfo.IsMeta));
            }

            return list;
        }

        public static void FormatNative(CodeWriter writer, IReadOnlyList<MetaAttribute> attributes)
        {
            List<MetaAttribute>? metaTags = null;

            bool any = false;
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];

                if (attribute.IsMeta)
                {
                    metaTags ??= new List<MetaAttribute>();
                    metaTags.Add(attribute);
                    continue;
                }

                if (any)
                    writer.Write(", ");
                any = true;

                writer.Write(attribute.FormatNative());
            }

            if (metaTags?.Count > 0)
            {
                if (any)
                    writer.Write(", ");
                any = false;

                writer.Write("meta=(");

                for (int i = 0; i < metaTags.Count; i++)
                {
                    if (any)
                        writer.Write(", ");
                    any = true;

                    var attribute = metaTags[i];
                    writer.Write(attribute.FormatNative());
                }

                writer.Write(")");
            }
        }

        public static void FormatManaged(CodeWriter writer, IReadOnlyList<MetaAttribute> attributes)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (i > 0)
                    writer.Write(", ");

                writer.Write(attributes[i].FormatManaged());
            }
        }

        public static readonly string ManagedTypeAttributeName = GetAttributeName(nameof(ManagedTypeAttribute));

        private static string GetAttributeName(string typeName)
        {
            const string Attr = "Attribute";
            if (typeName.EndsWith(Attr))
                return typeName.Substring(0, typeName.Length - Attr.Length);
            return typeName;
        }
    }
}