// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics;
using Unreal.Generation;

namespace Unreal.Metadata
{
    [DebuggerDisplay("{Name}")]
    public partial class FunctionDefinition : MemberDefinition
    {
        public const string ThisParameterName = "__self";

        public const string ReturnParameterName = "__return";

        public TransferableDefinition Return { get; }
        
        public ImmutableArray<ParameterDefinition> Parameters { get; }

        public string EntryPointName { get; }
        
        /// <summary>
        /// Custom native name for this method.
        /// </summary>
        public MethodSpecialType SpecialMethod { get; }

        /// <summary>
        /// Type that contains this member.
        /// </summary>
        /// <remarks>For functions this cannot be null.</remarks>
        public new TypeDefinition EnclosingType => base.EnclosingType!;

        public FunctionDefinition(string name, Module module,
            ImmutableList<MetaAttribute>? metaAttributes, Documentation? documentation,
            string comments, TypeDefinition? enclosingType, SymbolVisibility visibility,
            SymbolAttributeFlags attributes, ImmutableArray<string> managedAttributes, string entryPointName, TransferableDefinition @return,
            ImmutableArray<ParameterDefinition> parameters, MethodSpecialType specialMethod)
            : base(name, module, metaAttributes, documentation, comments, enclosingType, visibility, attributes, managedAttributes)
        {
            EntryPointName = entryPointName;
            Return = @return;
            Parameters = parameters;
            SpecialMethod = specialMethod;
        }

        public static FunctionDefinitionBuilder CreateBuilder(TypeDefinition declaringType, string name)
        {
            return new(declaringType.Module, name, declaringType);
        }
    }
}