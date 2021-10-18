// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "CoreMinimal.h"

/**
 * Hosts the JIT Clr.
 */
class ClrHost
{
	void* RuntimeInstance = nullptr;
	unsigned AppDomainId = 0;

	const void* EntryPoints = nullptr;
	
public:

	static FString GetAppPath();

	/**
	 * Initialize a new CLR host with a given name to it's main app domain.
	 * @throw InitializationException When it's not possible to initialize the CLR Host.
	 */
	explicit ClrHost(FString AppDomainName);

	virtual ~ClrHost();

	void* GetDelegate(FString AssemblyName, FString TypeName, FString MethodName);

	int ExecuteAssembly(FString AssemblyPath, const TArray<FString>& Argument);

	struct InitializationException
	{
	};
};
