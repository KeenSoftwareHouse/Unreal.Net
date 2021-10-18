// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    /// <summary>
    /// Diagnostic messages in this source generator.
    /// </summary>
    public static class Diagnostics
    {
        public static readonly DiagnosticDescriptor MetadataError = new DiagnosticDescriptor(id: "HDR0101",
            title: "Metadata error",
            messageFormat: "Attribute assembly metadata is incorrect: '{0}'",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor SymbolError = new DiagnosticDescriptor(id: "HDR0102",
            title: "Symbol Error",
            messageFormat: "While generating '{0}': Could not locate metadata for {1}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor GenerationError = new DiagnosticDescriptor(id: "HDR0103",
            title: "Generation Error",
            messageFormat: "{0}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor IOReadError = new DiagnosticDescriptor(id: "HDR0104",
            title: "Error reading from file",
            messageFormat: "Cannot read from file '{0}': {1}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor IOWriteError = new DiagnosticDescriptor(id: "HDR0105",
            title: "Error writing to file",
            messageFormat: "Cannot write to file '{0}': {1}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor UnknownError = new DiagnosticDescriptor(id: "HDR0106",
            title: "Unknown Error",
            messageFormat: "{0}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor ConfigurationError = new DiagnosticDescriptor(id: "HDR0107",
            title: "Configuration Error",
            messageFormat: "{0}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor NativeMetadataError = new DiagnosticDescriptor(id: "HDR0108",
            title: "Native Metadata Error",
            messageFormat: "Native Metadata for type {0} is incorrect: {1}",
            category: "Generation",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}