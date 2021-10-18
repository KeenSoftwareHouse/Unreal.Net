// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Unreal.Generation;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    public class FunctionDefinitionBuilder<TBuilder> : MemberDefinitionBuilder<TBuilder>
        where TBuilder : FunctionDefinitionBuilder<TBuilder>
    {
        protected TransferableDefinition Return = TransferableDefinition.Void;

        protected readonly List<ParameterDefinition> Parameters = new();

        protected string? EntryPointName;

        protected MethodSpecialType SpecialMethod = MethodSpecialType.None;

        protected FunctionDefinitionBuilder(Module module, string name, TypeDefinition declaringType)
            : base(module, name, declaringType)
        { }

        public TBuilder WithReturn(ITypeInfo @return,
            ManagedTransferType transfer = ManagedTransferType.ByValue, ITypeMarshaller? customMarshaller = null)
        {
            Return = new TransferableDefinition(new QualifiedTypeReference(@return, transfer), customMarshaller);
            return Get();
        }

        public TBuilder WithReturn(QualifiedTypeReference @return, ITypeMarshaller? customMarshaller = null)
        {
            Return = new TransferableDefinition(@return, customMarshaller);
            return Get();
        }

        public TBuilder WithReturn<T>(ManagedTransferType transfer = ManagedTransferType.ByValue,
            ITypeMarshaller? customMarshaller = null)
        {
            Return = new TransferableDefinition(new QualifiedTypeReference(ManagedTypeInfo.GetType<T>(), transfer),
                customMarshaller);
            return Get();
        }

        public TBuilder WithParameters(IEnumerable<ParameterDefinition> parameters)
        {
            Parameters.Clear();
            Parameters.AddRange(parameters);
            return Get();
        }

        public TBuilder WithParameter(ParameterDefinition parameter)
        {
            Parameters.Add(parameter);
            return Get();
        }

        public TBuilder WithParameter(string name, QualifiedTypeReference type, string? defaultValue = null,
            ITypeMarshaller? customMarshaller = null)
        {
            Parameters.Add(new ParameterDefinition(name, type, defaultValue, customMarshaller));
            return Get();
        }

        public TBuilder WithParameter(string name, ITypeInfo type,
            ManagedTransferType transfer = ManagedTransferType.ByValue, string? defaultValue = null,
            ITypeMarshaller? customMarshaller = null)
        {
            Parameters.Add(new ParameterDefinition(name, new QualifiedTypeReference(type, transfer), defaultValue,
                customMarshaller));
            return Get();
        }

        public TBuilder WithParameter<T>(string name,
            ManagedTransferType transfer = ManagedTransferType.ByValue, string? defaultValue = null,
            ITypeMarshaller? customMarshaller = null)
        {
            Parameters.Add(new ParameterDefinition(name,
                new QualifiedTypeReference(ManagedTypeInfo.GetType<T>(), transfer), defaultValue, customMarshaller));
            return Get();
        }

        public TBuilder WithEntryPointName(string? entryPointName)
        {
            EntryPointName = entryPointName;
            return Get();
        }

        public TBuilder WithSpecialMethod(MethodSpecialType specialMethod)
        {
            SpecialMethod = specialMethod;
            return Get();
        }
    }

    public class FunctionDefinitionBuilder : FunctionDefinitionBuilder<FunctionDefinitionBuilder>
    {
        public FunctionDefinitionBuilder(Module module, string name, TypeDefinition declaringType)
            : base(module, name, declaringType)
        { }

        public FunctionDefinition Build()
        {
            if (EntryPointName == null)
                EntryPointName = NameMangler.MangleMethodName(DeclaringType!, Name,
                    Parameters);

            return new FunctionDefinition(Name, Module, GetMetaAttributes(), Documentation, Comments,
                DeclaringType, Visibility, Attributes, ManagedAttributes.ToImmutableArray(), EntryPointName, Return,
                Parameters.ToImmutableArray(), SpecialMethod);
        }
    }
}