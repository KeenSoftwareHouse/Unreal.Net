// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public abstract class MemberWriter<TMember> : MemberWriter
        where TMember : MemberDefinition
    {
        public readonly TMember Member;

        public override MemberDefinition MemberDefinition => Member;

        protected MemberWriter(TMember member, MemberCodeComponentFlags components = MemberCodeComponentFlags.None)
            : base(components)
        {
            Member = member;
        }
    }

    public abstract class MemberWriter : AbstractWriter
    {
        public abstract MemberDefinition MemberDefinition { get; }

        public override string Name => MemberDefinition.Name;

        public override Module Module => MemberDefinition.Module;

        public MemberWriter(MemberCodeComponentFlags components)
        {
            Components = components;
            MetaSpace = CodespaceFlags.All;
        }

        protected virtual void WriteComments(CodeWriter writer, Codespace space)
        {
            if (MemberDefinition.Comments != "")
            {
                using (writer.Indent("// ")) // Prepend each line with a line comment.
                    writer.WriteLine(MemberDefinition.Comments);
            }

            if (MemberDefinition.Documentation != null)
            {
                if (space == Codespace.Managed)
                    writer.WriteLine(MemberDefinition.Documentation.FormatManaged());
                else
                    writer.WriteLine(MemberDefinition.Documentation.FormatNative());
            }
        }

        protected void WriteAnnotations(CodeWriter writer, Codespace space, string? mainAttributeName = null)
        {
            if (MetaSpace.HasSpace(space))
            {
                if (MemberDefinition.MetaAttributes != null)
                {
                    if (space == Codespace.Native)
                    {
                        // ENums use only the raw UMETA tag.
                        mainAttributeName ??= "UMeta";

                        writer.Write($"{mainAttributeName.ToUpper()}(");

                        MetaAttribute.FormatNative(writer, MemberDefinition.MetaAttributes);

                        writer.WriteLine(")");
                    }
                    else
                    {
                        // In managed code enums use not tag at all.
                        writer.Write("[");

                        if (!string.IsNullOrWhiteSpace(mainAttributeName))
                        {
                            writer.Write(mainAttributeName!);
                            if (MemberDefinition.MetaAttributes.Count > 0)
                                writer.Write(", ");
                        }

                        MetaAttribute.FormatManaged(writer, MemberDefinition.MetaAttributes);

                        writer.WriteLine("]");
                    }
                }
            }

            if (space == Codespace.Managed)
            {
                if (MemberDefinition.ManagedAttributes.Length > 0)
                    writer.WriteLine($"[{string.Join(", ", MemberDefinition.ManagedAttributes)}]");
            }
        }

        protected void WriteManagedVisibilityAndAttributes(CodeWriter writer)
        {
            writer.Write($"{MemberDefinition.Visibility.Format()} ");

            if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.New))
                writer.Write("new ");

            if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.Static))
                writer.Write("static ");

            if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.Unsafe))
                writer.Write("unsafe ");

            if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.Virtual))
            {
                if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.Override))
                    writer.Write("override ");
                else if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.Final))
                    writer.Write("final ");
                else
                    writer.Write("virtual ");
            }
        }

        protected void WriteNativeMemberAttributes(CodeWriter writer)
        {
            if (MemberDefinition.Attributes.HasAttribute(SymbolAttribute.Static))
                writer.Write("static ");
        }
    }
}