// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "DotNetBinderGameModeBase.generated.h"

/**
 * 
 */
UCLASS()
class DOTNETBINDER_API ADotNetBinderGameModeBase : public AGameModeBase
{
	GENERATED_BODY()
	
public:
	
	UFUNCTION(BlueprintPure, Category=Megatest)
    static void DoSomething1(FVector Arg1, FVector& Arg2) {}

	/**
	* @param Arg2 out param.
	*/
	UFUNCTION(BlueprintPure, Category=Megatest)
    static void DoSomething2(FVector Arg1, FVector& Arg2) {}

	UFUNCTION(BlueprintPure, Category=Megatest)
	static void DoSomething3(FVector Arg1, FVector& OutArg2) {}
	
	UFUNCTION(BlueprintPure, Category=Megatest)
	static void DoSomething4(const FVector& Arg1, FVector& OutArg2) {}

	UFUNCTION(BlueprintPure, Category=Megatest)
    static void DoSomething5(ADotNetBinderGameModeBase *& GameMode) {}
};
