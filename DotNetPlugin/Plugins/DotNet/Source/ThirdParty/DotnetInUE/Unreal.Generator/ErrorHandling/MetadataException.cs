// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class MetadataException : GenerationException
    {
        public MetadataException(string? message)
            : base(message)
        { }

        public override bool IsFatal => true;
        
        public override Diagnostic CreateDiagnostic()
        {
            return Diagnostic.Create(Diagnostics.MetadataError, Location.None, Message);
        }
    }
}