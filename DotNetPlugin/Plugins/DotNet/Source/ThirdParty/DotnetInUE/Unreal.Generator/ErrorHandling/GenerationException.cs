// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public abstract class GenerationException : Exception
    {
        protected GenerationException(string? message)
            : base(message)
        { }

        protected GenerationException()
        { }

        public abstract bool IsFatal { get; }

        /// <summary>
        /// Get the diagnostic descriptor that matches this exception.
        /// </summary>
        public abstract Diagnostic CreateDiagnostic();
    }
}