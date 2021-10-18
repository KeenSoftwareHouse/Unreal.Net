// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Metadata;
using Unreal.Util;
using TypeKind = Unreal.Metadata.TypeKind;

namespace Unreal.Marshalling
{
    public class CustomGenericTypeInfo : IGenericTypeInfo
    {
        public string ManagedName { get; }
        public string ManagedSourceName => ManagedName;
        public string NativeName { get; }
        public ITypeInfo? ParentType => null;
        public Type? ManagedType => null;
        public INamedTypeSymbol? TypeSymbol { get; }
        public bool IsManagedUObject => false;
        public string Namespace { get; }
        public string Header { get; }
        public TypeKind Kind { get; }
        public Module Module { get; }
        public NativeTransferType TypicalArgumentType { get; }
        public ITypeMarshaller? DefaultMarshaller { get; }
        public string NativeModule => "";

        public bool IsGenericType => true;

        public CustomGenericTypeInfo(GenerationContext context, INamedTypeSymbol genericTypeDefinition,
            NativeTypeAttributeData data,
            in MarshalFormats formats, IList<ITypeInfo> genericTypeArguments, INamedTypeSymbol? concreteType = null)
        {
            TypeSymbol = concreteType;
            ManagedName =
                $"{genericTypeDefinition.Name}<{string.Join(", ", genericTypeArguments.Select(x => x.ManagedName))}>";
            NativeName =
                $"{data.NativeTypeName}<{string.Join(", ", genericTypeArguments.Select(x => x.NativeName))}>";

            Namespace = genericTypeDefinition.ContainingNamespace.GetFullMetadataName();

            if (genericTypeDefinition.GetAttribute(context.GetSymbol<HeaderAttribute>()) is { } attr)
                Header = (string) attr.ConstructorArguments[0].Value!;
            else
                Header = "";

            Kind = genericTypeDefinition.GetKind();

            Module = context.GetModule(genericTypeDefinition.ContainingModule);
            TypicalArgumentType = data.MemoryKind == TypeMemoryKind.ReferenceType
                ? NativeTransferType.ByPointer
                : NativeTransferType.ByValue;

            DefaultMarshaller = new CustomTypeMarshaller(formats, data.GetTransformedType(context));

            GenericArguments = genericTypeArguments.ToImmutableArray();
        }

        // Not used.
        public ImmutableArray<string> GenericParameters { get; } = ImmutableArray<string>.Empty;

        public ImmutableArray<ITypeInfo> GenericArguments { get; }
    }
}