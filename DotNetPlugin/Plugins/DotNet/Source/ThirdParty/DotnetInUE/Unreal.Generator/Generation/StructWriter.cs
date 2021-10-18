// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class StructWriter : TypeWriter
    {
        public StructWriter(TypeDefinition type, IEnumerable<MemberWriter>? members = null)
            : base(type, members)
        { }

        protected override void WriteManagedPart(CodeWriter writer, List<MemberWriter> members)
        {
            base.WriteManagedPart(writer, members);
            
            WriteComments(writer, Codespace.Managed);
            WriteAnnotations(writer, Codespace.Managed, "UStruct");

            WriteManagedVisibilityAndAttributes(writer);

            writer.WriteLine($"struct {Member.Name}");
            using (writer.OpenBlock())
            {
                // TODO: Struct inheritance.

                for (var i = 0; i < members.Count; i++)
                {
                    if (i > 0)
                        writer.WriteLine();

                    members[i].Write(writer, MemberCodeComponent.ManagedPart);
                }
            }
        }
    }
}