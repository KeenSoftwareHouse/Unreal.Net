// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public partial class ClassWriter : TypeWriter
    {
        public ClassWriter(TypeDefinition type, IEnumerable<MemberWriter>? members = null)
            : base(type, members ?? Enumerable.Empty<MemberWriter>())
        { }

        protected override void WriteNativeClassDeclaration(CodeWriter writer, List<MemberWriter> members)
        {
            base.WriteNativeClassDeclaration(writer, members);

            WriteComments(writer, Codespace.Native);
            WriteAnnotations(writer, Codespace.Native, "UClass");
            writer.Write($"class {Module.ModuleApi} {Member.Name}");

            // Write parent and interfaces.
            WriteSupertypes(writer, Codespace.Native);

            // Class body.
            using (writer.OpenBlock(";"))
            {
                writer.WriteLine("GENERATED_BODY()");

                var currentVisibility = SymbolVisibility.Private;

                for (var i = 0; i < members.Count; i++)
                {
                    var member = members[i];
                    if (i < members.Count - 1)
                        writer.WriteLine();

                    var memberVisibility = GetNativeVisibility(member);
                    if (currentVisibility != memberVisibility)
                    {
                        currentVisibility = memberVisibility;

                        using (writer.UnIndent())
                            writer.WriteLine($"{memberVisibility.Format()}:");
                    }

                    member.Write(writer, MemberCodeComponent.NativeClassDeclaration);
                }
            }
        }

        protected override void WriteNativeFunctionDeclaration(CodeWriter writer, List<MemberWriter> members)
        {
            writer.WriteLine("#pragma once\n");

            // Write type forwarding.
            writer.WriteLine($"class {Member.NativeName};\n");

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                    writer.WriteLine();

                var member = members[i];
                member.Write(writer, MemberCodeComponent.NativeFunctionDeclaration);
            }
        }

        protected override void WriteManagedPart(CodeWriter writer, List<MemberWriter> members)
        {
            writer.WriteLine("#nullable disable\n");
            
            base.WriteManagedPart(writer, members);

            WriteComments(writer, Codespace.Managed);
            WriteAnnotations(writer, Codespace.Managed, "UClass");

            WriteManagedVisibilityAndAttributes(writer);

            writer.Write($"partial class {Member.Name}");

            // Write parent and interfaces.
            WriteSupertypes(writer, Codespace.Managed);

            // Class body.
            using (writer.OpenBlock())
            {
                for (var i = 0; i < members.Count; i++)
                {
                    if (i > 0)
                        writer.WriteLine();
                    members[i].Write(writer, MemberCodeComponent.ManagedPart);
                }
            }
        }

        private static SymbolVisibility GetNativeVisibility(MemberWriter member)
            => member.MemberDefinition.Visibility.GetNative();

        public override IEnumerable<ITypeInfo> GetTypeDependencies(Codespace space)
        {
            // Yield parent deps
            foreach (var dep in base.GetTypeDependencies(space))
                yield return dep;
            
            if (Member.ParentType != null)
                yield return Member.ParentType;

            foreach (var iface in GetInterfaces(space))
                yield return iface;
        }
    }
}