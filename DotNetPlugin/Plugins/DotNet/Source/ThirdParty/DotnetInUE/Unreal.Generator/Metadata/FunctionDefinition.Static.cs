// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.ErrorHandling;
using Unreal.Generation;
using Unreal.Marshalling;
using Unreal.NativeMetadata;

namespace Unreal.Metadata
{
    public partial class FunctionDefinition
    {
        public static FunctionDefinitionBuilder PrepareFromManaged(GenerationContext context,
            TypeDefinition enclosingType, MethodDeclarationSyntax syntax, SemanticModel model)
        {
            // Collect Name
            // ============
            var name = syntax.Identifier.ValueText;

            var builder = CreateBuilder(enclosingType, name);

            if (syntax.Parent is not ClassDeclarationSyntax @class)
                throw new SourceException("Only class level methods can be processed.", syntax);

            // Collect Meta Attributes
            // =======================
            builder.WithMetaAttributes(MetaAttribute.ExtractAttributes(context, syntax.AttributeLists, model));

            // Collect Attributes
            // ==================
            if (syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
                builder.WithAttribute(SymbolAttribute.Static);
            if (syntax.Modifiers.Any(SyntaxKind.VirtualKeyword))
                builder.WithAttribute(SymbolAttribute.Virtual);
            if (syntax.Modifiers.Any(SyntaxKind.FinallyKeyword))
                builder.WithAttribute(SymbolAttribute.Final);
            if (syntax.Modifiers.Any(SyntaxKind.OverrideKeyword))
                builder.WithAttribute(SymbolAttribute.Override);

            builder.WithAttribute(SymbolAttribute.Unsafe);

            // Collect Parameters
            // ==================

            // Prepend this parameter.
            ISymbol? symbol;
            foreach (var param in syntax.ParameterList.Parameters)
            {
                // TODO: Defaults 

                symbol = ModelExtensions.GetSymbolInfo(model, param.Type!).Symbol;
                var parameterType = context.ResolveSymbol(param.Type!, symbol!);

                builder.WithParameter(param.Identifier.Text, parameterType);
            }

            // Collect Return Type
            // ===================
            symbol = ModelExtensions.GetSymbolInfo(model, syntax.ReturnType).Symbol;
            var returnType = context.ResolveSymbol(syntax.ReturnType, symbol!);
            // TODO: Pull transfer type from modifiers.
            builder.WithReturn(returnType);

            return builder;
        }

        public static FunctionDefinitionBuilder PrepareFromNative(GenerationContext context,
            TypeDefinition enclosingType, UEFunction function)
        {
            var builder = CreateBuilder(enclosingType, function.Name);

            // TODO: Meta Attributes
            //builder.WithAttributes();

            bool isStatic = (function.Flags & FunctionFlags.Static) != 0;

            if (isStatic)
                builder.WithAttribute(SymbolAttribute.Static);
            if ((function.Flags & FunctionFlags.Final) == 0)
                builder.WithAttribute(SymbolAttribute.Virtual);
            // TODO: To find if the function is final or override we must look at the type's hierarchy and see if a previous virtual definition existed.

            builder.WithAttribute(SymbolAttribute.Unsafe);

            foreach (var parameter in function.Parameters)
            {
                // TODO: Defaults (they are available as meta attributes)

                var type = context.TypeResolver.Resolve(parameter);
                builder.WithParameter(parameter.Name, type);
            }

            var returnType = context.TypeResolver.Resolve(function.GetReturn());

            // TODO: Pull transfer type from modifiers.
            builder.WithReturn(returnType);

            builder.AppendComment($"Flags = {function.Flags}");

            if (function.Meta.TryGetValue("Comment", out var doc))
                builder.WithDocumentationFromNative(doc);

            return builder;
        }
    }
}