// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{Type} {Name}")]
    public class ParameterDefinition : TransferableDefinition
    {
        public string Name { get; }

        public string? DefaultValue { get; }

        public ParameterDefinition(string name, ITypeInfo type,
            ManagedTransferType transferType = ManagedTransferType.ByValue, string? defaultValue = null,
            ITypeMarshaller? marshaller = null)
            : this(name, new QualifiedTypeReference(type, transferType), defaultValue, marshaller)
        { }

        public ParameterDefinition(string name, QualifiedTypeReference type, string? defaultValue = null,
            ITypeMarshaller? marshaller = null)
            : base(type, marshaller)
        {
            DefaultValue = defaultValue;
            Name = name;
        }
    }
}