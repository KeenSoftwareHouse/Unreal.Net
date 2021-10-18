// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Attribute that provides the expressions used to generate marshalling of the type between managed and native code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MarshalFormatsAttribute : Attribute
    {
        public readonly string FromManagedToIntermediate;
        public readonly string FromIntermediateToManaged;
        public readonly string FromNativeToIntermediate;
        public readonly string FromIntermediateToNative;

        /// <summary>
        /// Header required by the native marshalling part if any.
        /// </summary>
        public string? RequiredHeader { get; set; }

        /// <summary>
        /// Header required by the managed marshalling part if any.
        /// </summary>
        public string? RequiredNamespace { get; set; }

        /// <summary>
        /// Construct a new MarshalFormats attribute. Each parameter should be a string a formatting string that will
        /// be used to marshal a single argument. 
        /// </summary>
        /// <param name="fromManagedToIntermediate">Managed to intermediate formatting string.</param>
        /// <param name="fromIntermediateToManaged">Intermediate to managed formatting string.</param>
        /// <param name="fromNativeToIntermediate">Native to intermediate formatting string.</param>
        /// <param name="fromIntermediateToNative">Intermediate to native formatting string.</param>
        public MarshalFormatsAttribute(string fromManagedToIntermediate, string fromIntermediateToManaged,
            string fromNativeToIntermediate, string fromIntermediateToNative)
        {
            FromManagedToIntermediate = fromManagedToIntermediate;
            FromIntermediateToManaged = fromIntermediateToManaged;
            FromNativeToIntermediate = fromNativeToIntermediate;
            FromIntermediateToNative = fromIntermediateToNative;
        }

        public static readonly MarshalFormatsAttribute Default = new("{0}", "{0}", "{0}", "{0}");
    }
}