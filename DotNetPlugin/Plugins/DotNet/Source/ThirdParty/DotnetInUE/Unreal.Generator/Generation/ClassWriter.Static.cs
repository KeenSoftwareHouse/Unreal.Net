// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Unreal.Marshalling;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public partial class ClassWriter
    {
        public const string FakeConstructorName = "FakeCtor";
        public const string RealConstructorName = "RealCtor";
        
        public static ClassWriter CreateUObjectWriter(TypeDefinition type, Codespace targetCodespace)
        {
            var writer = new ClassWriter(type)
            {
                HasGeneratedHeader = true,
                AdditionalHeaders = {"ManagedObject.h", "DotNet.h"},
                AdditionalNamespaces = {"Unreal", "System.ComponentModel", "Unreal.Core"},

                // Ensure the main component generates even if no members are defined.
                Components = targetCodespace == Codespace.Managed
                    ? MemberCodeComponentFlags.ManagedPart
                    : MemberCodeComponentFlags.NativeClassDeclaration,
                MetaSpace = targetCodespace.ToFlags()
            };

            // Add unreal generated header.

            // Since we want to always generate the ctor here we'll have errors in the ide because the parent's ctor is not called.
            // To quiet that down we add this 'fake' ctor.
            var fakeCtor = FunctionDefinition.CreateBuilder(type, FakeConstructorName)
                .WithSpecialMethod(MethodSpecialType.Constructor)
                .WithVisibility(SymbolVisibility.Protected)
                .WithManagedAttribute("EditorBrowsable(EditorBrowsableState.Never)")
                .Build();

            writer.AddMember(new FunctionWriter(fakeCtor, Codespace.Managed)
            {
                CustomBody = "throw new InvalidOperationException(\"The parameterless .ctor shall not be used.\");"
            });

            // Actual ctor that call s the parent chain correctly.
            var realCtor = FunctionDefinition.CreateBuilder(type, RealConstructorName)
                .WithSpecialMethod(MethodSpecialType.Constructor)
                .WithParameter<IntPtr>("nativeInstance")
                .WithVisibility(SymbolVisibility.Protected)
                .Build();

            writer.AddMember(new FunctionWriter(realCtor, Codespace.Managed)
            {
                InlineInitialization = "base(nativeInstance)",
                CustomBody = ""
            });

            return writer;
        }

        public static void AddStaticClassMembers(GenerationContext context, ModuleWriter module, ClassWriter writer,
            bool hasAPI)
        {
            // Note: To avoid running all static .cctors at the start of the module we are caching the type info in the module itself
            // Classes just store a reference to that.

            var type = writer.Member;
            // TODO: This should always work, later when it does we'll remove the line bellow. 
            if (!context.TryGetNativeTypeInfo(new QualifiedNativeTypeName("CoreUObject", "UClass"), out var uClassTypeInfo))
                uClassTypeInfo = context.GetMetadataTypeInfo("Unreal.CoreUObject.UClass");

            var index = module.TypesForRegistration.Count;
            module.TypesForRegistration.Add((type, hasAPI));

            // Registration function.
            var staticClass = FunctionDefinition.CreateBuilder(type, "StaticClass")
                .WithReturn(uClassTypeInfo!)
                .WithDocumentation(new Documentation {Summary = "Get the metadata class for this type."})
                .WithVisibility(SymbolVisibility.Public)
                .WithAttribute(SymbolAttribute.Static);

            // If parent type is not the base object type it will have a similar definition. 
            var isUObjectSubtype = type.GetManagedFullName() != "Unreal.CoreUObject.UObject";
            if (isUObjectSubtype)
                staticClass.WithAttribute(SymbolAttribute.New);

            writer.AddMember(new FunctionWriter(staticClass.Build(), Codespace.Managed)
            {
                CustomBody = $"return (UClass)ModuleHelper.GetMetaInstance({index});"
            });

            // Add virtual GetClass() method.

            var getClass = FunctionDefinition.CreateBuilder(type, "GetClass")
                .WithReturn(uClassTypeInfo!)
                .WithDocumentation(new Documentation {Summary = "Returns the class instance for this object."})
                .WithVisibility(SymbolVisibility.Public)
                .WithAttribute(SymbolAttribute.Virtual);

            if (isUObjectSubtype)
                getClass.WithAttribute(SymbolAttribute.Override);
            writer.AddMember(new FunctionWriter(getClass.Build(), Codespace.Managed)
            {
                // Class body must first check if the current class is the same as the native one.
                // If they differ return the native uclass instead.
                CustomBody = $@"var nativeClass = UObjectUtil.GetUClass(this);
if (nativeClass == ModuleHelper.GetNativeMetaInstance({index}))
    return StaticClass();
else
    return GetOrCreateNative<UClass>(nativeClass);"
            });
        }
    }
}