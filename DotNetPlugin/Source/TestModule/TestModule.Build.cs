// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using UnrealBuildTool;

public class TestModule : ModuleRules
{
	public TestModule(ReadOnlyTargetRules Target)
		: base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[]
			{"Core", "CoreUObject", "Engine"});
	}
}