// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class AggregateGenerationException : GenerationException
    {
        public ImmutableArray<GenerationException> Exceptions { get; }

        public AggregateGenerationException(IEnumerable<GenerationException> exceptions)
            : base("One or more generation errors occurred.")
        {
            Exceptions = exceptions.ToImmutableArray();
        }

        public override bool IsFatal => Exceptions.Any(x => x.IsFatal);
        public override Diagnostic CreateDiagnostic()
        {
            // Each individual exception should be logged instead.
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString() + "\n\n---------------- Exceptions: ----------------\n" + string.Join("\n", Exceptions);
        }
    }
}