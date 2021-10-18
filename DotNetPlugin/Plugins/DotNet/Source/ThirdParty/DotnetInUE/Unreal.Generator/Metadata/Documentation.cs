// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.Metadata
{
    /// <summary>
    /// Generic cross language documentation description.
    /// </summary>
    public class Documentation
    {
        public string? Summary;
        public string? Body;

        public readonly List<Parameter> Parameters = new();

        public string? Return;

        public static Documentation FromNative(string nativeDocumentation)
        {
            // TODO: implement
            return new Documentation();
        }

        public static Documentation FromManaged(string managedDocumentation)
        {
            // TODO: implement
            return new Documentation();
        }

        public string FormatNative()
        {
            // TODO: implement
            return "";
        }

        public string FormatManaged()
        {
            // TODO: implement
            return "";
        }

        public struct Parameter
        {
            public string Name;
            public string Description;
        }
    }
}