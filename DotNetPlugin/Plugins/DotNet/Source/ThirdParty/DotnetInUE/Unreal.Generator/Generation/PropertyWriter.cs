// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class PropertyWriter : MemberWriter<PropertyDefinition>
    {
        public PropertyWriter(PropertyDefinition property, Codespace destination)
            : base(property, DefinitionToDeclaration(destination))
        { }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            WriteComments(writer, component.GetSpace());

            WriteAnnotations(writer, component.GetSpace(), "UProperty");

            WriteManagedVisibilityAndAttributes(writer);

            if (component.GetSpace() == Codespace.Native)
                writer.Write(Member.Type.FormatNative());
            else
                writer.Write(Member.Type.FormatManaged());

            writer.Write(" ");
            writer.Write(Member.Name);

            if (Member.Initializer != null)
                writer.Write($"= {Member.Initializer!}");

            writer.WriteLine(";");
        }

        private static MemberCodeComponentFlags DefinitionToDeclaration(Codespace destination)
        {
            return destination == Codespace.Managed
                ? MemberCodeComponentFlags.ManagedPart
                : MemberCodeComponentFlags.NativeClassDeclaration;
        }

        public override IEnumerable<ITypeInfo> GetTypeDependencies(Codespace space)
        {
            return Member.Type.TypeInfo.GetTypeDependencies(true);
        }
    }
}