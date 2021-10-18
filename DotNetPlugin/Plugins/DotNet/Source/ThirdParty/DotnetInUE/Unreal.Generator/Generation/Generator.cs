// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Unreal.Generation
{
    /// <summary>
    /// Main Generator
    /// </summary>
    // Visual studio will create one instance of the generator and call execute multiple times. 
    // To us this is a nuisance, there is essentially no data that can be used between calls,
    // but we had been using the GenerationCoordinator to store values used by the nested generators.
    //
    // At this point it makes more sense to just create another indirection level and instantiate
    // a coordinator for each execute method.
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        { }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            var coordinator = new GenerationCoordinator(context);
            coordinator.Execute();
        }
    }
}