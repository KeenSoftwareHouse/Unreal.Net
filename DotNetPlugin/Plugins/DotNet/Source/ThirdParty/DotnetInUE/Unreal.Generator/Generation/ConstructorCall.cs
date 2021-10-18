// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Metadata;

namespace Unreal.Generation
{
    /// <summary>
    /// Represents a call to another constructor on the same type or supertype.
    /// </summary>
    public class ConstructorCall
    {
        /// <summary>
        /// Whether this calls a constructor on the same type or the supertype.
        /// </summary>
        public bool SameType;

        /// <summary>
        /// The arguments passed to the other constructor.
        /// </summary>
        public string Arguments;

        public ConstructorCall(bool sameType, string arguments)
        {
            SameType = sameType;
            Arguments = arguments;
        }

        /// <summary>
        /// Create a new constructor call calling another ctor on the same type. 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static ConstructorCall This(string arguments) => new(false, arguments);

        /// <summary>
        /// Create a new constructor call calling a ctor on the super type. 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static ConstructorCall Super(string arguments) => new(true, arguments);

        public void Write(CodeWriter writer, TypeDefinition enclosingType, Codespace space)
        {
            if (space == Codespace.Managed)
            {
                writer.Write(SameType ? "this" : "base");
                using(writer.OpenParenthesis("\n"))
                    writer.Write(Arguments);
            }
            else
            {
                writer.Write(SameType ? "this" : "base");
                using(writer.OpenParenthesis("\n"))
                    writer.Write(Arguments);
            }
            
        }
    }
}