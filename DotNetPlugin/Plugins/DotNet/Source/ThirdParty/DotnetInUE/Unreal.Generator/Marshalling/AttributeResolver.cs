// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.ErrorHandling;
using Unreal.Util;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Static helper containing methods to resolve various attribute syntaxes to their actual instances. 
    /// </summary>
    public static class AttributeResolver
    {
        /// <summary>
        /// Decode a <see cref="NativeModulesAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static NativeModulesAttribute DecodeNativeModulesAttribute(SemanticModel model, AttributeSyntax syntax)
        {
            if (syntax.ArgumentList == null)
                return new NativeModulesAttribute();

            string[] arguments = new string[syntax.ArgumentList.Arguments.Count];

            for (var i = 0; i < syntax.ArgumentList.Arguments.Count; i++)
            {
                var arg = syntax.ArgumentList.Arguments[i];

                var argumentValue = model.GetConstantValue(arg.Expression);

                if (argumentValue.HasValue)
                    arguments[i] = (string) argumentValue.Value!;
                else
                    throw new SourceException("Only string literals are supported as arguments for this attribute.",
                        arg);
            }

            return new NativeModulesAttribute(arguments);
        }

        /// <summary>
        /// Decode a <see cref="NativeModulesAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static NativeModulesAttribute DecodeNativeModulesAttribute(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 0)
                return new NativeModulesAttribute();

            // Only one array argument.
            var arg = attribute.ConstructorArguments[0];

            var values = arg.Values;

            string[] arguments = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
                arguments[i] = (string) values[i].Value!;

            return new NativeModulesAttribute(arguments);
        }

        /// <summary>
        /// Decode a <see cref="NativeTypeMappingsAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static INamedTypeSymbol[] DecodeModuleMarshallersAttribute(SemanticModel model,
            AttributeSyntax syntax)
        {
            try
            {
                if (syntax.ArgumentList == null)
                    return Array.Empty<INamedTypeSymbol>();

                INamedTypeSymbol[] arguments = new INamedTypeSymbol[syntax.ArgumentList.Arguments.Count];

                for (var i = 0; i < syntax.ArgumentList.Arguments.Count; i++)
                {
                    var arg = syntax.ArgumentList.Arguments[i];

                    arguments[i] =
                        (INamedTypeSymbol) model.GetTypeInfo(((TypeOfExpressionSyntax) arg.Expression).Type).Type!;
                }

                return arguments;
            }
            catch
            {
                throw new SourceException("Attribute arguments are malformed.", syntax);
            }
        }

        /// <summary>
        /// Decode a <see cref="NativeTypeMappingsAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static INamedTypeSymbol[] DecodeModuleMarshallersAttribute(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 0)
                return Array.Empty<INamedTypeSymbol>();

            // Only one array argument.
            var arg = attribute.ConstructorArguments[0];

            var values = arg.Values;

            INamedTypeSymbol[] arguments = new INamedTypeSymbol[values.Length];

            for (var i = 0; i < values.Length; i++)
                arguments[i] = (INamedTypeSymbol) values[i].Value!;

            return arguments;
        }

        /// <summary>
        /// Decode a <see cref="NativeTypeAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static NativeTypeAttributeData DecodeNativeTypeAttribute(SemanticModel model, AttributeSyntax syntax)
        {
            try
            {
                // TODO: This ignored "param:" parameters which can be specified out of order.
                var args = syntax.ArgumentList!.Arguments;

                var nativeName = (string) model.GetConstantValue(args[0].Expression).Value!;

                var propertyType = (string) model.GetConstantValue(args[1].Expression).Value!;

                var memoryKind = (TypeMemoryKind) (int) model.GetConstantValue(args[2].Expression).Value!;

                // FOr types we can only really use the typeof expression directly.
                var baseIntermediateType =
                    (INamedTypeSymbol) model.GetTypeInfo(((TypeOfExpressionSyntax) args[3].Expression).Type).Type!;

                var transformation = (TypeTransformation) (int) model.GetConstantValue(args[4].Expression).Value!;

                return new NativeTypeAttributeData(nativeName, propertyType, memoryKind, baseIntermediateType,
                    transformation);
            }
            catch
            {
                throw new SourceException("Attribute arguments are malformed.", syntax);
            }
        }

        /// <summary>
        /// Decode a <see cref="NativeTypeAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <returns></returns>
        public static NativeTypeAttributeData DecodeNativeTypeAttribute(AttributeData data)
        {
            var args = data.ConstructorArguments;

            var nativeName = (string) args[0].Value!;

            var propertyType = (string) args[1].Value!;

            var memoryKind = (TypeMemoryKind) (int) args[2].Value!;

            var baseIntermediateType = (INamedTypeSymbol) args[3].Value!;

            var transformation = (TypeTransformation) (int) args[4].Value!;

            return new NativeTypeAttributeData(nativeName, propertyType, memoryKind, baseIntermediateType,
                transformation);
        }

        /// <summary>
        /// Decode a <see cref="MarshalFormatsAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static MarshalFormatsAttribute DecodeMarshalFormatsAttribute(SemanticModel model, AttributeSyntax syntax)
        {
            try
            {
                var args = syntax.ArgumentList!.Arguments;

                string[] argumentNames =
                {
                    "fromManagedToIntermediate",
                    "fromIntermediateToManaged",
                    "fromNativeToIntermediate",
                    "fromIntermediateToNative",
                };

                string requiredHeader = "";
                string requiredNamespace = "";

                string fromManagedToIntermediate = "";
                string fromIntermediateToManaged = "";
                string fromNativeToIntermediate = "";
                string fromIntermediateToNative = "";

                for (int i = 0; i < args.Count; ++i)
                {
                    string paramName;
                    if (args[i].NameEquals is { } ne)
                    {
                        if (ne.Name.Identifier.ValueText == "RequiredHeader")
                            requiredHeader = (string?) model.GetConstantValue(args[i].Expression).Value ?? "";

                        if (ne.Name.Identifier.ValueText == "RequiredNamespace")
                            requiredNamespace = (string?) model.GetConstantValue(args[i].Expression).Value ?? "";

                        continue;
                    }
                    else if (args[i].NameColon is { } nc)
                    {
                        paramName = nc.Name.Identifier.ValueText;
                    }
                    else
                    {
                        paramName = argumentNames[i];
                    }

                    var value = (string?) model.GetConstantValue(args[i].Expression).Value ?? "{0}";
                    switch (paramName)
                    {
                        case "fromManagedToIntermediate":
                            fromManagedToIntermediate = value;
                            break;
                        case "fromIntermediateToManaged":
                            fromIntermediateToManaged = value;
                            break;
                        case "fromNativeToIntermediate":
                            fromNativeToIntermediate = value;
                            break;
                        case "fromIntermediateToNative":
                            fromIntermediateToNative = value;
                            break;
                    }
                }

                return new MarshalFormatsAttribute(fromManagedToIntermediate, fromIntermediateToManaged,
                    fromNativeToIntermediate, fromIntermediateToNative)
                {
                    RequiredHeader = requiredHeader,
                    RequiredNamespace = requiredNamespace
                };
            }
            catch
            {
                throw new SourceException("Attribute arguments are malformed.", syntax);
            }
        }

        /// <summary>
        /// Decode a <see cref="MarshalFormatsAttribute"/> construction from an attribute syntax.
        /// </summary>
        /// <returns></returns>
        public static MarshalFormatsAttribute DecodeMarshalFormatsAttribute(AttributeData data)
        {
            string requiredHeader = "";
            string requiredNamespace = "";

            string fromManagedToIntermediate = (string?) data.ConstructorArguments[0].Value ?? "{0}";
            string fromIntermediateToManaged = (string?) data.ConstructorArguments[1].Value ?? "{0}";
            string fromNativeToIntermediate = (string?) data.ConstructorArguments[2].Value ?? "{0}";
            string fromIntermediateToNative = (string?) data.ConstructorArguments[3].Value ?? "{0}";

            foreach (var arg in data.NamedArguments)
            {
                if (arg.Key == "RequiredHeader")
                    requiredHeader = (string?) arg.Value.Value ?? "";
                if (arg.Key == "RequiredNamespace")
                    requiredNamespace = (string?) arg.Value.Value ?? "";
            }

            return new MarshalFormatsAttribute(fromManagedToIntermediate, fromIntermediateToManaged,
                fromNativeToIntermediate, fromIntermediateToNative)
            {
                RequiredHeader = requiredHeader,
                RequiredNamespace = requiredNamespace
            };
        }
    }
}