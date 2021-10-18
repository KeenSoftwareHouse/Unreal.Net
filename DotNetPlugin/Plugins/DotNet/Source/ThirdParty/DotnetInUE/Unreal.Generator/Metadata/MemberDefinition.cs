// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Immutable;
using Unreal.Generation;

namespace Unreal.Metadata
{
    public class MemberDefinition
    {
        /// <summary>
        /// Name of this member.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Module where this member is defined.
        /// </summary>
        public readonly Module Module;

        // Only UE Attributes
        public ImmutableList<MetaAttribute>? MetaAttributes { get; }

        // Only managed attributes
        public ImmutableArray<string> ManagedAttributes { get; }

        public Documentation? Documentation { get; }

        // Additional non documentation comments.
        public string Comments { get; }

        /// <summary>
        /// Type that contains this member.
        /// </summary>
        public TypeDefinition? EnclosingType { get; }

        public SymbolVisibility Visibility { get; }

        public SymbolAttributeFlags Attributes { get; }

        public MemberDefinition(string name, Module module, ImmutableList<MetaAttribute>? metaAttributes,
            Documentation? documentation, string comments, TypeDefinition? enclosingType, SymbolVisibility visibility,
            SymbolAttributeFlags attributes, ImmutableArray<string> managedAttributes)
        {
            Name = name;
            Module = module;
            MetaAttributes = metaAttributes;
            Documentation = documentation;
            Comments = comments;
            EnclosingType = enclosingType;
            Visibility = visibility;
            Attributes = attributes;
            ManagedAttributes = managedAttributes;
        }

        public static MemberDefinitionBuilder CreateBuilder(Module module, string name,
            TypeDefinition? declaringType = null)
        {
            return new(module, name, declaringType);
        }
    }
}