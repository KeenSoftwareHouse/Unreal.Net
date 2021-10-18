// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.ErrorHandling;
using Unreal.Generation;
using Unreal.Metadata;
using Unreal.NativeMetadata;
using Unreal.Util;

namespace Unreal.Marshalling
{
    public class TypeResolver
    {
        public readonly GenerationContext Context;

        private readonly Dictionary<string, PropertyTypeResolver> m_resolverPerPropertyType = new();

        private readonly Dictionary<ITypeSymbol, ManagedTypeResolver> m_resolverPerTypeAttribute = new();

        private readonly DefaultPropertyTypeResolver m_defaultPropertyResolver;

        private readonly DefaultManagedTypeResolver m_defaultManagedTypeResolver;

        private readonly CustomManagedTypeResolver m_customResolver;

        public TypeResolver(GenerationContext context)
        {
            Context = context;

            m_defaultPropertyResolver = new DefaultPropertyTypeResolver();
            m_defaultPropertyResolver.Register(this);

            m_defaultManagedTypeResolver = new DefaultManagedTypeResolver();
            m_defaultManagedTypeResolver.Register(this);

            m_customResolver = new CustomManagedTypeResolver();
            RegisterManagedResolver(context.NativeTypeAttribute, m_customResolver);
        }

        #region Registration

        public void RegisterPropertyResolver(string propertyType, PropertyTypeResolver resolver)
        {
            m_resolverPerPropertyType.Add(propertyType, resolver);
            resolver.Register(this);
        }

        /// <summary>
        /// Register a managed type resolver bound to a given type attribute.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="resolver"></param>
        public void RegisterManagedResolver(ITypeSymbol attributeType, ManagedTypeResolver resolver)
        {
            m_resolverPerTypeAttribute.Add(attributeType, resolver);
            if (resolver.Container != this)
                resolver.Register(this);
        }

        /// <summary>
        /// Register any type mappings from types in the module.
        /// </summary>
        /// <param name="module"></param>
        public void RegisterResolversFromMetadataModule(IModuleSymbol module)
        {
            if (!module.TryGetAttribute(Context.NativeTypeMappingsAttribute, out var data))
                return;

            var marshallers = AttributeResolver.DecodeModuleMarshallersAttribute(data!);

            foreach (var marshaller in marshallers)
            {
                var mappingData =
                    AttributeResolver.DecodeNativeTypeAttribute(marshaller.GetAttribute(Context.NativeTypeAttribute)!);
                var marshallingConversion = marshaller.GetAttribute(Context.MarshalFormatsAttribute) is { } mf
                    ? AttributeResolver.DecodeMarshalFormatsAttribute(mf)
                    : MarshalFormatsAttribute.Default;

                RegisterCustomMapping(marshaller, mappingData, marshallingConversion);
            }
        }

        /// <summary>
        /// Register any custom type mappings in the provided type declarations.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="model"></param>
        /// <param name="syntax"></param>
        /// <param name="nativeTypeAttribute"></param>
        public void RegisterResolverFromSyntax(GenerationContext context, SemanticModel model,
            TypeDeclarationSyntax syntax, AttributeSyntax nativeTypeAttribute)
        {
            var mappingData = AttributeResolver.DecodeNativeTypeAttribute(model, nativeTypeAttribute);
            MarshalFormatsAttribute marshallingFormats;

            var mf = syntax.AttributeLists.GetAttributeOfType(model, context.MarshalFormatsAttribute);
            if (mf == null)
                marshallingFormats = MarshalFormatsAttribute.Default;
            else
                marshallingFormats = AttributeResolver.DecodeMarshalFormatsAttribute(model, mf);

            var type = (INamedTypeSymbol) model.GetDeclaredSymbol(syntax)!;
            RegisterCustomMapping(type, mappingData, marshallingFormats);
        }

        /// <summary>
        /// Register a custom type mapping.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="formats"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void RegisterCustomMapping(INamedTypeSymbol type, NativeTypeAttributeData data,
            MarshalFormatsAttribute formats)
        {
            if (!m_resolverPerPropertyType.TryGetValue(data.PropertyType, out var resolver))
            {
                m_resolverPerPropertyType[data.PropertyType] = resolver = new CustomPropertyTypeResolver();
                resolver.Register(this);
            }
            else if (resolver is not CustomPropertyTypeResolver)
            {
                throw new InvalidOperationException(
                    $"Cannot register custom type conversion for properties of type {data.PropertyType}, a built-in type resolver already exists for that property type.");
            }

            CustomTypeMapping mapping;
            if (type.IsGenericType)
                mapping = new GenericTypeMapping(this, type, data, formats);
            else
                mapping = new ConcreteTypeMapping(Context, type, data, formats);

            ((CustomPropertyTypeResolver) resolver).AddMapping(mapping);
            m_customResolver.AddMapping(mapping);
        }

        #endregion

        #region Resolve

        public QualifiedTypeReference Resolve(UEProperty property)
        {
            if (!m_resolverPerPropertyType.TryGetValue(property.PropertyType, out var resolver))
                resolver = m_defaultPropertyResolver;

            var type = resolver.Resolve(property);

            // Throw if not resolved.
            if (type == null)
                throw new MissingSymbolException(property.GetPrettyType());

            ManagedTransferType transfer = ManagedTransferType.ByValue;
            if ((property.Flags & PropertyFlags.ReturnParm) == 0)
            {
                if ((property.Flags & PropertyFlags.ReferenceParm) != 0)
                {
                    if ((property.Flags & PropertyFlags.ConstParm) != 0)
                        transfer = ManagedTransferType.In;
                    else
                        transfer = ManagedTransferType.Ref;
                }
                else if ((property.Flags & PropertyFlags.OutParm) != 0)
                {
                    transfer = ManagedTransferType.Out;
                }
            }

            return new QualifiedTypeReference(type, transfer);
        }

        public QualifiedTypeReference Resolve(TypeReferenceBase reference)
        {
            if (reference is TypeNameReference name)
            {
                return new QualifiedTypeReference(
                    Context.GetNativeTypeInfo(new QualifiedNativeTypeName(name.Module, name.CppName)));
            }

            return Resolve(((TypePropertyReference) reference).Property);
        }

        public ITypeInfo Resolve(ITypeSymbol typeSymbol, SyntaxNode? contextNode = null)
        {
            // Try get cached.
            if (Context.TryGetSymbolTypeInfo(typeSymbol, out var type))
                return type!;

            ManagedTypeResolver? resolver = null;
            foreach (var attribute in typeSymbol.GetAttributes())
            {
                if (m_resolverPerTypeAttribute.TryGetValue(attribute.AttributeClass!, out resolver))
                    break;
            }

            resolver ??= m_defaultManagedTypeResolver;

            type = resolver.Resolve(typeSymbol);

            // Throw if not resolved.
            if (type == null)
                throw new MissingSymbolException(typeSymbol.ToDisplayString(), location: contextNode?.GetLocation());

            return type;
        }

        #endregion
    }
}