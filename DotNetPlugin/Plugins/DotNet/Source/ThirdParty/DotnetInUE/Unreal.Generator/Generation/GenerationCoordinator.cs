// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Unreal.ErrorHandling;
using Unreal.Marshalling;
using Unreal.Metadata;
using Unreal.Util;

namespace Unreal.Generation
{
    public class GenerationCoordinator
    {
        public readonly string ProjectBasePath;

        public readonly string NativeOutputPath;

        public readonly GenerationContext GenerationContext;

        public readonly Module Module;

        public readonly ModuleWriter ModuleWriter;

        private readonly GeneratorBase[] m_generators;

        internal readonly GeneratorExecutionContext ExecutionContext;

        public GenerationCoordinator(GeneratorExecutionContext executionContext)
        {
            ExecutionContext = executionContext;

            m_writerBacking = new StringWriter();
            m_managedWriter = new CodeWriter(m_writerBacking, CodeWriter.ManagedIndent);

            m_generators = new GeneratorBase[]
            {
                new ManagedBindingGenerator(this),
                new NativeBindingGenerator(this)
            };

            // Initialize generators if needed.
            foreach (var generator in m_generators)
                generator.Initialize();

            Module = new Module(executionContext);
            ModuleWriter = new ModuleWriter(Module);
            try
            {
                GenerationContext = new GenerationContext(executionContext);
            }
            catch (Exception e)
            {
                Error(new MetadataException(e.Message));
                throw;
            }

            // Determine if the current process is an IDE or csc process.
            if (Assembly.GetEntryAssembly()?.GetName().Name == "csc")
            {
                ProjectBasePath = executionContext.GetMsBuildProperty("UnrealProjectPath", "")!;
                NativeOutputPath = NormalizePath(executionContext.GetMsBuildProperty("UnrealNativeOutputPath", ""))!;
            }
            else
            {
                // When not running in the actual compiler we don't want to generate any output.
                ProjectBasePath = "";
                NativeOutputPath = "";
            }
        }


        public void Execute()
        {
            // TODO: Make this work together with caching. Ideally record a manifest and delete only files that are not generated.
            if (!string.IsNullOrEmpty(NativeOutputPath))
            {
                EmptyDirectory(Path.Combine(NativeOutputPath, "Public"));
                EmptyDirectory(Path.Combine(NativeOutputPath, "Private"));
            }

            // Collect declared types.
            var types = ExecutionContext.Compilation.SyntaxTrees.SelectMany(GetDeclarations).ToArray();

            DoWithGenerators(x => x.CollectTypes(types));

            // Collect custom native type mappings
            CollectTypeMappings(types);

            DoWithGenerators(x => x.ProcessAndExportTypes());

            ModuleWriter.PostProcess();
            WriteComponents(ModuleWriter);

            var build = new ModuleBuildWriter(ModuleWriter);
            WriteComponents(build);
        }

        private void DoWithGenerators(Action<GeneratorBase> action)
        {
            foreach (var generator in m_generators)
            {
                try
                {
                    action(generator);
                }
                catch (FatalGenerationException)
                {
                    // These are thrown to stop generation, but have already produced their diagnostic, and so are safe to just ignore.
                }
                catch (Exception ex)
                {
                    ExecutionContext.ReportDiagnostic(new UnknownErrorException(ex).CreateDiagnostic());
                }
            }
        }

        private void CollectTypeMappings(TypeDeclarationSyntax[] types)
        {
            var modules = CollectModules();

            // TODO: Collect native modules and the matching managed module where to find them.

            foreach (var module in modules)
                GenerationContext.TypeResolver.RegisterResolversFromMetadataModule(module);

            // Find any type mappings
            var compilation = ExecutionContext.Compilation;
            foreach (var type in types)
            {
                var model = compilation.GetSemanticModel(type.SyntaxTree);
                var attr = type.AttributeLists.GetAttributeOfType(model, GenerationContext.NativeTypeAttribute);

                if (attr == null)
                    continue;

                GenerationContext.TypeResolver.RegisterResolverFromSyntax(GenerationContext, model, type, attr);

                INamedTypeSymbol typeSymbol = (INamedTypeSymbol) model.GetDeclaredSymbol(type)!;
                ModuleWriter.DeclaredTypeMappings.Add(typeSymbol);
            }
        }

        private HashSet<IModuleSymbol> CollectModules()
        {
            var modules = new HashSet<IModuleSymbol>(SymbolEqualityComparer.Default);
            var compilation = ExecutionContext.Compilation;

            foreach (var reference in compilation.ExternalReferences)
            {
                var symbol = ExecutionContext.Compilation.GetAssemblyOrModuleSymbol(reference);
                switch (symbol)
                {
                    case IModuleSymbol module:
                        AddModules(module, modules);

                        break;
                    case IAssemblySymbol assembly:
                        AddModules(assembly, modules);
                        break;
                }
            }

            return modules;
        }

        private void AddModules(IAssemblySymbol reference, HashSet<IModuleSymbol> modules)
        {
            foreach (var module in reference.Modules)
                AddModules(module, modules);
        }

        private void AddModules(IModuleSymbol module, HashSet<IModuleSymbol> modules)
        {
            // Avoid cycles in the dependency graph.
            if (modules.Add(module))
            {
                foreach (var assembly in module.ReferencedAssemblySymbols)
                    AddModules(assembly, modules);
            }
        }

        private static IEnumerable<TypeDeclarationSyntax> GetDeclarations(SyntaxTree tree)
        {
            return tree.GetRoot()
                .DescendantNodesAndSelf(n =>
                    n is CompilationUnitSyntax || n is NamespaceDeclarationSyntax || n is TypeDeclarationSyntax)
                .OfType<TypeDeclarationSyntax>();
        }

        private static void EmptyDirectory(string dir) => EmptyDirectory(new DirectoryInfo(dir));

        private static void EmptyDirectory(DirectoryInfo dir)
        {
            if (!dir.Exists)
                return;

            foreach (var info in dir.EnumerateFileSystemInfos())
            {
                switch (info)
                {
                    case DirectoryInfo dirInfo:
                        EmptyDirectory(dirInfo);
                        break;
                    case FileInfo file:
                        file.Delete();
                        break;
                }
            }
        }

        /// <summary>
        /// Notify of an error in the generation process.
        /// </summary>
        /// <param name="exception"></param>
        /// <exception cref="FatalGenerationException"></exception>
        public void Error(GenerationException exception)
        {
            if (exception is AggregateGenerationException aggregate)
            {
                foreach (var ex in aggregate.Exceptions)
                    Error(ex);
            }
            else
            {
                ExecutionContext.ReportDiagnostic(exception.CreateDiagnostic());
            }

            // Rethrow fatal exception and stop generation.
            if (exception.IsFatal)
                throw new FatalGenerationException(exception);
        }

        private string NormalizePath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            if (Path.IsPathRooted(path))
                return path!;

            var combinedPath = Path.Combine(ProjectBasePath, path!);
            return new FileInfo(combinedPath).FullName;
        }

        #region Writing

        private readonly Regex m_nameCleaner = new(@"[\\/]");

        private void WriteManaged(string source, string name)
        {
            ExecutionContext.AddSource(CleanHintName(name), source);
        }

        private void WriteManaged(SourceText source, string name)
        {
            ExecutionContext.AddSource(CleanHintName(name), source);
        }

        private string CleanHintName(string name)
        {
            return m_nameCleaner.Replace(name, " ");
        }

        /// <summary>
        /// Write the components of a type writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="directory">Subdirectory to place the generated files.</param>
        public void WriteComponents(AbstractWriter writer, string directory = "")
        {
            if (writer.Components.HasComponent(MemberCodeComponent.ManagedPart))
                WriteManaged(writer, directory);

            if (writer.Components.HasComponent(MemberCodeComponent.NativeFunctionDeclaration))
                WriteNative(writer, MemberCodeComponent.NativeFunctionDeclaration, directory);

            if (writer.Components.HasComponent(MemberCodeComponent.NativeClassDeclaration))
                WriteNative(writer, MemberCodeComponent.NativeClassDeclaration, directory);

            if (writer.Components.HasComponent(MemberCodeComponent.NativeImplementation))
                WriteNative(writer, MemberCodeComponent.NativeImplementation, directory, false);

            if (writer.Components.HasComponent(MemberCodeComponent.Custom))
                WritePhysical(writer, MemberCodeComponent.Custom, directory);
        }

        private readonly StringWriter m_writerBacking;
        private readonly CodeWriter m_managedWriter;

        private void WriteManaged(AbstractWriter typeWriter, string directory)
        {
            typeWriter.Write(m_managedWriter, MemberCodeComponent.ManagedPart);
            var result = m_writerBacking.ToString();
            m_managedWriter.Clear();
            m_writerBacking.GetStringBuilder().Clear();

            var path = Path.Combine(directory, typeWriter.GetFilename(MemberCodeComponent.ManagedPart));

            WriteManaged(result, path);
        }

        /// <summary>
        /// Write a native source file.
        /// </summary>
        /// <param name="typeWriter"></param>
        /// <param name="directory">The directory to write to.</param>
        /// <param name="component">The component to write.</param>
        /// <param name="public">Whether the file should be included into the 'Public' or 'Private' source directories. </param>
        private void WriteNative(AbstractWriter typeWriter, MemberCodeComponent component, string directory,
            bool @public = true)
        {
            directory = Path.Combine(@public ? "Public" : "Private", directory);
            WritePhysical(typeWriter, component, directory);
        }

        /// <summary>
        /// Write a physical source file.
        /// </summary>
        /// <param name="typeWriter"></param>
        /// <param name="directory">The directory to write to.</param>
        /// <param name="component">The component to write.</param>
        private void WritePhysical(AbstractWriter typeWriter, MemberCodeComponent component, string directory)
        {
            if (NativeOutputPath == "")
                return;

            var fullPath = Path.Combine(NativeOutputPath, directory, typeWriter.GetFilename(component));

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                using (var writer = new CodeWriter(new StreamWriter(fullPath), CodeWriter.NativeIndent))
                    typeWriter.Write(writer, component);
            }
            catch (GenerationException gen)
            {
                Error(gen);
            }
            catch (Exception ex) when (ex is IOException
                                       || ex is UnauthorizedAccessException
                                       || ex is PathTooLongException
                                       || ex is DirectoryNotFoundException
                                       || ex is NotSupportedException
                                       || ex is SecurityException)
            {
                Error(new IOGenerationException(ex, fullPath, true));
            }
            catch (Exception ex)
            {
                Error(new UnknownErrorException(ex));
            }
        }

        #endregion
    }
}