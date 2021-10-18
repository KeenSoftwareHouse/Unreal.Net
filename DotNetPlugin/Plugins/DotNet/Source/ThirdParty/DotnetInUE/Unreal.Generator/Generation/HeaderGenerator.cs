// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/*
namespace Unreal.Generator.Generation
{
    [Generator]
    public class HeaderGenerator : ISourceGenerator
    {
        private static readonly (string Type, string CppName)[] SupportedTypes =
        {
            (typeof(long).FullName!, "uint64_t"),
            (typeof(int).FullName!, "int32_t"),
            (typeof(short).FullName!, "int16_t"),
            (typeof(ulong).FullName!, "uint64_t"),
            (typeof(uint).FullName!, "uint32_t"),
            (typeof(ushort).FullName!, "uint16_t"),
            (typeof(byte).FullName!, "uint8_t"),
            (typeof(sbyte).FullName!, "int8_t"),
            (typeof(double).FullName!, "double"),
            (typeof(float).FullName!, "float"),
            (typeof(char).FullName!, "wchar_t"),
            (typeof(void).FullName!, "void"),
            //(typeof(bool).FullName!, "bool"), // TODO: Must enforce 1 byte encoding of bool arguments.
        };

        // Warning shows even though I'm using the correct comparer.
        #pragma warning disable RS1024 // Compare symbols correctly
        private readonly Dictionary<ISymbol, string> m_cppTypes = new(SymbolEqualityComparer.Default);
        #pragma warning restore RS1024 // Compare symbols correctly

        public void Initialize(GeneratorInitializationContext context)
        { }

        public void Execute(GeneratorExecutionContext context)
        {
            const string UnmanagedCallersAttributeName = "System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";
            

            var exportAttribute = context.Compilation.GetTypeByMetadataName(UnmanagedCallersAttributeName);

            if (exportAttribute == null)
            {
                context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostics.MetadataError, Location.None,
                    UnmanagedCallersAttributeName));
                return;
            }

            CollectTypes(context);

            var headerPath = context.Compilation.Options.SourceReferenceResolver?.NormalizePath("Native", "");

            if (headerPath != null && Directory.Exists(Path.GetRelativePath(headerPath, "..")))
            {
                Directory.CreateDirectory(headerPath);

                using var exports = new StreamWriter(Path.Combine(headerPath, "Public", "Exports.h"));

                var methods = DetermineMethodsToGenerate(context, exportAttribute);

                exports.WriteLine(@"//=== GENERATED HEADER =========================================================
//= Declarations for the exports defined by managed code.
//==============================================================================

#pragma once

#if defined(_MSC_VER)
    //  Microsoft 
    #define EXPORT __declspec(dllexport)
#elif defined(__GNUC__)
    //  GCC
    #define EXPORT __attribute__((visibility(""default"")))
#else
    //  do nothing and hope for the best?
#define EXPORT
#endif

#ifdef __cplusplus
extern ""C"" {
#endif
");

                foreach (var (method, attribute) in methods)
                {
                    var assignment = attribute
                        .DescendantNodesAndSelf(_ => true)
                        .OfType<AttributeArgumentSyntax>()
                        .FirstOrDefault(x => x.NameEquals?.Name.Identifier.ValueText == "EntryPoint");

                    if (assignment == null)
                        continue;

                    try
                    {
                        var returnType = TranslateType(context, method.ReturnType);

                        var name = ((LiteralExpressionSyntax) assignment.Expression).Token.Value;

                        var parameters = method.ParameterList.Parameters.Select(x => GetParameter(context, x));

                        var doc = method.GetLeadingTrivia().NormalizeWhitespace("").ToFullString();

                        if (!string.IsNullOrWhiteSpace(doc))
                            exports.Write(doc);

                        exports.WriteLine(
                            $"EXPORT {returnType} {name} ({string.Join(", ", parameters)});");
                        
                        exports.WriteLine();
                    }
                    catch (SourceException e)
                    {
                        string name;
                        if (method.Parent is ClassDeclarationSyntax parent)
                            name = parent.Identifier.ValueText + "." + method.Identifier.ValueText;
                        else
                            name = method.Identifier.ValueText;

                        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.IllegalMethod, e.Location, name,
                            e.Message));
                    }
                }

                exports.WriteLine(@"#ifdef __cplusplus
}
#endif");
            }
        }

        private string GetParameter(GeneratorExecutionContext context, ParameterSyntax parameter)
        {
            if (parameter.Default != null)
                throw new SourceException("Default values are not supported.", parameter);

            if (parameter.Modifiers.Count > 0)
                throw new SourceException("Parameter modifiers are not supported.", parameter);

            if (parameter.Type == null)
                throw new SourceException($"Parameter {parameter.Identifier.Text} must have a type", parameter);

            var type = TranslateType(context, parameter.Type);

            return $"{type} {parameter.Identifier.Text}";
        }

        private void CollectTypes(GeneratorExecutionContext context)
        {
            foreach (var (type, cppName) in SupportedTypes)
            {
                var symbol = context.Compilation.GetTypeByMetadataName(type);

                if (symbol == null)
                {
                    context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostics.MetadataError, Location.None, type));
                    continue;
                }

                m_cppTypes[symbol] = cppName;
            }
        }

        private string TranslateType(GeneratorExecutionContext context, TypeSyntax symbol)
        {
            if (symbol is NullableTypeSyntax)
                throw new SourceException("Nullable types are not supported.", symbol);

            if (symbol is PointerTypeSyntax ps)
                return TranslateType(context, ps.ElementType) + "*";

            if (symbol is RefTypeSyntax)
                throw new SourceException("Reference types are not supported.", symbol);

            if (symbol is FunctionPointerTypeSyntax fpt)
            {
                if (fpt.CallingConvention?.ManagedOrUnmanagedKeyword.ValueText != "unmanaged")
                    throw new SourceException(
                        "Calling convention for function pointers passed as arguments to exported methods must be unmanaged.",
                        symbol);

                // TODO: Validate and translate the function pointer instead.
                return "void *";
            }

            if (symbol is NullableTypeSyntax)
                throw new SourceException("Nullable types are not supported.", symbol);

            var semanticModel = context.Compilation.GetSemanticModel(symbol.SyntaxTree);
            var type = semanticModel.GetSymbolInfo(symbol).Symbol;

            if (type == null)
                throw new SourceException($"Type {symbol.ToString()} could not be resolved.", symbol);

            if (m_cppTypes.TryGetValue(type, out var nativeType))
                return nativeType;

            throw new SourceException($"Type {symbol.ToString()} is not supported by the C language.", symbol);
        }

        private static List<(MethodDeclarationSyntax Method, AttributeSyntax Attribute)> DetermineMethodsToGenerate(
            GeneratorExecutionContext context,
            INamedTypeSymbol exportAttribute)
        {
            List<(MethodDeclarationSyntax Method, AttributeSyntax Attribute)> methods = new();

            foreach (var inputDocument in context.Compilation.SyntaxTrees)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var methodNodes = inputDocument.GetRoot()
                    .DescendantNodesAndSelf(n =>
                        n is CompilationUnitSyntax || n is NamespaceDeclarationSyntax || n is TypeDeclarationSyntax)
                    .OfType<MethodDeclarationSyntax>();

                var semanticModel = context.Compilation.GetSemanticModel(inputDocument);

                foreach (var method in methodNodes)
                {
                    var attr = method.AttributeLists.GetAttributeOfType(semanticModel, exportAttribute, true);

                    if (attr != null)
                        methods.Add((method, attr));
                }
            }

            return methods;
        }
    }
}*/