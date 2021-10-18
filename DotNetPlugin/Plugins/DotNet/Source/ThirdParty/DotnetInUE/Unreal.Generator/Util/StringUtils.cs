// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace Unreal.Util
{
    public static class StringUtils
    {
        public static string FormatLiteral(string @string)
        {
            return SyntaxFactory.Literal(@string).ToFullString();
        }
    }
}