// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text;
using UnrealBuildTool;

public class DotNetPlugin : ModuleRules
{
	public DotNetPlugin(ReadOnlyTargetRules Target)
		: base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[]
			{ "Core", "CoreUObject", "Engine", "DotNet", "TestModule" });
		// After first build uncomment the two remaining modules below (and delete the line above).
		//{ "Core", "CoreUObject", "Engine", "DotNet", "TestModule", "Tests", "UnrealEngine"});
	}
}