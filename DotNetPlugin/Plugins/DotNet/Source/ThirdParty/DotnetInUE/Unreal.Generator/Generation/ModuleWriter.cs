// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Unreal.Marshalling;
using Unreal.Metadata;
using Unreal.Util;

namespace Unreal.Generation
{
    public class ModuleWriter : AbstractWriter
    {
        /// <summary>
        /// Module that is written.
        /// </summary>
        public override Module Module { get; }

        /// <summary>
        /// Types defined in the module who demand registration.
        /// </summary>
        public readonly List<(TypeDefinition Type, bool HasAPI)> TypesForRegistration = new();

        /// <summary>
        /// Custom type mappings declared in this module.
        /// </summary>
        public readonly List<INamedTypeSymbol> DeclaredTypeMappings = new();

        /// <summary>
        /// Writers for types defined in the module.
        /// </summary>
        public readonly List<TypeWriter> DefinedTypes = new();

        /// <summary>
        /// Native modules whose types are contained in the managed module.
        /// </summary>
        public readonly HashSet<string> NativeModules = new();

        /// <summary>
        /// Native dependencies of the module.
        /// </summary>
        public readonly HashSet<string> ModuleDependencies = new();

        public ModuleWriter(Module module)
        {
            Module = module;
            Components = MemberCodeComponentFlags.ManagedPart
                         | MemberCodeComponentFlags.NativeClassDeclaration
                         | MemberCodeComponentFlags.NativeImplementation;

            NativeModules.Add(module.ModuleId);
        }

        public override string Name => Module.ModuleId;

        public void PostProcess()
        {
            // Sort registered types, for cosmetic reasons :)
            TypesForRegistration.Sort((x, y) =>
            {
                var package = StringComparer.Ordinal.Compare(x.Type.NativeModule, y.Type.NativeModule);
                if (package != 0)
                    return package;

                return StringComparer.Ordinal.Compare(x.Type.NativeName, y.Type.NativeName);
            });

            // Collect native dependencies.
            ModuleDependencies.UnionWith(
                // Select all types and their dependencies.
                DefinedTypes.SelectMany(x =>
                        x.GetTypeDependencies(Codespace.Native).Append(x.Member))
                    .Select(x => x.NativeModule).Where(x => x != ""));

            // Remove self
            ModuleDependencies.Remove(Module.ModuleId);
        }

        public override void Write(CodeWriter writer, MemberCodeComponent component)
        {
            WriteFileHeader(writer, component);

            switch (component)
            {
                case MemberCodeComponent.NativeClassDeclaration:
                {
                    WriteNativeClassDeclaration(writer);
                    break;
                }
                case MemberCodeComponent.NativeImplementation:
                    WriteNativeClassImplementation(writer);
                    break;
                case MemberCodeComponent.ManagedPart:
                    WriteManagedPart(writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(component), component, null);
            }
            
            WriteFileFooter(writer, component);
        }

        private void WriteNativeClassDeclaration(CodeWriter writer)
        {
            var registration = new { Ticket = Module.Ticket };

            writer.WriteLine(TemplateWriter.WriteTemplate(NativeModuleHeaderTemplate, Module, registration));
        }

        //language=C++
        public const string NativeModuleHeaderTemplate =
            @"
#pragma once

#include ""CoreMinimal.h""

#define {NameUpperCamelCase}_GENERATION_TICKET {Ticket}

// Only define the export if the build is JIT.
#if defined(BUILD_JIT)
    #if defined(_MSC_VER)
        //  Microsoft 
        #define {ModuleExport} __declspec(dllexport)
    #elif defined(__GNUC__)
        //  GCC
        #define {ModuleExport} __attribute__((visibility(""default"")))
    #else
    //  do nothing and hope for the best?
    #define {ModuleExport}
    #endif
#else
    // No exports for managed thunks, manually queried.
    #define {ModuleExport}
#endif

class {ModuleApi} F{ModuleId}Module : public IModuleInterface
{
public:
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
};

";

        private void WriteNativeClassImplementation(CodeWriter writer)
        {
            writer.WriteLine($@"#include ""{Module.ModuleHeader}""

#include ""DotNet.h""
#include <CoreUObject.h>
");

            WriteIncludes(writer, new List<MemberWriter>());

            var registrations = TypesForRegistration.Select(
                (x, i) => x.HasAPI
                    ? $"{x.Type.NativeName}::StaticClass()"
                    : $"GetUClass(TEXT(\"/Script/{x.Type.NativeModule}\"), TEXT(\"{x.Type.CosmeticName}\"))");

            var registration = new
            {
                Ticket = Module.Ticket,
                ClassCount = TypesForRegistration.Count,
                Registration = string.Join(",\n        ", registrations)
            };

            writer.WriteLine(TemplateWriter.WriteTemplate(NativeModuleSourceTemplate, Module, registration));
        }

        //language=C++
        public const string NativeModuleSourceTemplate =
            @"
#define INIT_PARAMETERS uint64 ticket, QueryEntryPointCallback EntryPointGetter, UField* Classes[]

typedef void* (*QueryEntryPointCallback)(const UCS2CHAR* EntryPoint);
typedef void (*RuntimeInit)(INIT_PARAMETERS);
	
static void * ModuleHandle;

// Init the unreal DotNet runtime.
extern ""C"" void {NameUpperCamelCase}__Init(INIT_PARAMETERS);

static UField* GetUClass(const TCHAR * PackageName, const TCHAR* ClassName)
{
    const auto Package = FindObject<UPackage>(ANY_PACKAGE, PackageName);

    if (!Package)
        return nullptr;

    return FindObject<UField>(Package, ClassName, false);
}

// Entry point query from managed code.
static void* GetEntryPoint(const UCS2CHAR* EntryPoint)
{
	if (ModuleHandle == nullptr)
		return nullptr;
	return FPlatformProcess::GetDllExport(ModuleHandle, StringCast<TCHAR>(EntryPoint).Get());
}

void F{ModuleId}Module::StartupModule()
{
    RuntimeInit Initializer; 

#if BUILD_JIT
    Initializer = (RuntimeInit)FDotNetModule::Get()->GetManagedEntryPoint(""{Name}"", ""ModuleHelper"", ""Init"");
#else
    Initializer = {NameUpperCamelCase}__Init;
#endif

    auto moduleDllPath = FModuleManager::Get().GetModuleFilename(""{ModuleId}"");

    ModuleHandle = FPlatformProcess::GetDllHandle(*moduleDllPath);
    
    static UField* Classes[] = {
        {Registration}
    }; 
    

    Initializer({NameUpperCamelCase}_GENERATION_TICKET, GetEntryPoint, Classes);
}

void F{ModuleId}Module::ShutdownModule()
{
}

IMPLEMENT_GAME_MODULE(F{ModuleId}Module, {ModuleId})

";

        private void WriteManagedPart(CodeWriter writer)
        {
            var registrations = TypesForRegistration.Select((x, i) =>
            {
                var implementation = x.Type.IsManagedUObject ? "Managed" : "Native";
                var name = x.Type.GetManagedFullName();
                return $"RegisterClass(handles[{i}], typeof({name}), TypeImplementation.{implementation});";
            });

            var nativeModules = string.Join(",", NativeModules.Select(x => $"\"{x}\""));
            var typeMappings = string.Join(", ", DeclaredTypeMappings.Select(x => $"typeof({x.GetFullReferenceName()})"));

            var registration = new
            {
                Ticket = Module.Ticket,
                ClassCount = TypesForRegistration.Count,
                NativeModules = nativeModules,
                TypeMappings = typeMappings,
                Registration = string.Join("\n        ", registrations)
            };

            writer.WriteLine(
                TemplateWriter.WriteTemplate(ManagedModuleTemplate, Module, registration));
        }

        //language=C#
        public const string ManagedModuleTemplate =
            @"
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ComponentModel;

using Unreal;
using Unreal.Core;
using Unreal.CoreUObject;

[module:NativeModules({NativeModules})]
[module:NativeTypeMappings({TypeMappings})]

internal static class ModuleHelper
{
    private static unsafe delegate* unmanaged<char*, void*> m_entryGetter;
    
    private static readonly UField[] m_classes = new UField[{ClassCount}];
    
    private static unsafe IntPtr* m_handles;
    
    internal static ulong Ticket = {Ticket}; 
    
    [UnmanagedCallersOnly(EntryPoint = ""{NameUpperCamelCase}__Init"")]
    private static unsafe void Init(ulong ticket, delegate * unmanaged<char*, void*> entryPointGetter, IntPtr* classHandles)
    {
        // Collect entry point function.
        m_entryGetter = entryPointGetter;

        try
        {
            if(ticket != Ticket)
                throw new Exception($""Native module ticket {ticket} does not match our managed number {Ticket}."");
            
            // Copy handles from native to managed.
            m_handles = classHandles;
            
            // Register module types.
            var classes = new Span<IntPtr>(classHandles, {ClassCount});
            RegisterTypes(classes);
        }
        catch (Exception ex)
        {
            UeLog.Log(LogVerbosity.Fatal, $""Could not register object types: {ex}"");
        }
    }

    private static void RegisterTypes(Span<IntPtr> handles)
    {
        {Registration}
    }

    private static void RegisterClass(IntPtr nativeHandle, Type type, TypeImplementation implementation)
    {
        if(nativeHandle == IntPtr.Zero)
            throw new TypeLoadException($""Could not locate reflection info for type {type}"");
        
        UObjectReflection.Instance.RegisterType(nativeHandle, type, implementation);
    }
    
    internal static unsafe UField GetMetaInstance(int index)
    {
        if (m_classes[index] is not {} uClass)
            m_classes[index] = uClass = UObjectBase.GetOrCreateNative<UField>(m_handles[index])!; // Never null.
        return uClass;
    }
    
    internal static unsafe IntPtr GetNativeMetaInstance(int index)
    {
        return m_handles[index];
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe void* GetFunction(string functionName)
    {
        fixed (char* pinnedName = functionName)
        {
            var entry = m_entryGetter(pinnedName);
            if (entry == null)
                throw new MissingMethodException($""Could not locate entry point for function named '{functionName}'"");
            return entry;
        }
    }
}
";

        public override IEnumerable<ITypeInfo> GetTypeDependencies(Codespace space)
        {
            foreach (var (type, _) in TypesForRegistration)
                yield return type;
        }
    }
}