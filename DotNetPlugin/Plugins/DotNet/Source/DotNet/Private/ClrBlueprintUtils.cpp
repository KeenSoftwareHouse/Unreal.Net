// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

// Fill out your copyright notice in the Description page of Project Settings.

#include "ClrBlueprintUtils.h"
#include "DotNet.h"
#include "ClrHost.h"

void UClrBlueprintUtils::RunVoidMethod(FString Assembly, FString Class, FString Method)
{
	auto CoreClrHost = FDotNetModule::Get()->HostInstance;

	if (!CoreClrHost)
	{
		UE_LOG(LogTemp, Error, TEXT("Core CLR Host is not initialized."));
		return;
	}

	const auto MethodPtr = static_cast<void (*)()>(CoreClrHost->GetDelegate(Assembly, Class, Method));
	if (MethodPtr)
		MethodPtr();
	else
	UE_LOG(LogTemp, Error, TEXT("Cannot find method [%s]%s.%s"), *Assembly, *Class, *Method);
}
