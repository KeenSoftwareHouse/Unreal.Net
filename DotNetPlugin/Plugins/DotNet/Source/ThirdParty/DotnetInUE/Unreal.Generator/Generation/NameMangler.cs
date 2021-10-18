// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unreal.Metadata;

namespace Unreal.Generation
{
    /// <summary>
    /// Provides helper methods for converting class names into a compatible representation.
    /// </summary>
    public static class NameMangler
    {
        public static string MangleTypeName(ITypeInfo type)
        {
            var @namespace = SanitizeName(type.Namespace.Replace(".", "_"));

            var typeName = SanitizeName(type.ManagedName);

            return $"{@namespace}__{typeName}";
        }

        public static string MangleMethodName(ITypeInfo enclosingType, string methodName,
            IReadOnlyList<ParameterDefinition> parameters)
        {
            var typeName = MangleTypeName(enclosingType);

            // TODO: Ideally we'd s
            var sanitizedMethodName = SanitizeName(methodName);

            if (parameters.Count == 0)
            {
                return $"{typeName}__{sanitizedMethodName}_0_";
            }
            else
            {
                var arguments = string.Join("__", parameters.Select(x => MangleType(x.Type)));
                return $"{typeName}__{sanitizedMethodName}_{parameters.Count - 0}_{arguments}";
            }
        }

        public static string MangleType(QualifiedTypeReference type)
        {
            char modifier = type.TransferType switch
            {
                ManagedTransferType.ByValue => 'V',
                ManagedTransferType.In => 'I',
                ManagedTransferType.Out => 'O',
                ManagedTransferType.Ref => 'R',
                _ => throw new ArgumentOutOfRangeException()
            };

            return $"{modifier}{SanitizeName(type.TypeInfo.ManagedName)}";
        }

        public static string SanitizeName(string managedName)
        {
            // TODO: Tweak sanitization rules so that name mangling can be fully reversible.
            // Alternatively we can just make sure the original name mapping is preserved somewhere
            // , and use a more aggressive sanitization instead. 
            StringBuilder name = new();

            for (int i = 0; i < managedName.Length; i++)
            {
                var c = managedName[i];

                // Safe characters (we must comply to C ABI in function names.
                if ((c >= '0' && c <= '9')
                    || (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || c == '_')
                {
                    name.Append(c);
                }
                else
                {
                    int unichar = c;
                    if (char.IsHighSurrogate(c)
                        && i < managedName.Length - 1
                        && managedName[i + 1] is { } next
                        && char.IsLowSurrogate(next))
                    {
                        unichar = char.ConvertToUtf32(c, next);
                        ++i; // Advance over the low surrogate.
                    }

                    name.Append($"u{unichar:x}");
                }
            }

            return name.ToString();
        }
    }
}