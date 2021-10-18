// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class FunctionWriter : FunctionWriterBase
    {
        public FunctionWriter(FunctionDefinition member, Codespace space)
            : base(member, DefinitionToDeclaration(space))
        { }

        /// <summary>
        /// Inline initialization used in ctors.
        /// </summary>
        public string? InlineInitialization { get; set; }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            if (CustomBody == null)
                throw new InvalidOperationException("Custom body must be set.");

            if (component == MemberCodeComponent.NativeClassDeclaration)
                WriteNativeSignature(writer);
            else if (component == MemberCodeComponent.ManagedPart)
                WriteManagedSignature(writer);
            else
                throw new ArgumentOutOfRangeException(nameof(component), component, null);

            if (InlineInitialization != null)
            {
                using (writer.Indent())
                    writer.WriteLine($": {InlineInitialization}");
            }

            using (writer.OpenBlock())
                writer.WriteLine(CustomBody!);
        }

        private static MemberCodeComponentFlags DefinitionToDeclaration(Codespace destination)
        {
            return destination == Codespace.Managed
                ? MemberCodeComponentFlags.ManagedPart
                : MemberCodeComponentFlags.NativeClassDeclaration;
        }
    }
}