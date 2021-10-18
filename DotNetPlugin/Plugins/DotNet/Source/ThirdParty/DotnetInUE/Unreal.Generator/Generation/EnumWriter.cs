// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class EnumWriter : TypeWriter
    {
        public bool IsFlags;

        public EnumWriter(TypeDefinition type, IEnumerable<EnumPropertyWriter> members, Codespace space, bool isFlags)
            : base(type, members)
        {
            Components = space == Codespace.Managed
                ? MemberCodeComponentFlags.ManagedPart
                : MemberCodeComponentFlags.NativeClassDeclaration;
            IsFlags = isFlags;
        }

        protected override void WriteManagedPart(CodeWriter writer, List<MemberWriter> members)
        {
            base.WriteManagedPart(writer, members);
            
            WriteComments(writer, Codespace.Managed);
            WriteAnnotations(writer, Codespace.Managed, "UEnum");

            WriteManagedVisibilityAndAttributes(writer);
            writer.WriteLine($"enum {Member.Name}");
            using (writer.OpenBlock())
            {
                foreach (var member in members)
                    member.Write(writer, MemberCodeComponent.ManagedPart);
            }
        }
    }
}