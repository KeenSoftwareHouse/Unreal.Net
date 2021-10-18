// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "DotNet.h"
#include "Core.h"
#include "Modules/ModuleManager.h"

#include "ClrHost.h"
#include "CoreClrEntryPoints.h"
#include "Interfaces/IPluginManager.h"

//= Types
//==============================================================================

typedef void* (*QueryEntryPointCallback)(const UCS2CHAR* EntryPoint);

typedef void (*RuntimeInit)(QueryEntryPointCallback EntryPointGetter);

//= AoT Entry Points
//==============================================================================

// Main Initialization for the CoreRT Runtime.
extern "C" void CoreRT_StaticInitialization();

// Init the unreal DotNet runtime.
extern "C" void Unreal_Core__Runtime__Init(QueryEntryPointCallback EntryPointGetter);


//= Entry Point Queries
//==============================================================================

static void* OurDllHandle = nullptr;

// Entry point query from managed code.
static void* GetEntryPoint(const UCS2CHAR* EntryPoint)
{
	if (OurDllHandle == nullptr)
		return nullptr;
	return FPlatformProcess::GetDllExport(OurDllHandle, StringCast<TCHAR>(EntryPoint).Get());
}

// Entry point query from managed code.
static void* ManagedEntryPointDummyGetter(char* Assembly, char* Type, char* Function)
{
	return nullptr;
}

//= Entry Point Query
//==============================================================================

template <typename TFunc>
bool TryLoadLibraryFunction(void* LibraryHandle, TCHAR* FunctionName, TFunc& FunctionDestination)
{
	FunctionDestination = static_cast<TFunc>(FPlatformProcess::GetDllExport(LibraryHandle, FunctionName));

	if (FunctionDestination == nullptr)
	{
		UE_LOG(LogClr, Error, TEXT("Core CLR Initialization Failed: Could not load Core Clr DLL"));
		return false;
	}

	return true;
}

//= Module Impl
//==============================================================================

void FDotNetModule::StartupModule()
{
	// TODO: Check what happens in shipped builds, at first I though they were always statically linked.
	auto moduleDllPath = FModuleManager::Get().GetModuleFilename("DotNet");

	OurDllHandle = FPlatformProcess::GetDllHandle(*moduleDllPath);

#if defined(BUILD_JIT)
	auto plugin = IPluginManager::Get().FindPlugin("DotNet");

	auto& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

	// Get the base directory of this plugin
	FString BaseDir = IPluginManager::Get().FindPlugin("DotNet")->GetBaseDir();

	BaseDir = PlatformFile.ConvertToAbsolutePathForExternalAppForRead(*BaseDir);

#if PLATFORM_WINDOWS
	ClrPath = FPaths::Combine(BaseDir, FString("Binaries/ThirdParty/DotNetLibrary/Win64/RuntimeBinaries/")).Replace(
		TEXT("/"), TEXT("\\"));
	BclPath = FPaths::Combine(BaseDir, FString("Binaries/ThirdParty/DotNetLibrary/Win64/Managed")).Replace(
		TEXT("/"), TEXT("\\"));
	AppPath = FPaths::Combine(BaseDir, FString("Binaries/ThirdParty/DotNetLibrary/Win64/")).Replace(
		TEXT("/"), TEXT("\\"));
#else
#error Unsupported platform.
#endif

	// Entry point map, declared before first 'goto'.
	CoreClrEntryPoints* Functions = nullptr;

	const auto Coreclr = FPaths::Combine(ClrPath, FString("coreclr.dll"));
	if (!PlatformFile.FileExists(*Coreclr))
	{
		UE_LOG(LogClr, Error, TEXT("Core CLR Will not be available: Main Library not found."));
		goto fail;
	}

	CoreClrLibraryHandle = FPlatformProcess::GetDllHandle(*Coreclr);

	if (CoreClrLibraryHandle == nullptr)
	{
		UE_LOG(LogClr, Error, TEXT("Core CLR Initialization Failed: Could not load Core Clr DLL"));
		goto fail;
	}

	Functions = new CoreClrEntryPoints();

	if (!TryLoadLibraryFunction(CoreClrLibraryHandle, TEXT("coreclr_initialize"), Functions->Initialize))
		goto fail;
	if (!TryLoadLibraryFunction(CoreClrLibraryHandle, TEXT("coreclr_shutdown_2"), Functions->Shutdown))
		goto fail;
	if (!TryLoadLibraryFunction(CoreClrLibraryHandle, TEXT("coreclr_create_delegate"), Functions->CreateDelegate))
		goto fail;
	if (!TryLoadLibraryFunction(CoreClrLibraryHandle, TEXT("coreclr_execute_assembly"), Functions->ExecuteAssembly))
		goto fail;

	CoreLibraryEntryPoints = Functions;
	LoadedSuccessfully = true;

	HostInstance = new ClrHost("Main Domain");

	EntryGetter = static_cast<EntryPointGetter>(HostInstance->GetDelegate(
		"Unreal.Core", "Unreal.Core.Runtime", "GetFunctionNative"));
	if (EntryGetter == nullptr)
	{
		UE_LOG(LogClr, Fatal, TEXT("Entry point getter could not be loaded, generated bindings will not work."))
		EntryGetter = ManagedEntryPointDummyGetter; // Assign dummy. This removes the need for a null check.
		goto fail;
	}

	const auto RuntimeInitializer = static_cast<RuntimeInit>(HostInstance->GetDelegate(
		"Unreal.Core", "Unreal.Core.Runtime", "Init"));
	if (RuntimeInitializer == nullptr)
	{
		UE_LOG(LogClr, Fatal, TEXT("Could not load runtime initializer."))
		goto fail;
	}

	RuntimeInitializer(GetEntryPoint);

	return;
fail:
	if (CoreClrLibraryHandle != nullptr)
	{
		FPlatformProcess::FreeDllHandle(CoreClrLibraryHandle);
		CoreClrLibraryHandle = nullptr;
	}

	if (Functions != nullptr)
	{
		delete Functions;
		Functions = nullptr;
	}

	if (HostInstance != nullptr)
	{
		delete HostInstance;
		HostInstance = nullptr;
	}
#else
	// Initialize Core RT.
	CoreRT_StaticInitialization();

	Unreal_Core__Runtime__Init(GetEntryPoint)
#endif
}

void FDotNetModule::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module. For modules that support dynamic reloading,
	// we call this function before unloading the module.

	if (HostInstance)
		delete HostInstance;

	// Free the dll handle
	// As it turns out the CORE CLR library does not support library destruction.
	// Since we don't really need this at runtime I'll just leave it commented out.
	//FPlatformProcess::FreeDllHandle(CoreClrLibraryHandle);
	CoreClrLibraryHandle = nullptr;
}

void* FDotNetModule::GetManagedEntryPoint(char* Assembly, char* Type, char* Function) const
{
	const auto Entry = EntryGetter(Assembly, Type, Function);

	if(!Entry)
		UE_LOG(LogClr, Error, TEXT("Could not locate entry point %s"), StringCast<TCHAR>(Function).Get())
	
	return Entry;
}

IMPLEMENT_MODULE(FDotNetModule, DotNet)
DEFINE_LOG_CATEGORY(LogClr);
