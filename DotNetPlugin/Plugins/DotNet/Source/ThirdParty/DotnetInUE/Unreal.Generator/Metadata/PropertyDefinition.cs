// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Immutable;
using Unreal.Generation;

namespace Unreal.Metadata
{
    public partial class PropertyDefinition : MemberDefinition
    {
        /// <summary>
        /// Type of the property.
        /// </summary>
        public QualifiedTypeReference Type { get; }

        /// <summary>
        /// Initializer for this property if any.
        /// </summary>
        public string? Initializer { get; }
        
        /// <summary>
        /// Type that contains this member.
        /// </summary>
        /// <remarks>For functions this cannot be null.</remarks>
        public new TypeDefinition EnclosingType => base.EnclosingType!;

        public PropertyDefinition(string name, Module module,
            ImmutableList<MetaAttribute>? metaAttributes, Documentation? documentation,
            string comments, TypeDefinition? enclosingType, SymbolVisibility visibility,
            SymbolAttributeFlags attributes, ImmutableArray<string> managedAttributes, QualifiedTypeReference type, string? initializer)
            : base(name, module, metaAttributes, documentation, comments, enclosingType, visibility, attributes, managedAttributes)
        {
            Type = type;
            Initializer = initializer;
        }
        
        public static PropertyDefinitionBuilder CreateBuilder(TypeDefinition declaringType, string name)
        {
            return new(declaringType.Module, name, declaringType);
        }
    }
}