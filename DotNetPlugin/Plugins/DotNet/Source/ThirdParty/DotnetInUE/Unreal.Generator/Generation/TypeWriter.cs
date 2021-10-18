// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public abstract class TypeWriter : MemberWriter<TypeDefinition>
    {
        private readonly List<MemberWriter> m_memberList;

        protected readonly Dictionary<MemberCodeComponent, List<MemberWriter>> MembersByComponent;

        private static readonly List<MemberWriter> EmptyList = new();

        public List<(CodespaceFlags Spaces, ITypeInfo Type)> Interfaces { get; } = new();

        public bool HasGeneratedHeader = false;

        public IReadOnlyList<MemberWriter> Members => m_memberList;

        public TypeWriter(TypeDefinition type, IEnumerable<MemberWriter>? members)
            : base(type)
        {
            MembersByComponent = new Dictionary<MemberCodeComponent, List<MemberWriter>>();
            if (members is ICollection<MemberWriter> col)
                m_memberList = new List<MemberWriter>(col.Count);
            else
                m_memberList = new List<MemberWriter>();

            if (members != null)
            {
                foreach (var memberWriter in members)
                    AddMember(memberWriter);
            }
        }

        public void AddMember(MemberWriter member)
        {
            InsertMember(m_memberList.Count, member);
        }

        public void InsertMember(int index, MemberWriter writer)
        {
            m_memberList.Insert(index, writer);

            foreach (var component in writer.Components)
            {
                if (!MembersByComponent.TryGetValue(component, out var list))
                    MembersByComponent[component] = list = new List<MemberWriter>();
                list.Add(writer);
            }

            Components |= writer.Components;
        }

        protected ITypeInfo[] GetInterfaces(Codespace forSpace)
        {
            return Interfaces.Where(x => x.Spaces.HasSpace(forSpace)).Select(x => x.Type).ToArray();
        }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            WriteFileHeader(writer, component);

            if (!MembersByComponent.TryGetValue(component, out var members))
                members = EmptyList;

            // Get token for the current trailing state, allows base impl of managed part to open namespace.
            using (writer.GetCurrentToken())
            {
                switch (component)
                {
                    case MemberCodeComponent.NativeFunctionDeclaration:
                        WriteNativeFunctionDeclaration(writer, members);
                        break;
                    case MemberCodeComponent.NativeClassDeclaration:
                        WriteNativeClassDeclaration(writer, members);
                        break;
                    case MemberCodeComponent.NativeImplementation:
                        WriteNativeImplementation(writer, members);
                        break;
                    case MemberCodeComponent.ManagedPart:
                        WriteManagedPart(writer, members);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(component), component, null);
                }
            }

            WriteFileFooter(writer, component);
        }

        private void WriteIncludes(CodeWriter writer, List<MemberWriter> writers)
        {
            base.WriteIncludes(writer, writers, Member);
        }

        private void WriteNamespaces(CodeWriter writer, List<MemberWriter> writers)
        {
            base.WriteNamespaces(writer, writers, Member);
        }

        protected virtual void WriteNativeFunctionDeclaration(CodeWriter writer, List<MemberWriter> members)
        {
            writer.WriteLine("#pragma once\n");

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                    writer.WriteLine();

                var member = members[i];
                member.Write(writer, MemberCodeComponent.NativeFunctionDeclaration);
            }
        }

        protected virtual void WriteNativeClassDeclaration(CodeWriter writer, List<MemberWriter> members)
        {
            writer.WriteLine("#pragma once\n");

            WriteIncludes(writer, members);

            if (Components.HasComponent(MemberCodeComponent.NativeFunctionDeclaration))
                writer.WriteLine($"\n#include \"{Member.Name}.functions.h\"");

            if (HasGeneratedHeader)
                writer.WriteLine($"\n#include \"{Member.Name}.generated.h\"");
        }

        protected virtual void WriteNativeImplementation(CodeWriter writer, List<MemberWriter> members)
        {
            writer.WriteLine($"#include \"{Member.Header}\"\n");

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                    writer.WriteLine();

                var member = members[i];
                member.Write(writer, MemberCodeComponent.NativeImplementation);
            }
        }

        protected virtual void WriteManagedPart(CodeWriter writer, List<MemberWriter> members)
        {
            WriteNamespaces(writer, members);

            writer.WriteLine($"namespace {Member.Namespace}");

            // Leave undisposed. caller will be clearing that for us.
            writer.OpenBlock();
        }

        protected void WriteSupertypes(CodeWriter writer, Codespace space)
        {
            var ifaces = GetInterfaces(space);
            var hasInterfaces = ifaces.Length > 0;

            var hasParent = Member.ParentType != null;
            if (hasParent || hasInterfaces)
            {
                writer.Write(": ");

                if (hasParent)
                {
                    if (space == Codespace.Native)
                        writer.Write($"public {Member.ParentType!.NativeName}");
                    else
                        writer.Write(Member.ParentType!.ManagedName);
                }

                if (@hasParent && hasInterfaces)
                    writer.Write(", ");

                if (hasInterfaces)
                {
                    if (space == Codespace.Native)
                        writer.Write(string.Join(", ", ifaces.Select(x => "public " + x.NativeName)));
                    else
                        writer.Write(string.Join(", ", ifaces.Select(x => x.ManagedName)));
                }
            }

            writer.WriteLine();
        }

        /// <inheritdoc />
        public override IEnumerable<ITypeInfo> GetTypeDependencies(Codespace space)
        {
            return Members.SelectMany(x => x.GetTypeDependencies(space));
        }
    }
}