// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "coreclrhost.h"

struct CoreClrEntryPoints
{
	coreclr_initialize_ptr Initialize;
	coreclr_shutdown_2_ptr Shutdown;
	coreclr_create_delegate_ptr CreateDelegate;
	coreclr_execute_assembly_ptr ExecuteAssembly;
};

