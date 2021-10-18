// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.ErrorHandling;
using Unreal.Util;

namespace Unreal.Generation
{
    /// <summary>
    /// Base class for a generator in our generation framework.
    /// </summary>
    public abstract class GeneratorBase
    {
        private readonly GenerationCoordinator m_coordinator;

        protected Module Module => m_coordinator.Module;

        protected ModuleWriter ModuleWriter => m_coordinator.ModuleWriter;

        protected GenerationContext Context => m_coordinator.GenerationContext;

        protected ref readonly GeneratorExecutionContext ExecutionContext => ref m_coordinator.ExecutionContext; 

        public GeneratorBase(GenerationCoordinator coordinator)
        {
            m_coordinator = coordinator;
        }

        public virtual void Initialize()
        { }

        /// <summary>
        /// Collect the types that will be generated.
        /// </summary>
        /// <remarks>
        /// This step should only collect and create initial type definitions. Most importantly it should register any
        /// custom definitions for types declared in the module (see <see cref="AddType"/>).
        /// </remarks>
        /// <param name="declaredTypes"></param>
        public abstract void CollectTypes(TypeDeclarationSyntax[] declaredTypes);

        /// <summary>
        /// Do final processing on types and export any new files.
        /// </summary>
        public abstract void ProcessAndExportTypes();

        /// <summary>
        /// Add a type to the generated module.
        /// </summary>
        /// <remarks>Also registers this type with the generation context.</remarks>
        /// <param name="typeWriter"></param>
        protected void AddType(TypeWriter typeWriter)
        {
            Context.RegisterType(typeWriter.Member);
            ModuleWriter.DefinedTypes.Add(typeWriter);
        }
        
        #region Writing
        /// <summary>
        /// Write the components of a type writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="directory">Subdirectory to place the generated files.</param>
        protected void WriteComponents(AbstractWriter writer, string directory = "")
        {
            m_coordinator.WriteComponents(writer, directory);
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Notify of an error in the generation process.
        /// </summary>
        /// <param name="exception"></param>
        /// <exception cref="FatalGenerationException"></exception>
        protected virtual void Error(GenerationException exception)
        {
            if (exception is AggregateGenerationException aggregate)
            {
                foreach (var ex in aggregate.Exceptions)
                    Error(ex);
            }
            else
            {
                m_coordinator.ExecutionContext.ReportDiagnostic(exception.CreateDiagnostic());
            }

            // Rethrow fatal exception and stop generation.
            if (exception.IsFatal)
                throw new FatalGenerationException(exception);
        }

        #endregion

        /// <summary>
        /// Validate the signature of a special method. Currently that's either a custom constructor or destructor.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="syntax"></param>
        /// <exception cref="SourceException"></exception>
        protected void ValidateSpecialSignature(SemanticModel model, MethodDeclarationSyntax syntax)
        {
            var voidType = Context.GetSymbol(typeof(void));

            var returnType = model.GetSymbolInfo(syntax.ReturnType).Symbol;

            if (!returnType.IsEqualTo(voidType))
                throw new SourceException("Method should return void.", syntax);

            if (syntax.TypeParameterList != null)
                throw new SourceException("Method must not be generic.", syntax);

            if (syntax.ParameterList.Parameters.Count > 0)
                throw new SourceException("Method should have no parameters.", syntax);

            if (!syntax.Modifiers.Any(SyntaxKind.PrivateKeyword))
                throw new SourceException("Method should be private.", syntax);

            if (syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
                throw new SourceException("Method should be an instance method.", syntax);
        }
    }
}