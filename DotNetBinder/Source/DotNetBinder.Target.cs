// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using UnrealBuildTool;
using System.Collections.Generic;

public class DotNetBinderTarget : TargetRules
{
	public DotNetBinderTarget( TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;
		DefaultBuildSettings = BuildSettingsVersion.V2;
		ExtraModuleNames.AddRange( new string[] { "DotNetBinder" } );
	}
}
