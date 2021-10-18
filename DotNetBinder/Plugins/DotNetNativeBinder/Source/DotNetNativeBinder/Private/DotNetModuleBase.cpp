// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "DotNetModuleBase.h"

#include <Features/IModularFeatures.h>

#include "DebuggerHelper.h"
#include "Misc/FileHelper.h"

void FDotNetModuleBase::StartupModule()
{
	//Debugger_Launch();
	
	IModularFeatures::Get().RegisterModularFeature(TEXT("ScriptGenerator"), this);

	const auto MainModule = FPaths::GetBaseFilename(FPaths::GetProjectFilePath());
	ProjectDir = FPaths::GetPath(FPaths::GetProjectFilePath());

	auto Config = FPaths::Combine(ProjectDir, TEXT("Config"));

	const auto ConfigFile = FPaths::Combine(Config, TEXT("DotNetGenerator.ini"));

	auto& PlatformFile = IPlatformFile::GetPlatformPhysical();

	if (!PlatformFile.FileExists(*ConfigFile))
	{
		auto ConfigContents = FString::Format(
			TEXT("[Settings]\nMainModule={0}\nOutputPath=Intermediate/DotNet/Metadata"), {MainModule});
		FFileHelper::SaveStringToFile(ConfigContents, *ConfigFile, FFileHelper::EEncodingOptions::ForceUTF8);
	}

	Settings = FSettings::Load(*ConfigFile);
}

void FDotNetModuleBase::ShutdownModule()
{
	IModularFeatures::Get().UnregisterModularFeature(TEXT("ScriptGenerator"), this);
}

void FDotNetModuleBase::PreUnloadCallback()
{
}

void FDotNetModuleBase::PostLoadCallback()
{
}

bool FDotNetModuleBase::SupportsDynamicReloading()
{
	return false;
}

bool FDotNetModuleBase::SupportsAutomaticShutdown()
{
	return true;
}

bool FDotNetModuleBase::IsGameModule() const
{
	return false;
}

FString FDotNetModuleBase::GetGeneratedCodeModuleName() const
{
	return "Engine"; // Will always produce some output.
}
