// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unreal.ErrorHandling;
using Unreal.Marshalling;
using Unreal.Metadata;
using Unreal.Util;

namespace Unreal.Generation
{
    public class ManagedBindingGenerator : GeneratorBase
    {
        // These are only used after they are set.

        private ITypeInfo m_managedObjectType = null!;

        private ITypeInfo m_createManagedInstanceFuncType = null!;
        
        private readonly SymbolSortAdapter m_sorter = new ();
        
        private readonly List<(ClassWriter Def, ClassDeclarationSyntax Syntax)> m_classes = new();

        public ManagedBindingGenerator(GenerationCoordinator coordinator)
            : base(coordinator)
        { }

        public override void CollectTypes(TypeDeclarationSyntax[] declaredTypes)
        {
            var module = Context.GetModule("DotNet");

            m_managedObjectType = new NativeOnlyType(module, "IManagedObject", "ManagedObject.h");
            m_createManagedInstanceFuncType = new NativeOnlyType(module, "CreateManagedInstanceFunc",
                typicalArgumentType: NativeTransferType.ByValue);

            // Collect types.
            foreach (var @class in declaredTypes.OfType<ClassDeclarationSyntax>())
            {
                var model = Context.Compilation.GetSemanticModel(@class.SyntaxTree);

                if (@class.AttributeLists.ContainsAttributeType(model, Context.ManagedTypeAttribute))
                    continue;
                
                if (!@class.AttributeLists.ContainsAttributeType(model, Context.UClassAttribute))
                    continue;
                m_sorter.Add(@class, model);
            }

            if (m_sorter.Empty)
                return;

            // Sort by hierarchy.
            m_sorter.Sort();

            // Collect types.
            foreach (var @class in m_sorter.Classes)
            {
                try
                {
                    // Create a writer for the type. 
                    var classWriter = CreateType(@class);
                    m_classes.Add((classWriter, @class));

                    AddType(classWriter);
                }
                catch (GenerationException e)
                {
                    Error(e);
                }
            }
        }

        public override void ProcessAndExportTypes()
        {
            if (m_sorter.Empty)
                return;
            
            // Collect members.
            foreach (var (writer, @class) in m_classes)
            {
                try
                {
                    ProcessClass(writer, @class);
                    ClassWriter.AddStaticClassMembers(Context, ModuleWriter, writer, true);
                }
                catch (GenerationException e)
                {
                    Error(e);
                    continue;
                }

                WriteComponents(writer);
            }
        }

        private ClassWriter CreateType(ClassDeclarationSyntax classSyntax)
        {
            // Check that the class is partial.n
            if (!classSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                throw new SourceException(
                    $"Cannot generate UE4 bindings for {classSyntax.GetFullName()}: Type must be partial.",
                    classSyntax);

            // Check constructors.
            foreach (var ctor in classSyntax.Members.OfType<ConstructorDeclarationSyntax>())
            {
                throw new SourceException(
                    $"UObject type '{classSyntax.GetFullName()}' should not declare any constructors.", ctor);
            }

            var builder = TypeDefinition.PrepareFromManaged(Module, Context, classSyntax)
                .WithIsManagedUObject(true)
                .WithMetaAttribute(Context.GetMetaAttribute(MetaAttribute.ManagedTypeAttributeName));

            return ClassWriter.CreateUObjectWriter(builder.Build(), Codespace.Native);
        }

        private void ProcessClass(ClassWriter writer, ClassDeclarationSyntax classSyntax)
        {
            ErrorCollector collector = default;

            writer.AdditionalNamespaces.Add("System.Runtime.InteropServices");

            var classType = writer.Member;
            var model = Context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

            ManagedFunctionBinder? constructor = null, destructor = null;

            // Add functions.
            foreach (var functionSyntax in classSyntax.Members.OfType<MethodDeclarationSyntax>())
            {
                // Allow UFunctions and functions with a special name.
                if (functionSyntax.AttributeLists.GetAttributeOfType(model, Context.ExportedFunctionAttribute) is
                    not { } attributeSyntax)
                    continue;

                ManagedFunctionBinder function;
                try
                {
                    var functionDef = FunctionDefinition
                        .PrepareFromManaged(Context, writer.Member, functionSyntax, model)
                        .Build();
                    function = new ManagedFunctionBinder(functionDef);
                }
                catch (GenerationException ex)
                {
                    if (ex is MissingSymbolException ms)
                        ms.RequestingType = $"{classType.ManagedName}.{functionSyntax.Identifier.ValueText}";

                    collector.Add(ex);
                    continue;
                }

                var type = ModelExtensions.GetTypeInfo(model, attributeSyntax).Type;

                if (!type.IsEqualTo(Context.UFunctionAttribute))
                {
                    // Not a UFunction, so a special method then.

                    // Validate signature of special method.
                    ValidateSpecialSignature(model, functionSyntax);

                    if (type.IsEqualTo(Context.ConstructorAttribute))
                    {
                        if (constructor != null)
                            throw new SourceException("Type may only have one constructor.", functionSyntax);
                        constructor = function;

                        // Do not create class implementation, these are manually generated.
                        constructor.Components =
                            constructor.Components.Without(MemberCodeComponent.NativeClassDeclaration);
                    }
                    else if (type.IsEqualTo(Context.DestructorAttribute))
                    {
                        if (destructor != null)
                            throw new SourceException("Type may only have one destructor.", functionSyntax);
                        destructor = function;
                        // Do not create class implementation, these are manually generated.
                        destructor.Components =
                            destructor.Components.Without(MemberCodeComponent.NativeClassDeclaration);
                    }
                }

                writer.AddMember(function);
            }

            AddCtorAndDtor(writer, constructor, destructor);

            // Allow generation to continue.
            collector.ThrowIfNeeded();
        }

        private class SymbolSortAdapter : ITopologicalSortAdapter<INamedTypeSymbol>
        {
            private Dictionary<INamedTypeSymbol, ClassDeclarationSyntax>
                m_mapping = new(SymbolEqualityComparer.Default);

            private List<INamedTypeSymbol> m_symbols = new();

            public IEnumerable<ClassDeclarationSyntax> Classes => m_symbols.Select(x => m_mapping[x]);

            public bool Empty => m_symbols.Count == 0;

            public void Add(ClassDeclarationSyntax classDeclaration, SemanticModel model)
            {
                var symbol = model.GetDeclaredSymbol(classDeclaration)!;
                m_mapping.Add(symbol, classDeclaration);
                m_symbols.Add(symbol);
            }

            public void Sort()
            {
                SortUtil.TopologicalSort(m_symbols, this);
            }

            public int GetLinkCount(in INamedTypeSymbol item)
            {
                return item.BaseType != null && m_mapping.ContainsKey(item.BaseType) ? 1 : 0;
            }

            public INamedTypeSymbol GetLink(in INamedTypeSymbol item, int index)
            {
                if (index > 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return item.BaseType!;
            }

            public bool HandleCycle(IEnumerable<INamedTypeSymbol> participants)
            {
                throw new InvalidOperationException("Type hierarchy cycle detected.");
            }
        }

        private void AddCtorAndDtor(ClassWriter writer, ManagedFunctionBinder? constructor,
            ManagedFunctionBinder? destructor)
        {
            var type = writer.Member;

            // Add include for main module header.
            writer.AdditionalHeaders.Add(Module.ModuleHeader);

            // Add native ctors.

            // Parent type should always be set.
            string superCall = $"{type.ParentType!.NativeName}(CreateInstance)";

            // IManagedObject
            if (type.ParentType?.IsManagedUObject != true)
            {
                writer.Interfaces.Add((CodespaceFlags.Native, m_managedObjectType));
                superCall = $"{m_managedObjectType.NativeName}(CreateInstance, this)";
            }

            // Main ctor.
            var mainCtor = FunctionDefinition.CreateBuilder(type, "ctor")
                .WithSpecialMethod(MethodSpecialType.Constructor)
                .WithVisibility(SymbolVisibility.Protected)
                .WithParameter("CreateInstance", m_createManagedInstanceFuncType)
                .Build();

            var mainCtorBody = constructor != null ? $"{constructor.Member.EntryPointName}(this);" : "";

            // Leave body empty for now.
            writer.AddMember(new FunctionWriter(mainCtor, Codespace.Native)
            {
                InlineInitialization = superCall,
                CustomBody = mainCtorBody
            });

            // Create instance method
            var createInstance = FunctionDefinition.CreateBuilder(type, "CreateInstance")
                .WithReturn<IntPtr>()
                .WithVisibility(SymbolVisibility.Private)
                .WithAttributes(SymbolAttributeFlags.Static)
                .WithParameter<IntPtr>("nativeInstance")
                .WithManagedAttribute("EditorBrowsable(EditorBrowsableState.Never)")
                .Build();

            writer.AddMember(new ManagedFunctionBinder(createInstance)
            {
                CustomBody = $"return (IntPtr)GCHandle.Alloc(new {type.Name}(nativeInstance));",
                // Do not declare method in class.
                Components = MemberCodeComponentFlags.All.Without(MemberCodeComponent.NativeClassDeclaration)
            });

            // Constructor used for new instances of the current type.
            var localCtor = FunctionDefinition.CreateBuilder(type, "ctor")
                .WithSpecialMethod(MethodSpecialType.Constructor)
                .WithVisibility(SymbolVisibility.Public)
                .Build();

            writer.AddMember(new FunctionWriter(localCtor, Codespace.Native)
            {
                InlineInitialization = $"{type.Name}({createInstance.EntryPointName})",
                CustomBody = ""
            });

            // Add dtor calling destruct method if any.
            if (destructor != null)
            {
                var localDtor = FunctionDefinition.CreateBuilder(type, "dtor")
                    .WithSpecialMethod(MethodSpecialType.Destructor)
                    .WithVisibility(SymbolVisibility.Public)
                    .Build();

                writer.AddMember(new FunctionWriter(localDtor, Codespace.Native)
                {
                    CustomBody = $"{destructor.Member.EntryPointName}();"
                });
            }
        }
    }
}