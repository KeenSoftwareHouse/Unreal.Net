// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Marshalling
{
    public struct MarshalFormats
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
        public MarshalFormats(string fromManagedToIntermediate, string fromIntermediateToManaged,
            string fromNativeToIntermediate, string fromIntermediateToNative)
        {
            FromManagedToIntermediate = fromManagedToIntermediate;
            FromIntermediateToManaged = fromIntermediateToManaged;
            FromNativeToIntermediate = fromNativeToIntermediate;
            FromIntermediateToNative = fromIntermediateToNative;

            RequiredHeader = RequiredNamespace = null;
        }

        public MarshalFormats(MarshalFormatsAttribute attr)
        {
            FromManagedToIntermediate = attr.FromManagedToIntermediate;
            FromIntermediateToManaged = attr.FromIntermediateToManaged;
            FromNativeToIntermediate = attr.FromNativeToIntermediate;
            FromIntermediateToNative = attr.FromIntermediateToNative;
            RequiredHeader = attr.RequiredHeader;
            RequiredNamespace = attr.RequiredNamespace;
        }
        

        public static readonly MarshalFormats Default = new("{0}", "{0}", "{0}", "{0}");
    }
}