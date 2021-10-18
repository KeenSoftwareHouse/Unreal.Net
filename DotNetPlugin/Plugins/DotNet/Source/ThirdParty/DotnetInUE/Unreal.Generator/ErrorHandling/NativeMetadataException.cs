// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class NativeMetadataException : GenerationException
    {
        private readonly string m_type;

        public NativeMetadataException(string type, string message)
            : base(message)
        {
            m_type = type;
        }

        public override bool IsFatal => true;
        
        public override Diagnostic CreateDiagnostic()
        {
            return Diagnostic.Create(Diagnostics.NativeMetadataError, Location.None, m_type, Message);
        }
    }
}