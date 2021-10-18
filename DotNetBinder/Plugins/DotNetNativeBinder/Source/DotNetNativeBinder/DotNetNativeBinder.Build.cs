// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using UnrealBuildTool;

public class DotNetNativeBinder : ModuleRules
{
	public DotNetNativeBinder(ReadOnlyTargetRules Target)
		: base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
		bUseUnity = false;
		
		OptimizeCode = CodeOptimization.Never;
		
		PrivateIncludePaths.AddRange(
			new[]
			{
				"Programs/UnrealHeaderTool/Public"
			}
		);

		PublicDependencyModuleNames.AddRange(
			new[]
			{
				"Core"
			}
		);

		PrivateDependencyModuleNames.AddRange(
			new[]
			{
				"CoreUObject",
				"Json"
			}
		);
	}
}