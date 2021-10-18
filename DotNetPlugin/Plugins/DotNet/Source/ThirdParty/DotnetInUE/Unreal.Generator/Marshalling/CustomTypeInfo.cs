// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Unreal.Generation;
using Unreal.Metadata;
using Unreal.Util;
using TypeKind = Unreal.Metadata.TypeKind;

namespace Unreal.Marshalling
{
    public class CustomTypeInfo : ITypeInfo
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
        
        public bool IsGenericType => false;

        public CustomTypeInfo(GenerationContext context, INamedTypeSymbol symbol, NativeTypeAttributeData data,
            in MarshalFormats formats)
        {
            TypeSymbol = symbol;
            ManagedName = symbol.Name;
            NativeName = data.NativeTypeName;

            Namespace = symbol.ContainingNamespace.GetFullMetadataName();

            if (symbol.GetAttribute(context.GetSymbol<HeaderAttribute>()) is { } attr)
                Header = (string) attr.ConstructorArguments[0].Value!;
            else
                Header = "";

            Kind = symbol.GetKind();

            Module = context.GetModule(symbol.ContainingModule);
            TypicalArgumentType = data.MemoryKind == TypeMemoryKind.ReferenceType
                ? NativeTransferType.ByPointer
                : NativeTransferType.ByValue;

            DefaultMarshaller = new CustomTypeMarshaller(formats, data.GetTransformedType(context));
        }
    }
}