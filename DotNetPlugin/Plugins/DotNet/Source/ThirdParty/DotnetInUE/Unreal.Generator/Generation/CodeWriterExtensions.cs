// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Generation
{
    public static class CodeWriterExtensions
    {
        public static CodeWriterToken Indent(this CodeWriter writer, string? indent = null)
        {
            writer.PushIndent(indent);
            return writer.PushUnIndentTrail();
        }
        
        public static CodeWriterToken UnIndent(this CodeWriter writer)
        {
            writer.PopIndent(out var indent);
            return writer.PushIndentTrail(indent);
        }

        public static CodeWriterToken OpenBlock(this CodeWriter writer, string additionalTrailing = "")
        {
            writer.WriteLine("{");

            var token = writer.PushTextTrail($"}}{additionalTrailing}\n");
            writer.Indent(); // Push indent with the pop tail.

            // Since the indent happens after it will be popped at the same time as the trail. 
            return token;
        }

        public static CodeWriterToken OpenParenthesis(this CodeWriter writer, string additionalTrailing = "")
        {
            writer.Write("(");
            return writer.PushTextTrail(")" + additionalTrailing);
        }

        public static CodeWriterToken OpenSquareBraces(this CodeWriter writer, string additionalTrailing = "")
        {
            writer.Write("[");
            return writer.PushTextTrail("]" + additionalTrailing);
        }

        public static CodeWriterToken OpenAngledBraces(this CodeWriter writer, string additionalTrailing = "")
        {
            writer.Write("<");
            return writer.PushTextTrail(">" + additionalTrailing);
        }

        /// <summary>
        /// Return a token that writes a given string to the code writer once it gets disposed.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trailing"></param>
        /// <returns></returns>
        public static CodeWriterToken WriteTrailing(this CodeWriter writer, string trailing)
        {
            return writer.PushTextTrail(trailing);
        }

        /// <summary>
        /// Return a token that writes invokes a given action on the code writer once it gets disposed.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trailing"></param>
        /// <returns></returns>
        public static CodeWriterToken WriteTrailing(this CodeWriter writer, Action<CodeWriter> trailing)
        {
            return writer.PushActionTrail(trailing);
        }
    }
}