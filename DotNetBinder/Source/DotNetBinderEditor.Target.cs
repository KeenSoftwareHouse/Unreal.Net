// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using UnrealBuildTool;
using System.Collections.Generic;

public class DotNetBinderEditorTarget : TargetRules
{
	public DotNetBinderEditorTarget( TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V2;
		ExtraModuleNames.AddRange( new string[] { "DotNetBinder" , "DotNetNativeBinder"} );
	}
}
