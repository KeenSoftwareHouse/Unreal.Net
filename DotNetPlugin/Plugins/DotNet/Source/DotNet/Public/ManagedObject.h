// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "ManagedObject.generated.h"

UINTERFACE()
class DOTNET_API UManagedObject : public UInterface
{
	GENERATED_BODY()
};

class DOTNET_API IManagedObject
{
	GENERATED_BODY()

	void* ManagedObjectHandle = nullptr;

	friend class FNativeHelper;

public:

	void* GetManagedHandle() const
	{
		return ManagedObjectHandle;
	}

protected:

	typedef void* (*CreateManagedInstanceFunc)(void* ThisInstance);

	IManagedObject()
	{
	}

	IManagedObject(const CreateManagedInstanceFunc CreateInstance, void* ThisInstance)
	{
		ManagedObjectHandle = CreateInstance(ThisInstance);
	}
};
