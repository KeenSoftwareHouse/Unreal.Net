// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include <CoreMinimal.h>

#include "DotNetModuleBase.h"
#include "TypeInformation.h"

DECLARE_LOG_CATEGORY_EXTERN(LogDotNetGenerator, Log, All);

class FDotNetNativeBinderModule : public FDotNetModuleBase
{
	IFileHandle * Modules = nullptr;

	TSet<UClass*> Visited;

	TArray<FString> ModulesToGenerate;

	FString OutputPath;

	mutable FTypeCollector Collector;

public:
	virtual ~FDotNetNativeBinderModule() = default;

	virtual void Initialize(const FString& RootLocalPath, const FString& RootBuildPath, const FString& OutputDirectory,
	                        const FString& IncludeBase) override;
	virtual bool SupportsTarget(const FString& TargetName) const override;
	virtual bool ShouldExportClassesForModule(const FString& ModuleName, EBuildModuleType::Type ModuleType,
	                                          const FString& ModuleGeneratedIncludeDirectory) const override;
	virtual void ExportClass(UClass* Class, const FString& SourceHeaderFilename, const FString& GeneratedHeaderFilename,
	                         bool bHasChanged) override;
	virtual void FinishExport() override;
	virtual FString GetGeneratorName() const override;
};