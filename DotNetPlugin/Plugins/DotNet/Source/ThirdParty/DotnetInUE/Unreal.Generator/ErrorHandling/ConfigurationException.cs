// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class ConfigurationException : GenerationException
    {
        public ConfigurationException(string message)
            : base(message)
        { }

        public override bool IsFatal => true;

        public override Diagnostic CreateDiagnostic()
        {
            return Diagnostic.Create(Diagnostics.ConfigurationError, Location.None, Message);
        }
    }
}