// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Unreal.Util
{
    internal static class SymbolExtensions
    {
        public static bool TryGetAttribute(this ISymbol self, INamedTypeSymbol attributeType,
            out AttributeData? attribute)
        {
            var attrs = self.GetAttributes();
            for (int i = 0; i < attrs.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(attributeType, attrs[i].AttributeClass))
                {
                    attribute = attrs[i];
                    return true;
                }
            }

            attribute = null;
            return false;
        }

        public static AttributeData? GetAttribute(this ISymbol self, INamedTypeSymbol attributeType)
        {
            var attrs = self.GetAttributes();
            for (int i = 0; i < attrs.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(attributeType, attrs[i].AttributeClass))
                {
                    return attrs[i];
                }
            }

            return null;
        }

        public static bool IsNullable(this ITypeSymbol type)
        {
            return ((type as INamedTypeSymbol)?.IsGenericType ?? false)
                   && type.OriginalDefinition.ToDisplayString()
                       .Equals("System.Nullable<T>", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullable(this ITypeSymbol type, out ITypeSymbol? nullableType)
        {
            if (type.IsNullable())
            {
                nullableType = ((INamedTypeSymbol) type).TypeArguments.First();
                return true;
            }
            else
            {
                nullableType = null;
                return false;
            }
        }

        private static readonly Dictionary<string, string> builtinTypeMapping =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"string", typeof(string).ToString()},
                {"long", typeof(long).ToString()},
                {"int", typeof(int).ToString()},
                {"short", typeof(short).ToString()},
                {"ulong", typeof(ulong).ToString()},
                {"uint", typeof(uint).ToString()},
                {"ushort", typeof(ushort).ToString()},
                {"byte", typeof(byte).ToString()},
                {"double", typeof(double).ToString()},
                {"float", typeof(float).ToString()},
                {"decimal", typeof(decimal).ToString()},
                {"bool", typeof(bool).ToString()},
            };

        public static string GetFullReferenceName(this ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                return $"{arrayType.ElementType.GetFullReferenceName()}[]";
            }

            if (type.IsNullable(out ITypeSymbol? elementType))
            {
                return $"{elementType!.GetFullReferenceName()}?";
            }

            var name = type.Name;

            if (!builtinTypeMapping.TryGetValue(name, out string output))
            {
                output = name;
            }

            if (type is INamedTypeSymbol ts)
            {
                if (!ts.TypeParameters.IsEmpty)
                {
                    output += "<" + new string(',', ts.TypeArguments.Length - 1) + ">";
                }
                else if (!ts.TypeArguments.IsEmpty)
                {
                    output += "<" + string.Join(", ", ts.TypeArguments.Select(x => x.GetFullReferenceName())) + ">";
                }
            }

            return output;
        }

        public static string GetFullName(this INamespaceOrTypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                return $"{arrayType.ElementType.GetFullName()}[]";
            }

            if (type is ITypeSymbol ts && ts.IsNullable(out var elementType))
            {
                return $"System.Nullable`1[{elementType!.GetFullName()}]";
            }

            var name = type.ToDisplayString();

            if (!builtinTypeMapping.TryGetValue(name, out string output))
            {
                output = name;
            }

            return output;
        }

        public static string GetFullMetadataName(this INamespaceOrTypeSymbol symbol)
        {
            ISymbol currentSymbol = symbol;
            var sb = new StringBuilder(currentSymbol.MetadataName);

            var last = currentSymbol;
            currentSymbol = currentSymbol.ContainingSymbol;

            if (currentSymbol == null)
            {
                return symbol.GetFullName();
            }

            while (currentSymbol != null && !IsRootNamespace(currentSymbol))
            {
                if (currentSymbol is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }

                sb.Insert(0, currentSymbol.MetadataName);

                currentSymbol = currentSymbol.ContainingSymbol;
            }

            var namedType = symbol as INamedTypeSymbol;

            if (namedType?.TypeArguments.Any() ?? false)
            {
                var genericArgs = string.Join(",", namedType.TypeArguments.Select(GetFullMetadataName));
                sb.Append($"[{genericArgs}]");
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(ISymbol s)
        {
            return s is INamespaceSymbol {IsGlobalNamespace: true};
        }

        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType,
            bool exactMatch = false)
        {
            if (exactMatch)
            {
                return SymbolEqualityComparer.Default.Equals(sourceType, targetType);
            }

            ITypeSymbol? symbol = sourceType;
            while (symbol != null)
            {
                if (SymbolEqualityComparer.Default.Equals(symbol, targetType))
                {
                    return true;
                }

                if (targetType.TypeKind == TypeKind.Interface)
                {
                    return symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetType));
                }

                symbol = symbol.BaseType;
            }

            return false;
        }

        public static bool IsEqualTo(this ISymbol? symbol, ISymbol other)
        {
            return SymbolEqualityComparer.Default.Equals(symbol, other);
        }

        internal static IEnumerable<ITypeSymbol> GetBaseTypes(this ITypeSymbol typeSymbol)
        {
            var currentSymbol = typeSymbol;
            while (currentSymbol.BaseType != null && currentSymbol.BaseType.GetFullMetadataName() != "System.Object")
            {
                currentSymbol = currentSymbol.BaseType;
                yield return currentSymbol;
            }
        }
    }
}