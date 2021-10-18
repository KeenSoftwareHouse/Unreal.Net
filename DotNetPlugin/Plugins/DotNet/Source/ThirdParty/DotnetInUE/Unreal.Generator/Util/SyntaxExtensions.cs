// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unreal.Util
{
    internal static class SyntaxExtensions
    {
        internal static bool ContainsAttributeType(this SyntaxList<AttributeListSyntax> attributes,
            SemanticModel semanticModel, INamedTypeSymbol attributeType, bool exactMatch = false)
            => attributes.Any(list => list.Attributes.Any(attrbute =>
                attributeType.IsAssignableFrom(ModelExtensions.GetTypeInfo(semanticModel, attrbute).Type, exactMatch)));

        internal static AttributeSyntax? GetAttributeOfType(this SyntaxList<AttributeListSyntax> attributes,
            SemanticModel semanticModel, INamedTypeSymbol attributeType, bool exactMatch = false)
            => attributes.SelectMany(x => x.Attributes)
                .FirstOrDefault(attribute =>
                    attributeType.IsAssignableFrom(ModelExtensions.GetTypeInfo(semanticModel, attribute).Type,
                        exactMatch));

        internal static SyntaxToken[] GetModifiers(this Accessibility accessibility)
        {
            var list = new List<SyntaxToken>(2);

            switch (accessibility)
            {
                case Accessibility.Internal:
                    list.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                    break;
                case Accessibility.Public:
                    list.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                    break;
                case Accessibility.Private:
                    list.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                    break;
                case Accessibility.Protected:
                    list.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                    break;
                case Accessibility.ProtectedOrInternal:
                    list.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                    list.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                    break;
                case Accessibility.ProtectedAndInternal:
                    list.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                    list.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                    break;
                case Accessibility.NotApplicable:
                    break;
            }

            return list.ToArray();
        }
    }
}