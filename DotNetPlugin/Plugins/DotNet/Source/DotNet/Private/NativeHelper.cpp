// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "NativeHelper.h"

extern "C" DOTNET_API IManagedObject* NativeHelper_Cast_UObject_IManagedObject(UObject* Object)
{
	return dynamic_cast<IManagedObject*>(Object);
}

class FContextObjectManager
{
public:
	static size_t GetClassOffset()
	{
		return offsetof(UObject, ClassPrivate);
	}
};

extern "C" DOTNET_API size_t UObject_GetFieldOffset_UClass()
{
	return FContextObjectManager::GetClassOffset();
}

extern "C" DOTNET_API UClass* UClass_GetSuperClass(UClass* Class)
{
	return Class->GetSuperClass();
}

extern "C" DOTNET_API UField* UClass_Find(const UCS2CHAR * PackageName, const UCS2CHAR* ClassName)
{
	const auto Package = FindObject<UPackage>(ANY_PACKAGE, StringCast<TCHAR>(PackageName).Get());

	if (!Package)
		return nullptr;

	return FindObject<UField>(Package, StringCast<TCHAR>(ClassName).Get(), false);
}

extern "C" DOTNET_API size_t IManagedObject_GetFieldOffset_Handle()
{
	return FNativeHelper::ManagedObject_Handle_Offset;
}

extern "C" DOTNET_API UObject* NativeHelper_CreateUObject(UClass* Class, UObject* Outer)
{
	return NewObject<UObject>(Outer, Class);
}
