// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "ClrHost.h"

#include <string>


#include "DotNet.h"
#include "CoreClrEntryPoints.h"

#include "HAL/PlatformFilemanager.h"

static void AddFilesFromDirectoryToTpaList(const TCHAR* Directory, std::string& TpaList)
{
	const FString tpaExtensions[] = {
		".ni.dll", // Probe for .ni.dll first so that it's preferred if ni and il coexist in the same dir
		".dll",
		".ni.exe",
		".exe",
	};

	auto& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

	TArray<FString> Files;

	PlatformFile.IterateDirectoryStat(Directory, [&Files](const TCHAR* Path, const FFileStatData& Data) -> bool
	{
		if (!Data.bIsDirectory)
		{
			const auto FixePath = FString(Path).Replace(TEXT("/"), TEXT("\\"));
			Files.Add(FixePath);
		}
		return true;
	});

	TSet<FString> AddedAssemblies;

	// Walk the directory for each extension separately so that we first get files with .ni.dll extension,
	// then files with .dll extension, etc.
	for (int extIndex = 0; extIndex < sizeof(tpaExtensions) / sizeof(tpaExtensions[0]); extIndex++)
	{
		auto ext = tpaExtensions[extIndex];

		// For all entries in the directory
		for (auto File : Files)
		{
			// Check if the extension matches the one we are looking for
			if (!File.EndsWith(ext))
				continue;

			auto NoExtension = File.LeftChop(ext.Len());

			if (AddedAssemblies.Contains(NoExtension))
				continue;

			AddedAssemblies.Add(NoExtension);
			TpaList.append(StringCast<char>(*File).Get());

#ifdef PLATFORM_WINDOWS
			TpaList.append(";");
#else
			TpaList.append(":");
#endif
		}
	}
}

#define ENTRIES() static_cast<const CoreClrEntryPoints*>(this->EntryPoints)

FString ClrHost::GetAppPath()
{
	auto* Module = static_cast<FDotNetModule*>(FModuleManager::Get().GetModule("DotNet"));

	// Library not loaded.
	if (!Module->LoadedSuccessfully)
		return "";

	return Module->AppPath;
}

ClrHost::ClrHost(FString AppDomainName)
{
	// Get the module.
	auto* Module = static_cast<FDotNetModule*>(FModuleManager::Get().GetModule("DotNet"));

	// Library not loaded.
	if (!Module->LoadedSuccessfully)
		throw InitializationException();

	const FString& BclPath = Module->BclPath;
	const FString& AppPath = Module->AppPath;

	const auto Launch = FPlatformProcess::ExecutablePath();

	UE_LOG(LogClr, Display, TEXT("Initializing Core CLR"));
	UE_LOG(LogClr, Log, TEXT("App Path: %s"), *AppPath);
	UE_LOG(LogClr, Log, TEXT("App Domain Name: %s"), *AppDomainName);

	const auto AnsiPath = StringCast<char>(*AppPath);
	const auto AnsiLaunch = StringCast<char>(Launch);
	const auto AnsiDomainName = StringCast<char>(*AppDomainName);

	std::string TpaList;
	AddFilesFromDirectoryToTpaList(*BclPath, TpaList);

	const char* PropertyKeys[] = {
		"TRUSTED_PLATFORM_ASSEMBLIES",
		"APP_PATHS",
		"System.GC.Server",
		"System.Globalization.Invariant"
	};
	const char* PropertyValues[] = {
		// TRUSTED_PLATFORM_ASSEMBLIES
		TpaList.c_str(),
		// APP_PATHS
		AnsiPath.Get(),
		"true",
		"true"
	};

	EntryPoints = Module->CoreLibraryEntryPoints;
	const int Result = ENTRIES()->Initialize(AnsiLaunch.Get(), AnsiDomainName.Get(),
	                                         sizeof(PropertyKeys) / sizeof(const char*),
	                                         PropertyKeys, PropertyValues,
	                                         &RuntimeInstance, &AppDomainId);

	if (Result != 0)
	{
		UE_LOG(LogClr, Error, TEXT("Core CLR Initialization Failed: 0x%08X"), Result)
		throw InitializationException();
	}

	UE_LOG(LogClr, Display, TEXT("Core CLR Initialized, App Domain Handle: 0x%X"), AppDomainId)
}

ClrHost::~ClrHost()
{
	int ReturnCode;
	const auto Result = ENTRIES()->Shutdown(RuntimeInstance, AppDomainId, &ReturnCode);;

	if (Result == 0)
	{
		UE_LOG(LogClr, Display, TEXT("Core CLR Shut Down, Return Code: %d"), ReturnCode);
	}
	else
	{
		UE_LOG(LogClr, Error, TEXT("Core CLR Shutdown Failed: 0x%08X"), Result);
	}
}

void* ClrHost::GetDelegate(FString AssemblyName, FString TypeName, FString MethodName)
{
	const auto AnsiAssemblyName = StringCast<char>(*AssemblyName);
	const auto AnsiTypeName = StringCast<char>(*TypeName);
	const auto AnsiMethodName = StringCast<char>(*MethodName);

	void* Delegate;
	const auto Result = ENTRIES()->CreateDelegate(RuntimeInstance, AppDomainId, AnsiAssemblyName.Get(),
	                                              AnsiTypeName.Get(),
	                                              AnsiMethodName.Get(), &Delegate);

	if (Result != 0)
	UE_LOG(LogClr, Error, TEXT("Core CLR: Could not create delegate for method [%s]%s.%s: 0x%08X"), *AssemblyName,
	       *TypeName, *MethodName, Result);

	return Delegate;
}

int ClrHost::ExecuteAssembly(FString AssemblyPath, const TArray<FString>& Arguments)
{
	auto& Files = FPlatformFileManager::Get().GetPlatformFile();

	if (!Files.FileExists(*AssemblyPath))
		return 0x80070002; // ERROR_FILE_NOT_FOUND

	const auto AnsiAssemblyName = StringCast<char>(*AssemblyPath);

	const char* * AnsiArguments = new const char*[Arguments.Num()];

	for (int i = 0; i < Arguments.Num(); ++i)
		AnsiArguments[i] = _strdup(StringCast<char>(*Arguments[i]).Get());

	unsigned ReturnCode;
	const auto Result = ENTRIES()->ExecuteAssembly(RuntimeInstance, AppDomainId, Arguments.Num(), AnsiArguments,
	                                               AnsiAssemblyName.Get(),
	                                               &ReturnCode);

	if (Result != 0)
	{
		UE_LOG(LogClr, Error, TEXT("Core CLR: Could not execute assembly %s: 0x%08X"), *AssemblyPath, Result);
		return -1;
	}

	// Free strings.
	for (int i = 0; i < Arguments.Num(); ++i)
		free(const_cast<void*>(static_cast<const void*>(AnsiArguments[i])));

	delete[] AnsiArguments;

	return ReturnCode;
}