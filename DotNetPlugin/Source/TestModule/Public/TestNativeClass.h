// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"


#include "Particles/Rotation/ParticleModuleRotationBase.h"
#include "UObject/Object.h"

#include "TestNativeClass.generated.h"

/**
 * Test Class.
 */
UCLASS(BlueprintType)
class TESTMODULE_API UTestNativeClass : public UObject
{
	GENERATED_BODY()

public:

	UFUNCTION(BlueprintCallable)
	static int AddNumbers(int lhs, int rhs)
	{
		return lhs + rhs;
	}

	UFUNCTION(BlueprintCallable)
	static void GetObject(UObject*& UObject)
	{
		// Use well known static object.
		UObject = GEngine;
	}
};


