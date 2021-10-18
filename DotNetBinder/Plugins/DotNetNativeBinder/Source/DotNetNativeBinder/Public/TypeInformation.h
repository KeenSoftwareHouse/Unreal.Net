// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#pragma once

#include "Json.h"

#include <CoreUObject.h>

#include "Programs/UnrealHeaderTool/Public/IScriptGeneratorPluginInterface.h"

// This makes things confusing, we want to use it as an enum field but engine defines it as macro.
#undef UProperty

struct FTypeInfo;
struct FTypeCollector;

enum class ETypeKind : uint8
{
	None,
	UPackage,
	UObject,
	UInterface,
	UStruct,
	UEnum,
	UProperty,
	UFunction,
	UDelegate,
};

inline TCHAR* ToString(ETypeKind Kind)
{
	switch (Kind)
	{
	default:
	case ETypeKind::None: return TEXT("None");
	case ETypeKind::UObject: return TEXT("UObject");
	case ETypeKind::UInterface: return TEXT("UInterface");
	case ETypeKind::UStruct: return TEXT("UStruct");
	case ETypeKind::UFunction: return TEXT("UFunction");
	case ETypeKind::UEnum: return TEXT("UEnum");
	case ETypeKind::UDelegate: return TEXT("UDelegate");
	case ETypeKind::UPackage: return TEXT("UPackage");
	case ETypeKind::UProperty: return TEXT("UProperty");
	}
}

struct FMetaInfo
{
	void* MetaInstance;

	TSharedPtr<FJsonObject> Serialized;

	explicit FMetaInfo(void* Meta);

	/**
	 * @brief Collect initial info about this type.
	 * @param Collector Type collector.
	 */
	virtual void CollectInfo(FTypeCollector* Collector);

	/**
	 * @brief Collect final info about this type before exporting.
	 * @param Collector Type collector.
	 */
	virtual void FinalizeInfo(FTypeCollector* Collector)
	{
	}

	void WriteTo(FString Destination, FString NameOverride = "");

	virtual ~FMetaInfo()
	{
	}

	virtual ETypeKind GetKind() = 0;

	virtual FString GetName() const = 0;
};

struct FPackageInfo : FMetaInfo
{
	typedef FMetaInfo Super;
	
	EBuildModuleType::Type ModuleType;

	TArray<FTypeInfo*> Types;

	explicit FPackageInfo(UPackage* Package);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	UPackage* GetPackage() const
	{
		return static_cast<UPackage*>(MetaInstance);
	}

	virtual ETypeKind GetKind() override
	{
		return ETypeKind::UPackage;
	}

	virtual FString GetName() const override
	{
		return FPackageName::GetShortName(GetPackage());
	}

	void SetType(EBuildModuleType::Type Type);
};

struct FFieldInfo : FMetaInfo
{
	typedef FMetaInfo Super;
	
	explicit FFieldInfo(UField* Field);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	virtual FString GetName() const override
	{
		return GetField()->GetName();
	}

	UField* GetField() const
	{
		return static_cast<UField*>(MetaInstance);
	}
};

struct FTypeInfo : FFieldInfo
{
	typedef FFieldInfo Super;
	
	explicit FTypeInfo(UField* Field);

	virtual void CollectInfo(FTypeCollector* Collector) override;
};

struct FEnumInfo : FTypeInfo
{
	typedef FTypeInfo Super;
	
	explicit FEnumInfo(UEnum* Enum);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	UEnum* GetEnum() const
	{
		return static_cast<UEnum*>(MetaInstance);
	}

	virtual ETypeKind GetKind() override
	{
		return ETypeKind::UEnum;
	}
};

struct FPropertyInfo : FMetaInfo
{
	typedef FMetaInfo Super;
	
	explicit FPropertyInfo(FProperty* Property);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	FProperty* GetProperty() const
	{
		return static_cast<FProperty*>(MetaInstance);
	}

	virtual FString GetName() const override
	{
		return GetProperty()->GetName();
	}

	virtual ETypeKind GetKind() override
	{
		return ETypeKind::UProperty;
	}

private:
	bool HasGenericTypeArguments;
	
	void SetType(FTypeCollector* Collector, UField* Field);

	void AddGenericArgument(FTypeCollector* Collector, FProperty* Property);
	void AddGenericArgument(FTypeCollector* Collector, UField* Field);
	void AddGenericArgument(TSharedPtr<FJsonObject> Type);
};

struct FStructInfo : FTypeInfo
{
	typedef FTypeInfo Super;
	
	TArray<FPropertyInfo*> Properties;

	explicit FStructInfo(UStruct* Struct);

	UStruct* GetStruct() const
	{
		return static_cast<UStruct*>(MetaInstance);
	}

	virtual void CollectInfo(FTypeCollector* Collector) override;
};

struct FScriptStructInfo : FStructInfo
{
	typedef FStructInfo Super;
	
	explicit FScriptStructInfo(UScriptStruct* Struct);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	UScriptStruct* GetScriptStruct() const
	{
		return static_cast<UScriptStruct*>(MetaInstance);
	}

	virtual ETypeKind GetKind() override
	{
		return ETypeKind::UStruct;
	}
};

struct FFunctionInfo : FFieldInfo
{
	typedef FFieldInfo Super;
	
	TArray<FPropertyInfo*> Parameters;

	FPropertyInfo* Return;

	explicit FFunctionInfo(UFunction* Function);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	UFunction* GetFunction() const
	{
		return static_cast<UFunction*>(MetaInstance);
	}

	virtual ETypeKind GetKind() override
	{
		return ETypeKind::UFunction;
	}
};

struct FClassInfo : FStructInfo
{
	typedef FStructInfo Super;
	
	TArray<FFunctionInfo*> Functions;

	explicit FClassInfo(UClass* Class);

	virtual void CollectInfo(FTypeCollector* Collector) override;

	UClass* GetClass() const
	{
		return static_cast<UClass*>(MetaInstance);
	}

	virtual ETypeKind GetKind() override
	{
		return ETypeKind::UObject;
	}
};

struct FTypeCollector
{
	TMap<UPackage*, FPackageInfo*> Packages;

	TMap<UClass*, FClassInfo*> Classes;

	TMap<UScriptStruct*, FScriptStructInfo*> Structs;

	TMap<UEnum*, FEnumInfo*> Enums;

	FPackageInfo* TouchPackage(UPackage* Package);

	void SetPackageType(FString PackageName, EBuildModuleType::Type Type);

	FFieldInfo* TouchField(UField* Field);
	
	FClassInfo* TouchClass(UClass* Class);

	FScriptStructInfo* TouchStruct(UScriptStruct* Struct);

	FEnumInfo* TouchEnum(UEnum* Enum);

	FPropertyInfo* TouchProperty(FProperty* Property);

	FFunctionInfo* TouchFunction(UFunction* Function);

	void ExportAllTypes(FString DestinationPath);
};
