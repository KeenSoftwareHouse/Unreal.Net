// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

DECLARE_LOG_CATEGORY_EXTERN(LogClr, Log, All);

class ClrHost;

class DOTNET_API FDotNetModule : public IModuleInterface
{
public:
	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;

	static FDotNetModule* Get()
	{
		return static_cast<FDotNetModule*>(FModuleManager::Get().GetModule("DotNet"));
	}
	
	/**
	* @brief Get a function pointer to a managed entry point.
	*
	* That entry point must be a function marked with [UnmanagedCallersOnly].
	* If the function name is ambiguous nothing will be returned.
	* 
	* @param Assembly Name of the assembly where the method is defined.
	* @param Type Name of the type where the method is defined.
	* @param Function Name of the method.
	*/
	void* GetManagedEntryPoint(char* Assembly, char* Type, char* Function) const;

private:
	/** Handle to the test dll we will load */
	void* CoreClrLibraryHandle = nullptr;

	/** Set of entry points to the CoreClrLibrary .*/
	void* CoreLibraryEntryPoints = nullptr;

	/** Path to the clr directory. */
	FString ClrPath;

	/** Path to the directory containing BCL Assemblies. */
	FString BclPath;

	/** Path to the application's directory */
	FString AppPath;

	/** Whether the CLR Dll and entry points were loaded correctly. */
	bool LoadedSuccessfully = false;

	ClrHost* HostInstance = nullptr;

	friend class UClrBlueprintUtils;
	friend class ClrHost;

	typedef void* (* EntryPointGetter)(char * Assembly, char * Type, char * Function);

	EntryPointGetter EntryGetter;
};