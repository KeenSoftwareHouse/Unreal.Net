// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include <CoreMinimal.h>

#include "Internationalization/Regex.h"

// TODO: Finish later in appropriate task.

enum class EPatternType : uint8
{
	Include,
	Exclude
};

struct FNamePattern
{
	FRegexPattern Pattern;
	EPatternType Type;

	static inline bool IsMatch(TArray<FNamePattern> Patterns, FString Name)
	{
		// TODO: Implement matching correctly.
		for (int i = Patterns.Num() - 1; i >= 0; --i)
		{
			bool IsMatch = FRegexMatcher(Patterns[i].Pattern, Name).FindNext();
		}
		return false;
	}
};

/**
* Definition for the generation of a specific module.
*/
struct FModuleGeneration
{
	FString Name;

	// Patterns to match the types from the module that should be included. 
	TArray<FNamePattern> Types;
};

struct FModuleGenerationSet
{
	// Patterns to match modules to be included in generation.
	TArray<FNamePattern> ModulePatterns;

	/**
	 * Modules with class specific rules.
	 * These are only considered if the module matches a pattern in @ref ModulePatterns
	 */
	TArray<FModuleGeneration> DetailedModules;
};

/**
 * Settings for binding generation.
 */
struct FSettings
{
	FModuleGenerationSet EngineModules;

	FModuleGenerationSet GameModules;

	FString OutputPath;

	FString MainModuleName;

	static FSettings Load(const TCHAR* Path);
};
