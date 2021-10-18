// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/Object.h"
#include "ClrBlueprintUtils.generated.h"

/**
 * 
 */
UCLASS()
class DOTNET_API UClrBlueprintUtils : public UObject
{
	GENERATED_BODY()

public:
	/**
	* Run any managed static method that takes no arguments and returns void.
	* @remark This function cannot validate the method arguments and return value, make sure you call the correct method or it may crash the runtime.
	*/
	UFUNCTION(BlueprintCallable, Category=Clr)
	static void RunVoidMethod(FString Assembly, FString Class, FString Method);
};
