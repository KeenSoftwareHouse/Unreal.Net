// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "DotNet.h"

extern "C" {

DOTNET_API void UeLog_Log(ELogVerbosity::Type Verbosity, const UTF16CHAR* Msg)
{
	const FString Message(Msg);

	switch (Verbosity)
	{
	case ELogVerbosity::Fatal:
		UE_LOG(LogClr, Fatal, TEXT("%s"), *Message);
		break;
	case ELogVerbosity::Error:
		UE_LOG(LogClr, Error, TEXT("%s"), *Message);
		break;
	case ELogVerbosity::Warning:
		UE_LOG(LogClr, Warning, TEXT("%s"), *Message);
		break;
	case ELogVerbosity::Display:
		UE_LOG(LogClr, Display, TEXT("%s"), *Message);
		break;
	case ELogVerbosity::Log:
		UE_LOG(LogClr, Log, TEXT("%s"), *Message);
		break;
	case ELogVerbosity::Verbose:
		UE_LOG(LogClr, Verbose, TEXT("%s"), *Message);
		break;
	case ELogVerbosity::VeryVerbose:
		UE_LOG(LogClr, VeryVerbose, TEXT("%s"), *Message);
		break;
	default: ;
	}
}

DOTNET_API UEngine** UEngine_Get_GEngine()
{
	return &GEngine;
}

DOTNET_API void UEngine_AddOnScreenDebugMessage(UEngine* Engine, int Key, float Time, uint32_t Color, const UTF16CHAR* Msg)
{
	Engine->AddOnScreenDebugMessage(Key, Time, FColor(Color), FString(Msg));
}

}
