// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "GenerationSettings.h"

#define SI_CONVERT_GENERIC
#include "SimpleIni/SimpleIni.h"

typedef CSimpleIniTempl<TCHAR, SI_Case<TCHAR>, SI_ConvertW<TCHAR>> CSimpleIniTChar;

FSettings FSettings::Load(const TCHAR* Path)
{
	auto& PlatformFile = IPlatformFile::GetPlatformPhysical();

	FSettings Settings;

	if (PlatformFile.FileExists(Path))
	{
		CSimpleIniTChar Archive(true, false, true);
		Archive.LoadFile(Path);

		FString RequestedModules = Archive.GetValue(TEXT("Settings"), TEXT("Modules"));

		Settings.MainModuleName = Archive.GetValue(TEXT("Settings"), TEXT("MainModule"));
		Settings.OutputPath = Archive.GetValue(TEXT("Settings"), TEXT("OutputPath"));
	}

	return Settings;
}
