// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text;
using UnrealBuildTool;

public class DotNet : ModuleRules
{
	public DotNet(ReadOnlyTargetRules Target)
		: base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
		//bEnableExceptions = true;

		PublicDependencyModuleNames.AddRange(new[] {"Core", "CoreUObject", "Engine", "InputCore"});

		PrivateDefinitions.Add("BUILD_JIT");
		
		var sb = new StringBuilder();

		try
		{
			// App and runtime DLLs
			string appPath = Path.Combine(PluginDirectory, @"Binaries\ThirdParty\DotNetLibrary\Win64\RuntimeBinaries");
			foreach (var dll in Directory.EnumerateFiles(appPath, "*.dll"))
			{
				RuntimeDependencies.Add(Path.Combine("$(TargetOutputDir)", Path.GetFileName(dll)), dll);
				sb.AppendLine(dll);
			}

			/*PublicAdditionalLibraries.Add(
				@"C:\Users\Daniel\.nuget\packages\runtime.win-x64.microsoft.dotnet.ilcompiler\6.0.0-dev\sdk\bootstrapperdll.lib");
			PublicAdditionalLibraries.Add(
				@"C:\Users\Daniel\.nuget\packages\runtime.win-x64.microsoft.dotnet.ilcompiler\6.0.0-dev\sdk\Runtime.lib");
			PublicAdditionalLibraries.Add(
				@"C:\Users\Daniel\.nuget\packages\runtime.win-x64.microsoft.dotnet.ilcompiler\6.0.0-dev\framework\System.IO.Compression.Native-static.lib");
			
			PublicAdditionalLibraries.Add("bcrypt.lib");*/
			
			PublicIncludePaths.AddRange(
				new string[]
					{ }
			);

			// I Guess here I should add any headers I produce myself.
			/*PublicIncludePaths.AddRange(
				new string[]
				{
					Path.Combine(PluginDirectory, @"Source/ThirdParty/DotnetInUE/Artefacts/inc")
				}
			);*/

			PublicDependencyModuleNames.AddRange(
				new string[]
				{
					"Core",
					"Projects"
					// ... add other public dependencies that you statically link with here ...
				}
			);

			PrivateDependencyModuleNames.AddRange(
				new string[]
				{
					// ... add private dependencies that you statically link with here ...	
				}
			);

			DynamicallyLoadedModuleNames.AddRange(
				new string[]
				{
					// ... add any modules that your module loads dynamically here ...
				}
			);
		}
		catch (Exception e)
		{
			sb.Append(e);
		}

		// Diag file
		var outfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "uebuild.txt");
		File.WriteAllText(outfile, sb.ToString());
	}
}

public abstract class DotnetModuleRules : ModuleRules
{
	private static bool BuildJit = true;

	public DotnetModuleRules(ReadOnlyTargetRules Target)
		: base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new[] {"Core", "CoreUObject", "DotNet"});

		if (BuildJit)
			SetupJit();
		else
			SetupAot();
	}

	protected virtual void SetupJit()
	{
		PrivateDefinitions.Add("BUILD_JIT");
	}

	protected virtual void SetupAot()
	{
		PrivateDefinitions.Add("BUILD_AOT");

		// TODO: Inject code to collect assemblies here.
		// Similar to this
		/*PublicAdditionalLibraries.Add(
			@"C:\Users\Daniel\.nuget\packages\runtime.win-x64.microsoft.dotnet.ilcompiler\6.0.0-dev\sdk\bootstrapperdll.lib");
		PublicAdditionalLibraries.Add(
			@"C:\Users\Daniel\.nuget\packages\runtime.win-x64.microsoft.dotnet.ilcompiler\6.0.0-dev\sdk\Runtime.lib");
		PublicAdditionalLibraries.Add(
			@"C:\Users\Daniel\.nuget\packages\runtime.win-x64.microsoft.dotnet.ilcompiler\6.0.0-dev\framework\System.IO.Compression.Native-static.lib");
			
		PublicAdditionalLibraries.Add("bcrypt.lib");
			
		var libPath = Path.Combine(BasePath, "DotNetUEServices.lib");
		PublicAdditionalLibraries.Add(libPath);*/
	}
}