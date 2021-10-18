// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "FTestStruct.generated.h"

USTRUCT(BlueprintType)
struct FTestStruct
{
	GENERATED_BODY()
	
	UPROPERTY()
	float Pi = 3.14;
};
