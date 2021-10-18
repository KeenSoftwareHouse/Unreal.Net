// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public static class GenerationContextExtensions
    {
        #region Module

        public static Module GetModule(this GenerationContext self, System.Reflection.Module systemModule)
        {
            var module = Module.GetModule(systemModule);

            self.EnsureCached(module.Name, module);

            return module;
        }

        public static Module GetModule(this GenerationContext self, IModuleSymbol moduleSymbol)
        {
            return self.GetModule(moduleSymbol.Name);
        }

        public static Module GetModule(this GenerationContext self, string moduleName)
        {
            return self.GetCached(moduleName, name => new Module(name));
        }

        #endregion
    }
}