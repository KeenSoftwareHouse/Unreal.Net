// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "DotNetNativeBinder.h"

#include "GenericPlatform/GenericPlatformFile.h"
#include "UObject/UnrealType.h"

#define LOCTEXT_NAMESPACE "FDotNetNativeBinderModule"

void FDotNetNativeBinderModule::Initialize(const FString& RootLocalPath, const FString& RootBuildPath,
                                           const FString& OutputDirectory, const FString& IncludeBase)
{
	auto& PlatformFile = IPlatformFile::GetPlatformPhysical();

	OutputPath = FPaths::Combine(ProjectDir, Settings.OutputPath);

	PlatformFile.CreateDirectoryTree(*OutputPath);
	Modules = PlatformFile.OpenWrite(*FPaths::Combine(OutputPath,TEXT("Modules.txt")));
}

static bool IsGame(EBuildModuleType::Type ModuleType)
{
	switch (ModuleType)
	{
	default: ;
	case EBuildModuleType::Max:

	case EBuildModuleType::Program:

	case EBuildModuleType::EngineRuntime:
	case EBuildModuleType::EngineUncooked:
	case EBuildModuleType::EngineDeveloper:
	case EBuildModuleType::EngineEditor:
	case EBuildModuleType::EngineThirdParty:
		return false;

	case EBuildModuleType::GameRuntime:
	case EBuildModuleType::GameUncooked:
	case EBuildModuleType::GameDeveloper:
	case EBuildModuleType::GameEditor:
	case EBuildModuleType::GameThirdParty:
		return true;
	}
}

bool FDotNetNativeBinderModule::SupportsTarget(const FString& TargetName) const
{
	return true;
}

bool FDotNetNativeBinderModule::ShouldExportClassesForModule(const FString& ModuleName,
                                                             EBuildModuleType::Type ModuleType,
                                                             const FString& ModuleGeneratedIncludeDirectory) const
{
	if (!IsGame(ModuleType) && ModuleName != "Engine" && ModuleName != "CoreUObject")
		return false;
	
	Collector.SetPackageType(ModuleName, ModuleType);
	return true;
}


void FDotNetNativeBinderModule::ExportClass(UClass* Class, const FString& SourceHeaderFilename,
                                            const FString& GeneratedHeaderFilename, bool bHasChanged)
{
	Collector.TouchClass(Class);
}

void FDotNetNativeBinderModule::FinishExport()
{
	Collector.ExportAllTypes(OutputPath);
}

FString FDotNetNativeBinderModule::GetGeneratorName() const
{
	return "DotNetBindingGenerator";
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(FDotNetNativeBinderModule, DotNetNativeBinder)

DEFINE_LOG_CATEGORY(LogDotNetGenerator);
