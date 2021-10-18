// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Unreal.Generation;

namespace Unreal.Metadata
{
    public class MemberDefinitionBuilder<TBuilder>
        where TBuilder : MemberDefinitionBuilder<TBuilder>
    {
        public readonly string Name;
        
        public readonly Module Module;

        public readonly TypeDefinition? DeclaringType;
        
        private List<MetaAttribute>? m_metaAttributes;

        public Documentation? Documentation;

        public string Comments = ""; // Additional non documentation comments.


        public SymbolVisibility Visibility;

        public SymbolAttributeFlags Attributes;

        public readonly List<string> ManagedAttributes = new();

        protected MemberDefinitionBuilder(Module module, string name, TypeDefinition? declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
            Module = module;

            Visibility = SymbolVisibility.Public;

            Attributes = 0;
        }

        protected TBuilder Get() => (TBuilder) this;

        protected ImmutableList<MetaAttribute>? GetMetaAttributes() => m_metaAttributes?.ToImmutableList();

        /// <summary>
        /// Mask this symbol as annotated with the relevant UMeta tag.
        /// </summary>
        /// <returns></returns>
        public TBuilder WithUMeta()
        {
            m_metaAttributes ??= new List<MetaAttribute>();
            return Get();
        }
        
        public TBuilder WithMetaAttribute(MetaAttribute attribute)
        {
            m_metaAttributes ??= new List<MetaAttribute>();
            m_metaAttributes.Add(attribute);
            return Get();
        }

        public TBuilder WithMetaAttributes(params MetaAttribute[] attribute)
        {
            m_metaAttributes ??= new List<MetaAttribute>();
            m_metaAttributes.AddRange(attribute);
            return Get();
        }

        public TBuilder WithMetaAttributes(IEnumerable<MetaAttribute> attribute)
        {
            m_metaAttributes ??= new List<MetaAttribute>();
            m_metaAttributes.AddRange(attribute);
            return Get();
        }

        public TBuilder WithDocumentation(Documentation documentation)
        {
            Documentation = documentation;
            return Get();
        }

        public TBuilder WithDocumentationFromNative(string nativeDocs)
        {
            Documentation = Documentation.FromNative(nativeDocs);
            return Get();
        }

        public TBuilder WithDocumentationFromManaged(string managedDocs)
        {
            Documentation = Documentation.FromManaged(managedDocs);
            return Get();
        }

        public TBuilder WithComment(string comment)
        {
            Comments = comment;
            return Get();
        }

        public TBuilder AppendComment(string comment)
        {
            if (string.IsNullOrEmpty(Comments))
                Comments = comment;
            else
                Comments = $"{Comments}\n{comment}";
            return Get();
        }

        public TBuilder WithVisibility(SymbolVisibility visibility)
        {
            Visibility = visibility;
            return Get();
        }

        public TBuilder WithAttributes(SymbolAttributeFlags attributes)
        {
            Attributes = attributes;
            return Get();
        }

        public TBuilder WithAttribute(SymbolAttribute attribute)
        {
            Attributes |= attribute.ToFlags();
            return Get();
        }
        
        public TBuilder WithManagedAttribute(string attribute)
        {
            ManagedAttributes.Add(attribute);
            return Get();
        }
    }

    public class MemberDefinitionBuilder : MemberDefinitionBuilder<MemberDefinitionBuilder>
    {
        public MemberDefinitionBuilder(Module module, string name, TypeDefinition? declaringType)
            : base(module, name, declaringType)
        { }

        public MemberDefinition Build()
        {
            return new(Name, Module, GetMetaAttributes(), Documentation, Comments, DeclaringType,
                Visibility, Attributes, ManagedAttributes.ToImmutableArray());
        }
    }
}