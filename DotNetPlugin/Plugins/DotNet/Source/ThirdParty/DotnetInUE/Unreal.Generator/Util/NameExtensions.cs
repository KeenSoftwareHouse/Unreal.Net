// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unreal.Util
{
    public static class NameExtensions
    {
        public static string GetFullName(this TypeDeclarationSyntax @class)
        {
            string prev = @class.Parent switch
            {
                NamespaceDeclarationSyntax nds => nds.Name.WithoutTrivia().ToFullString() + ".",
                TypeDeclarationSyntax cds => cds.GetFullName() + "+",
                _ => ""
            };

            return $"{prev}{@class.Identifier.Text}";
        }

        /// <summary>
        /// Get all of the components of a class' full name.
        /// </summary>
        /// <remarks>
        /// The first component is always the namespace, even if empty;
        /// The last component is the actual class name;
        /// The remaining components are the names of other types the class is nested inside.
        /// </remarks>
        /// <param name="class"></param>
        /// <param name="namespace">The namespace syntax that encloses the top most type in the nesting hierarchy.</param>
        /// <param name="enclosingTypes">The list of types enclosing the class declaration syntax. Types are ordered from outermost to innermost.</param>
        /// <returns></returns>
        public static void GetFullNameComponents(this TypeDeclarationSyntax @class,
            out NamespaceDeclarationSyntax? @namespace, out TypeDeclarationSyntax[] enclosingTypes)
        {
            List<TypeDeclarationSyntax> parentClasses = new();

            var parent = @class.Parent;
            while (parent is TypeDeclarationSyntax pc)
            {
                parentClasses.Add(pc);
                parent = pc.Parent;
            }

            enclosingTypes = parentClasses.ToArray();

            if (parent is NamespaceDeclarationSyntax pn)
                @namespace = pn;
            else
                @namespace = null;
        }

        public static string GetNamespace(this TypeDeclarationSyntax @class)
        {
            return @class.Parent switch
            {
                NamespaceDeclarationSyntax nds => nds.Name.WithoutTrivia().ToFullString(),
                TypeDeclarationSyntax cds => cds.GetNamespace(),
                _ => ""
            };
        }
    }
}