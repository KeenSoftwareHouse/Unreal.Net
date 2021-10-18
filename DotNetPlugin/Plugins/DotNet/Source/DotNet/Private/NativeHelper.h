// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "ManagedObject.h"

class DOTNET_API FNativeHelper
{
public:
	static const int ManagedObject_Handle_Offset = offsetof(IManagedObject, ManagedObjectHandle);
};