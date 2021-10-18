// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class MissingSymbolException : GenerationException
    {
        /// <summary>
        /// Set this to override default severity.
        /// </summary>
        public DiagnosticSeverity ActualSeverity { get; set; }
        
        public override bool IsFatal => false;

        public string RequestingType;
        
        private readonly string m_missingType;

        private readonly Location m_location;

        public MissingSymbolException(string missingType, string requestingType = "unknown", Location? location = null)
        {
            m_missingType = missingType;
            RequestingType = requestingType;
            m_location = location ?? Location.None;

            ActualSeverity = Diagnostics.SymbolError.DefaultSeverity;
        }

        public override Diagnostic CreateDiagnostic()
        {
            return Diagnostic.Create(Diagnostics.SymbolError, m_location, ActualSeverity, null, null,  RequestingType, m_missingType);
        }
    }
}