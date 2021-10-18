// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Linq;
using Unreal.Marshalling;

namespace Unreal.Generation
{
    // TODO: In the future we'll add mechanisms for users to extend the generated module.
    // For now we're just overwriting all files.
    /// <summary>
    /// Writer for the project build file.
    /// </summary>
    public class ModuleBuildWriter : AbstractWriter
    {
        /// <inheritdoc />
        public override string Name => Module.ModuleId;

        /// <inheritdoc />
        public override Module Module => m_moduleWriter.Module;

        /// <inheritdoc />
        protected override string CustomFileName => $"{Name}.Build.cs";

        private readonly ModuleWriter m_moduleWriter;

        /// <inheritdoc />
        public ModuleBuildWriter(ModuleWriter module)
        {
            Components = MemberCodeComponentFlags.Custom;

            m_moduleWriter = module;
        }

        /// <inheritdoc />
        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            if (component != MemberCodeComponent.Custom)
                throw new InvalidOperationException();

            WriteFileHeader(writer, component);

            var moduleDependencies = string.Join(",\n		    ", m_moduleWriter.ModuleDependencies.Select(x => $"\"{x}\""));

            var parameters = new
            {
                ModuleId = Module.ModuleId,
                Dependencies = moduleDependencies
            };

            writer.WriteLine(TemplateWriter.WriteTemplate(Template, parameters));

            WriteFileFooter(writer, component);
        }

        //language=C#
        private const string Template = @"using System.Net.Configuration;
using UnrealBuildTool;

// Using shared base rules for all DotNet Modules.
public class {ModuleId} : DotnetModuleRules
{
	public {ModuleId}(ReadOnlyTargetRules Target)
		: base(Target)
	{
		PublicDependencyModuleNames.AddRange(new[] {
		    {Dependencies}
		});
    }
}";
    }
}