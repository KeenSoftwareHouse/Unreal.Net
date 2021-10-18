// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using UnrealBuildTool;
using System.Collections.Generic;

public class DotNetPluginEditorTarget : TargetRules
{
	public DotNetPluginEditorTarget( TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V2;
		ExtraModuleNames.AddRange( new string[] { "DotNetPlugin" , "DotNetNativeBinder"} );
	}
}
