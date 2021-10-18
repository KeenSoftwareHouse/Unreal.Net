// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "CoreMinimal.h"

#include "GenerationSettings.h"
#include "Programs/UnrealHeaderTool/Public/IScriptGeneratorPluginInterface.h"

class FDotNetModuleBase : public IScriptGeneratorPluginInterface
{
protected:

	// Generation settings.
	FSettings Settings;

	// Project dir.
	FString ProjectDir;

public:
	virtual ~FDotNetModuleBase() = default;

	/** IModuleInterface implementation */
	virtual void StartupModule() override;

	virtual void ShutdownModule() override;
	virtual void PreUnloadCallback() override;
	virtual void PostLoadCallback() override;

	virtual bool SupportsDynamicReloading() override;
	virtual bool SupportsAutomaticShutdown() override;
	virtual bool IsGameModule() const override;
	virtual FString GetGeneratedCodeModuleName() const override;
};
