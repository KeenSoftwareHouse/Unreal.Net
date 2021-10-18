// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    internal class SourceException : GenerationException
    {
        public readonly SyntaxNode? Syntax;

        public Location Location => Syntax?.GetLocation() ?? Location.None;

        public SourceException(string? message, SyntaxNode? syntax = null)
            : base(message)
        {
            Syntax = syntax;
        }

        public override bool IsFatal => false;
        
        public override Diagnostic CreateDiagnostic()
        {
            return Diagnostic.Create(Diagnostics.GenerationError, Location, Message);
        }
    }
}