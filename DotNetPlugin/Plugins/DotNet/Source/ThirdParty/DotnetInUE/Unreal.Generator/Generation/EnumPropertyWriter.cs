// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class EnumPropertyWriter : MemberWriter<PropertyDefinition>
    {
        public EnumPropertyWriter(PropertyDefinition property, Codespace destination)
            : base(property, DefinitionToDeclaration(destination))
        { }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            WriteComments(writer, component.GetSpace());

            WriteAnnotations(writer, component.GetSpace());

            writer.WriteLine($"{Member.Name} = {Member.Initializer!},");
        }

        private static MemberCodeComponentFlags DefinitionToDeclaration(Codespace destination)
        {
            return destination == Codespace.Managed
                ? MemberCodeComponentFlags.ManagedPart
                : MemberCodeComponentFlags.NativeClassDeclaration;
        }
    }
}