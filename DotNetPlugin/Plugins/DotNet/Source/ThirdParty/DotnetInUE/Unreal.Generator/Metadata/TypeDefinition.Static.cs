// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.Generation;
using Unreal.Marshalling;
using Unreal.NativeMetadata;
using Unreal.Util;

namespace Unreal.Metadata
{
    public partial class TypeDefinition
    {
        /// <summary>
        /// Create a type representation from the syntax of a class declaration.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="context"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static TypeDefinitionBuilder PrepareFromManaged(Module module, GenerationContext context,
            TypeDeclarationSyntax syntax)
        {
            var model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
            var name = syntax.Identifier.Text;

            ITypeInfo? parentType = null;

            var baseSyntax = syntax.BaseList?.Types.FirstOrDefault();
            if (baseSyntax != null)
            {
                if (model.GetTypeInfo(baseSyntax.Type).Type is INamedTypeSymbol symbol)
                    parentType = context.TypeResolver.Resolve(symbol);
            }

            var writer = CreateBuilder(module, name);

            writer.WithMetaAttributes(MetaAttribute.ExtractAttributes(context, syntax.AttributeLists, model))
                .WithNamespace(syntax.GetNamespace())
                .WithCosmeticName(name.Substring(1))
                .WithHeader($"{name}.h")
                .WithManagedAttribute($"Header(\"{name}.h\")")
                .WithManagedAttribute($"Module(\"{module.ModuleId}\")")
                .WithTypeSymbol((INamedTypeSymbol?)model.GetDeclaredSymbol(syntax))
                .WithDefaultMarshaller(ManagedUObjectMarshaller.Instance)
                .WithTypicalArgumentType(NativeTransferType.ByPointer)
                .WithKind(TypeKind.Class);

            if (parentType != null)
                writer.WithParentType(parentType);

            return writer;
        }

        /// <summary>
        /// Create a type representation from the syntax of a class declaration.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="context"></param>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public static TypeDefinitionBuilder PrepareFromNative(Module module, GenerationContext context,
            UEField typeInfo)
        {
            var name = typeInfo.Name;

            ITypeInfo? parentType = null;

            TypeKind kind = typeInfo.Kind switch
            {
                UETypeKind.UObject => TypeKind.Class,
                UETypeKind.UInterface => TypeKind.Interface,
                UETypeKind.UStruct => TypeKind.Struct,
                UETypeKind.UEnum => TypeKind.Enum,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (typeInfo is UEStruct structInfo)
            {
                if (structInfo.Parent is { } p)
                    parentType = context.GetNativeTypeInfo(new QualifiedNativeTypeName(p.Module, p.CppName));

                name = structInfo.CppName;
            }

            var nativeName = name;
            if (typeInfo is UEEnum enumInfo)
                nativeName = enumInfo.CppName;

            if (!typeInfo.Meta.TryGetValue("IncludePath", out string header))
            {
                if (typeInfo.Meta.TryGetValue("ModuleRelativePath", out header))
                {
                    var publicPrefix = "Public/";
                    var classesPrefix = "Classes/";

                    if (header.StartsWith(publicPrefix))
                        header = header.Substring(publicPrefix.Length);
                    else if (header.StartsWith(classesPrefix))
                        header = header.Substring(classesPrefix.Length);
                }
                else
                {
                    header = ""; // Don't allow null.
                }
            }

            var builder = CreateBuilder(module, name);

            builder.WithUMeta()
                .WithCosmeticName(typeInfo.Name)
                .WithNativeName(nativeName)
                .WithHeader(header)
                .WithNativeModule(typeInfo.Module)
                .WithManagedAttribute($"Module(\"{typeInfo.Module}\")")
                .WithNamespace("Unreal." + typeInfo.Module) // TODO: Check if module is indeed engine module.
                .WithKind(kind)
                .WithTypicalArgumentType(kind.IsValueType() ? NativeTransferType.ByValue : NativeTransferType.ByPointer)
                .WithVisibility(SymbolVisibility.Public);

            if (parentType != null)
            {
                builder.WithParentType(parentType)
                    .WithIsManagedUObject(parentType.IsManagedUObject);
                if (parentType.IsManagedUObject)
                    builder.WithMetaAttribute(context.GetMetaAttribute(MetaAttribute.ManagedTypeAttributeName));
            }

            if (kind.IsReferenceType())
            {
                if (parentType?.IsManagedUObject == true)
                    builder.WithDefaultMarshaller(ManagedUObjectMarshaller.Instance);
                else
                    builder.WithDefaultMarshaller(NativeUObjectMarshaller.Instance);
            }

            if (header != "")
                builder.WithManagedAttribute($"Header(\"{header}\")");

            if (typeInfo.Meta.TryGetValue("Comment", out var comment))
                builder.WithDocumentation(Documentation.FromNative(comment));

            return builder;
        }
    }
}