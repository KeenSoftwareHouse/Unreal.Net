// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#include "TypeInformation.h"

#include "DotNetNativeBinder.h"

//==============================================================================
//= Helper Declaration

static void AddFlag(FString& Str, FString Flag);
static FString FormatClassFlags(EClassFlags Flags);
static FString FormatFunctionFlags(EFunctionFlags Flags);
static FString FormatPropertyFlags(EPropertyFlags Flags);
static void SetJsonNumberField(TSharedPtr<FJsonObject> Object, FString Key, int64 Value);
static TCHAR* EnumToString(EBuildModuleType::Type ModuleType);

//==============================================================================
//= Implementation

FMetaInfo::FMetaInfo(void* Meta)
	: MetaInstance(Meta), Serialized(MakeShared<FJsonObject>())
{
}

void FMetaInfo::CollectInfo(FTypeCollector* Collector)
{
	const auto Kind = GetKind();
	if (Kind != ETypeKind::None)
		Serialized->SetStringField("Kind", ToString(Kind));
}

void FMetaInfo::WriteTo(FString Destination, FString NameOverride)
{
	// TODO: This can be done better by creating an archive directly.
	FString FileContents;
	TSharedRef<TJsonWriter<>> JsonWriter = TJsonWriterFactory<>::Create(&FileContents);

	const auto Name = NameOverride.Len() > 0 ? NameOverride : GetName();

	const auto Path = FPaths::Combine(Destination, Name + ".umeta");

	if (FJsonSerializer::Serialize(Serialized.ToSharedRef(), JsonWriter))
	{
		JsonWriter->Close();
		const auto Result =
			FFileHelper::SaveStringToFile(FileContents, *Path, FFileHelper::EEncodingOptions::ForceUTF8);
		if (!Result)
		{
			UE_LOG(LogDotNetGenerator, Error, TEXT("Could not serialize file %s"), *Path);
		}
	}
}

//======================================
//= Package


FPackageInfo::FPackageInfo(UPackage* Package): FMetaInfo(Package), ModuleType(EBuildModuleType::Max)
{
}

void FPackageInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	Serialized->SetStringField("LongName", GetPackage()->GetName());
	
	Serialized->SetStringField("Name", FPackageName::GetShortName(GetPackage()));

	Serialized->SetStringField("Folder", GetPackage()->GetFolderName().ToString());

	Serialized->SetStringField("File", GetPackage()->FileName.ToString());
}

void FPackageInfo::SetType(EBuildModuleType::Type Type)
{
	this->ModuleType = Type;
	Serialized->SetStringField("PackageType", EnumToString(Type));
}

//======================================
//= Field

inline TSharedPtr<FJsonObject> SerializeMetadata(const TMap<FName, FString>* Map)
{
	if (!Map)
		return TSharedPtr<FJsonObject>();

	auto SerializedMap = MakeShared<FJsonObject>();
	for (auto Pair : *Map)
		SerializedMap->SetStringField(Pair.Key.ToString(), Pair.Value);

	return SerializedMap;
}

inline TSharedPtr<FJsonObject> SerializeMetadata(UField* Field)
{
	auto Package = Field->GetOutermost();

	if (!Package)
		return TSharedPtr<FJsonObject>();

	auto Metadata = Package->GetMetaData();
	if (!Metadata)
		return TSharedPtr<FJsonObject>();

	return SerializeMetadata(Metadata->GetMapForObject(Field));
}


FFieldInfo::FFieldInfo(UField* Field) : FMetaInfo(Field)
{
}

void FFieldInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	const auto Field = GetField();
	
	Serialized->SetStringField("Name", Field->GetName());

	const auto Meta = SerializeMetadata(Field);
	if (Meta)
		Serialized->SetObjectField("Meta", Meta);
}

//======================================
//= Type Info

FTypeInfo::FTypeInfo(UField* Field): FFieldInfo(Field)
{
}

void FTypeInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	const auto Package = GetField()->GetPackage();

	const auto PackageInfo = Collector->TouchPackage(Package);
	PackageInfo->Types.Add(this);

	Serialized->SetStringField("Module", PackageInfo->GetName());

	//Serialized->SetStringField("Header", Header);
}

//======================================
//= Enum

static TCHAR* ToString(UEnum::ECppForm Form)
{
	switch (Form)
	{
	case UEnum::ECppForm::Regular: return TEXT("Regular");
	case UEnum::ECppForm::Namespaced: return TEXT("Namespaced");
	case UEnum::ECppForm::EnumClass: return TEXT("EnumClass");
	default: return TEXT("");
	}
}

FEnumInfo::FEnumInfo(UEnum* Enum) : FTypeInfo(Enum)
{
}

void FEnumInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	const auto Enum = GetEnum();

	Serialized->SetStringField("EnumKind", ToString(Enum->GetCppForm()));
	Serialized->SetStringField("CppName", Enum->CppType);

	Serialized->SetBoolField("IsFlags", Enum->HasAnyEnumFlags(EEnumFlags::Flags));

	SetJsonNumberField(Serialized, "MaximumValue", Enum->GetMaxEnumValue());

	// Collect Values
	TArray<TSharedPtr<FJsonValue>> Values;

	FName Name;
	for (int i = 0; !(Name = Enum->GetNameByIndex(i)).IsNone(); ++i)
	{
		auto Element = MakeShared<FJsonObject>();
		Element->SetStringField("Name", Name.ToString());

		SetJsonNumberField(Element, "Value", Enum->GetValueByIndex(i));

		Values.Add(MakeShared<FJsonValueObject>(Element));
	}

	Serialized->SetArrayField("Values", Values);
}

//======================================
//= Struct

static TSharedPtr<FJsonObject> SerializeAndCollectFieldType(FTypeCollector* Collector, UField* Field)
{
	auto Type = MakeShared<FJsonObject>();
	FFieldInfo* info = Collector->TouchField(Field);

	Type->SetStringField("FieldType", "TypeName");

	Type->SetStringField("Module", FPackageName::GetShortName(Field->GetPackage()));

	Type->SetStringField("Name", Field->GetName());

	auto cppName = info->Serialized->GetStringField("CppName");

	Type->SetStringField("CppName", cppName);

	return Type;
}

FStructInfo::FStructInfo(UStruct* Struct): FTypeInfo(Struct)
{
}

void FStructInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	const auto Struct = GetStruct();

	Serialized->SetStringField("CppName", Struct->GetPrefixCPP() + Struct->GetName());

	Serialized->SetNumberField("Size", Struct->GetStructureSize());

	// Do after name because properties can cause this reference to be used before it's done collecting.
	const auto SuperStruct = Struct->GetSuperStruct();
	if (SuperStruct)
		Serialized->SetObjectField("Parent", SerializeAndCollectFieldType(Collector, SuperStruct));
	

	TArray<TSharedPtr<FJsonValue>> SerializedProperties;

	TFieldIterator<FProperty> PropertyIterator(Struct, EFieldIteratorFlags::ExcludeSuper);
	for (; PropertyIterator; ++PropertyIterator)
	{
		const auto Index = Properties.Add(Collector->TouchProperty(*PropertyIterator));
		auto& Property = Properties[Index];

		SerializedProperties.Add(MakeShared<FJsonValueObject>(Property->Serialized));
	}

	Serialized->SetArrayField("Properties", SerializedProperties);
}

//======================================
//= Script Struct

FScriptStructInfo::FScriptStructInfo(UScriptStruct* Struct) : FStructInfo(Struct)
{
}

void FScriptStructInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);
}

//======================================
//= Class

FClassInfo::FClassInfo(UClass* Class): FStructInfo(Class)
{
}

void FClassInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	const auto Class = GetClass();

	TArray<TSharedPtr<FJsonValue>> Interfaces;

	for (const auto IFace : Class->Interfaces)
		Interfaces.Add(MakeShared<FJsonValueString>(IFace.Class->GetName()));

	Serialized->SetNumberField("Flags", Class->ClassFlags);
	Serialized->SetStringField("FlagsText", FormatClassFlags(Class->ClassFlags));

	Serialized->SetArrayField("Interfaces", Interfaces);

	TArray<TSharedPtr<FJsonValue>> SerializedFunctions;

	TFieldIterator<UFunction> FunctionIt(Class, EFieldIteratorFlags::ExcludeSuper);
	for (; FunctionIt; ++FunctionIt)
	{
		const auto Function = Collector->TouchFunction(*FunctionIt);

		Functions.Add(Function);
		SerializedFunctions.Add(MakeShared<FJsonValueObject>(Function->Serialized));
	}

	Serialized->SetArrayField("Functions", SerializedFunctions);
}

//======================================
//= Function

FFunctionInfo::FFunctionInfo(UFunction* Function) : FFieldInfo(Function), Return(nullptr)
{
}

static TSharedPtr<FJsonObject> WriteVoid()
{
	auto obj = MakeShared<FJsonObject>();

	obj->SetStringField("RawType", "void");

	return obj;
}

void FFunctionInfo::CollectInfo(FTypeCollector* Collector)
{
	Super::CollectInfo(Collector);

	const auto Function = GetFunction();

	TArray<TSharedPtr<FJsonValue>> SerializedParameters;

	TFieldIterator<FProperty> PropertyIterator(Function, EFieldIteratorFlags::ExcludeSuper);
	for (; PropertyIterator; ++PropertyIterator)
	{
		const auto Property = Collector->TouchProperty(*PropertyIterator);

		if (Property->GetProperty()->HasAnyPropertyFlags(CPF_ReturnParm))
		{
			Return = Property;
		}
		else
		{
			Parameters.Add(Property);
			SerializedParameters.Add(MakeShared<FJsonValueObject>(Property->Serialized));
		}
	}

	if (Function->HasAnyFunctionFlags(FUNC_Const))
		Serialized->SetBoolField("Const", true);

	Serialized->SetBoolField("Static", Function->HasAnyFunctionFlags(FUNC_Static));
	Serialized->SetBoolField("Final", Function->HasAnyFunctionFlags(FUNC_Final));

	Serialized->SetNumberField("Flags", Function->FunctionFlags);

	Serialized->SetStringField("FlagsText", FormatFunctionFlags(Function->FunctionFlags));

	Serialized->SetArrayField("Parameters", SerializedParameters);

	if (Return)
		Serialized->SetObjectField("Return", Return->Serialized);
	else
		Serialized->SetObjectField("Return", WriteVoid());
}

//======================================
//= Property

FPropertyInfo::FPropertyInfo(FProperty* Property): FMetaInfo(Property), HasGenericTypeArguments(false)
{
}

void FPropertyInfo::CollectInfo(FTypeCollector* Collector)
{
	const auto Property = GetProperty();

	Serialized->SetStringField("Name", Property->GetName());

	Serialized->SetStringField("RawType", Property->GetCPPType());

	const auto Meta = SerializeMetadata(Property->GetMetaDataMap());
	if (Meta)
		Serialized->SetObjectField("Meta", Meta);

	Serialized->SetNumberField("Offset", Property->GetOffset_ForInternal());

	SetJsonNumberField(Serialized, "Flags", Property->PropertyFlags);

	Serialized->SetStringField("FlagsText", FormatPropertyFlags(Property->PropertyFlags));

	Serialized->SetStringField("PropertyType", Property->GetClass()->GetName());

	if (Property->ArrayDim > 1)
	{
		Serialized->SetNumberField("ArrayDim", Property->ArrayDim);
	}
	// from the most common to the least
	if (Property->IsA<FStructProperty>())
	{
		const auto Prop = CastField<FStructProperty>(Property);
		SetType(Collector, Prop->Struct);
	}
	else if (Property->IsA<FObjectProperty>())
	{
		if (Property->IsA<FClassProperty>())
		{
			const auto Prop = CastField<FClassProperty>(Property);
			AddGenericArgument(Collector, Prop->MetaClass);
		}

		auto Prop = CastField<FObjectProperty>(Property);
		SetType(Collector, Prop->PropertyClass);
	}
	else if (Property->IsA<FWeakObjectProperty>())
	{
		auto Prop = CastField<FWeakObjectProperty>(Property);
		AddGenericArgument(Collector, Prop->PropertyClass);
	}
	else if (Property->IsA<FSoftObjectProperty>())
	{
		auto Prop = CastField<FSoftObjectProperty>(Property);
		AddGenericArgument(Collector, Prop->PropertyClass);
	}
	else if (Property->IsA<FLazyObjectProperty>())
	{
		auto Prop = CastField<FLazyObjectProperty>(Property);
		AddGenericArgument(Collector, Prop->PropertyClass);
	}
	else if (Property->IsA<FNumericProperty>())
	{
		const auto Numeric = CastField<FNumericProperty>(Property);

		UEnum* Enum = Numeric->GetIntPropertyEnum();
		if (Enum)
			AddGenericArgument(Collector, Enum);
	}
	else if (Property->IsA<FEnumProperty>())
	{
		auto Prop = CastField<FEnumProperty>(Property);
		SetType(Collector, Prop->GetEnum());
	}
	else if (Property->IsA<FBoolProperty>())
	{
		// Nothing
	}
	else if (Property->IsA<FNameProperty>())
	{
		// TODO: Marshal FName.
	}
	else if (Property->IsA<FStrProperty>())
	{
		// TODO: Marshal String.
	}
	else if (Property->IsA<FTextProperty>())
	{
		// TODO: Marshal Text.
	}
	else if (Property->IsA<FArrayProperty>())
	{
		auto Prop = CastField<FArrayProperty>(Property);
		AddGenericArgument(Collector, Prop->Inner);
	}
	else if (Property->IsA<FMapProperty>())
	{
		auto Prop = CastField<FMapProperty>(Property);

		AddGenericArgument(Collector, Prop->KeyProp);
		AddGenericArgument(Collector, Prop->ValueProp);
	}
	else if (Property->IsA<FSetProperty>())
	{
		auto Prop = CastField<FSetProperty>(Property);
		AddGenericArgument(Collector, Prop->ElementProp);
	}
	else if (Property->IsA<FMulticastDelegateProperty>())
	{
		// TODO: Multicast Delegate
	}
	else if (Property->IsA<FDelegateProperty>())
	{
		// TODO: Delegate
	}
	else
	{
		Serialized->SetBoolField("IsUnknown", true);
	}

	// TODO: There is more.
}


void FPropertyInfo::SetType(FTypeCollector* Collector, UField* Field)
{
	Serialized->SetObjectField("Type", SerializeAndCollectFieldType(Collector, Field));
}

inline void FPropertyInfo::AddGenericArgument(FTypeCollector* Collector, FProperty* Property)
{
	const auto KeyProp = Collector->TouchProperty(Property);

	auto Type = MakeShared<FJsonObject>();

	Type->SetStringField("FieldType", "Property");
	Type->SetObjectField("Property", KeyProp->Serialized);

	AddGenericArgument(Type);
}

inline void FPropertyInfo::AddGenericArgument(FTypeCollector* Collector, UField* Field)
{
	AddGenericArgument(SerializeAndCollectFieldType(Collector, Field));
}

void FPropertyInfo::AddGenericArgument(TSharedPtr<FJsonObject> Type)
{
	const TArray<TSharedPtr<FJsonValue>>* GenericParameters;
	if (!Serialized->TryGetArrayField("GenericTypeParameters", GenericParameters))
	{
		const TArray<TSharedPtr<FJsonValue>> Empty;
		Serialized->SetArrayField("GenericTypeParameters", Empty);
		GenericParameters = &Serialized->GetArrayField("GenericTypeParameters");
	}

	const_cast<TArray<TSharedPtr<FJsonValue>>*>(GenericParameters)->Add(MakeShared<FJsonValueObject>(Type));

	HasGenericTypeArguments = true;
}

//======================================
//= Type Collector

FPackageInfo* FTypeCollector::TouchPackage(UPackage* Package)
{
	auto Info = Packages.FindOrAdd(Package);
	if (!Info)
	{
		Info = Packages.Add(Package, new FPackageInfo(Package));
		Info->CollectInfo(this);
	}

	return Info;
}

void FTypeCollector::SetPackageType(const FString PackageName, const EBuildModuleType::Type Type)
{
	auto Name = FString::Printf(TEXT("/Script/%s"), *PackageName);

	const auto Package = FindPackage(nullptr, *Name);

	auto Info = TouchPackage(Package);
	Info->SetType(Type);
}

inline FFieldInfo* FTypeCollector::TouchField(UField* Field)
{
	if (Field->IsA<UClass>())
	{
		return TouchClass(Cast<UClass>(Field));
	}
	else if (Field->IsA<UScriptStruct>())
	{
		return TouchStruct(Cast<UScriptStruct>(Field));
	}
	else if (Field->IsA<UEnum>())
	{
		return TouchEnum(Cast<UEnum>(Field));
	}

	return nullptr;
}

FClassInfo* FTypeCollector::TouchClass(UClass* Class)
{
	auto Info = Classes.FindOrAdd(Class);
	if (!Info)
	{
		Info = Classes.Add(Class, new FClassInfo(Class));
		Info->CollectInfo(this);
	}

	return Info;
}

FScriptStructInfo* FTypeCollector::TouchStruct(UScriptStruct* Struct)
{
	auto info = Structs.FindOrAdd(Struct);
	if (!info)
	{
		info = Structs.Add(Struct, new FScriptStructInfo(Struct));
		info->CollectInfo(this);
	}

	return info;
}

FEnumInfo* FTypeCollector::TouchEnum(UEnum* Enum)
{
	auto info = Enums.FindOrAdd(Enum);
	if (!info)
	{
		info = Enums.Add(Enum, new FEnumInfo(Enum));
		info->CollectInfo(this);
	}

	return info;
}

FPropertyInfo* FTypeCollector::TouchProperty(FProperty* Property)
{
	FPropertyInfo* info = new FPropertyInfo(Property);
	info->CollectInfo(this);

	return info;
}

inline FFunctionInfo* FTypeCollector::TouchFunction(UFunction* Function)
{
	FFunctionInfo* info = new FFunctionInfo(Function);
	info->CollectInfo(this);

	return info;
}

void FTypeCollector::ExportAllTypes(FString DestinationPath)
{
	// TODO: We should be able to use a parallel for here.

	// TODO: In the haxe wrapper they also collect types that are loaded but not reported, we'll probably want to avoid
	// those for now since we can't use them anyway, 

	auto& File = IPlatformFile::GetPlatformPhysical();

	for (const auto Package : Packages)
	{
		auto Info = Package.Value;
		Info->WriteTo(*DestinationPath);

		auto PackageDir = FPaths::Combine(*DestinationPath, Info->GetName());
		File.CreateDirectoryTree(*PackageDir);

		for (auto Type : Info->Types)
			Type->WriteTo(PackageDir);
	}
}

//==============================================================================
//= Helpers

static void AddFlag(FString& Str, FString Flag)
{
	if (Str.Len() > 0)
		Str += " | ";
	Str += Flag;
}

static FString FormatClassFlags(EClassFlags Flags)
{
	FString String;

	if ((Flags & CLASS_Abstract) != 0) AddFlag(String, "Abstract");
	if ((Flags & CLASS_DefaultConfig) != 0) AddFlag(String, "DefaultConfig");
	if ((Flags & CLASS_Config) != 0) AddFlag(String, "Config");
	if ((Flags & CLASS_Transient) != 0) AddFlag(String, "Transient");
	if ((Flags & CLASS_Parsed) != 0) AddFlag(String, "Parsed");
	if ((Flags & CLASS_MatchedSerializers) != 0) AddFlag(String, "MatchedSerializers");
	if ((Flags & CLASS_ProjectUserConfig) != 0) AddFlag(String, "ProjectUserConfig");
	if ((Flags & CLASS_Native) != 0) AddFlag(String, "Native");
	if ((Flags & CLASS_NoExport) != 0) AddFlag(String, "NoExport");
	if ((Flags & CLASS_NotPlaceable) != 0) AddFlag(String, "NotPlaceable");
	if ((Flags & CLASS_PerObjectConfig) != 0) AddFlag(String, "PerObjectConfig");
	if ((Flags & CLASS_ReplicationDataIsSetUp) != 0) AddFlag(String, "ReplicationDataIsSetUp");
	if ((Flags & CLASS_EditInlineNew) != 0) AddFlag(String, "EditInlineNew");
	if ((Flags & CLASS_CollapseCategories) != 0) AddFlag(String, "CollapseCategories");
	if ((Flags & CLASS_Interface) != 0) AddFlag(String, "Interface");
	if ((Flags & CLASS_CustomConstructor) != 0) AddFlag(String, "CustomConstructor");
	if ((Flags & CLASS_Const) != 0) AddFlag(String, "Const");
	if ((Flags & CLASS_LayoutChanging) != 0) AddFlag(String, "LayoutChanging");
	if ((Flags & CLASS_CompiledFromBlueprint) != 0) AddFlag(String, "CompiledFromBlueprint");
	if ((Flags & CLASS_MinimalAPI) != 0) AddFlag(String, "MinimalAPI");
	if ((Flags & CLASS_RequiredAPI) != 0) AddFlag(String, "RequiredAPI");
	if ((Flags & CLASS_DefaultToInstanced) != 0) AddFlag(String, "DefaultToInstanced");
	if ((Flags & CLASS_TokenStreamAssembled) != 0) AddFlag(String, "TokenStreamAssembled");
	if ((Flags & CLASS_Hidden) != 0) AddFlag(String, "Hidden");
	if ((Flags & CLASS_Deprecated) != 0) AddFlag(String, "Deprecated");
	if ((Flags & CLASS_HideDropDown) != 0) AddFlag(String, "HideDropDown");
	if ((Flags & CLASS_GlobalUserConfig) != 0) AddFlag(String, "GlobalUserConfig");
	if ((Flags & CLASS_Intrinsic) != 0) AddFlag(String, "Intrinsic");
	if ((Flags & CLASS_Constructed) != 0) AddFlag(String, "Constructed");
	if ((Flags & CLASS_ConfigDoNotCheckDefaults) != 0) AddFlag(String, "ConfigDoNotCheckDefaults");
	if ((Flags & CLASS_NewerVersionExists) != 0) AddFlag(String, "NewerVersionExists");

	return String;
}

static FString FormatFunctionFlags(const EFunctionFlags Flags)
{
	FString String;

	// ReSharper disable StringLiteralTypo
	if ((Flags & FUNC_Final) != 0) AddFlag(String, "Final");
	if ((Flags & FUNC_RequiredAPI) != 0) AddFlag(String, "RequiredAPI");
	if ((Flags & FUNC_BlueprintAuthorityOnly) != 0) AddFlag(String, "BlueprintAuthorityOnly");
	if ((Flags & FUNC_BlueprintCosmetic) != 0) AddFlag(String, "BlueprintCosmetic");
	if ((Flags & FUNC_Net) != 0) AddFlag(String, "Net");
	if ((Flags & FUNC_NetReliable) != 0) AddFlag(String, "NetReliable");
	if ((Flags & FUNC_NetRequest) != 0) AddFlag(String, "NetRequest");
	if ((Flags & FUNC_Exec) != 0) AddFlag(String, "Exec");
	if ((Flags & FUNC_Native) != 0) AddFlag(String, "Native");
	if ((Flags & FUNC_Event) != 0) AddFlag(String, "Event");
	if ((Flags & FUNC_NetResponse) != 0) AddFlag(String, "NetResponse");
	if ((Flags & FUNC_Static) != 0) AddFlag(String, "Static");
	if ((Flags & FUNC_NetMulticast) != 0) AddFlag(String, "NetMulticast");
	if ((Flags & FUNC_UbergraphFunction) != 0) AddFlag(String, "UbergraphFunction");
	if ((Flags & FUNC_MulticastDelegate) != 0) AddFlag(String, "MulticastDelegate");
	if ((Flags & FUNC_Public) != 0) AddFlag(String, "Public");
	if ((Flags & FUNC_Private) != 0) AddFlag(String, "Private");
	if ((Flags & FUNC_Protected) != 0) AddFlag(String, "Protected");
	if ((Flags & FUNC_Delegate) != 0) AddFlag(String, "Delegate");
	if ((Flags & FUNC_NetServer) != 0) AddFlag(String, "NetServer");
	if ((Flags & FUNC_HasOutParms) != 0) AddFlag(String, "HasOutParms");
	if ((Flags & FUNC_HasDefaults) != 0) AddFlag(String, "HasDefaults");
	if ((Flags & FUNC_NetClient) != 0) AddFlag(String, "NetClient");
	if ((Flags & FUNC_DLLImport) != 0) AddFlag(String, "DLLImport");
	if ((Flags & FUNC_BlueprintCallable) != 0) AddFlag(String, "BlueprintCallable");
	if ((Flags & FUNC_BlueprintEvent) != 0) AddFlag(String, "BlueprintEvent");
	if ((Flags & FUNC_BlueprintPure) != 0) AddFlag(String, "BlueprintPure");
	if ((Flags & FUNC_EditorOnly) != 0) AddFlag(String, "EditorOnly");
	if ((Flags & FUNC_Const) != 0) AddFlag(String, "Const");
	if ((Flags & FUNC_NetValidate) != 0) AddFlag(String, "NetValidate");
	// ReSharper restore StringLiteralTypo

	return String;
}

static FString FormatPropertyFlags(EPropertyFlags Flags)
{
	FString String;

	// ReSharper disable StringLiteralTypo
	if ((Flags & CPF_Edit) != 0) AddFlag(String, "Edit");
	if ((Flags & CPF_ConstParm) != 0) AddFlag(String, "ConstParm");
	if ((Flags & CPF_BlueprintVisible) != 0) AddFlag(String, "BlueprintVisible");
	if ((Flags & CPF_ExportObject) != 0) AddFlag(String, "ExportObject");
	if ((Flags & CPF_BlueprintReadOnly) != 0) AddFlag(String, "BlueprintReadOnly");
	if ((Flags & CPF_Net) != 0) AddFlag(String, "Net");
	if ((Flags & CPF_EditFixedSize) != 0) AddFlag(String, "EditFixedSize");
	if ((Flags & CPF_Parm) != 0) AddFlag(String, "Parm");
	if ((Flags & CPF_OutParm) != 0) AddFlag(String, "OutParm");
	if ((Flags & CPF_ZeroConstructor) != 0) AddFlag(String, "ZeroConstructor");
	if ((Flags & CPF_ReturnParm) != 0) AddFlag(String, "ReturnParm");
	if ((Flags & CPF_DisableEditOnTemplate) != 0) AddFlag(String, "DisableEditOnTemplate");
	if ((Flags & CPF_Transient) != 0) AddFlag(String, "Transient");
	if ((Flags & CPF_Config) != 0) AddFlag(String, "Config");
	if ((Flags & CPF_DisableEditOnInstance) != 0) AddFlag(String, "DisableEditOnInstance");
	if ((Flags & CPF_EditConst) != 0) AddFlag(String, "EditConst");
	if ((Flags & CPF_GlobalConfig) != 0) AddFlag(String, "GlobalConfig");
	if ((Flags & CPF_InstancedReference) != 0) AddFlag(String, "InstancedReference");
	if ((Flags & CPF_DuplicateTransient) != 0) AddFlag(String, "DuplicateTransient");
	if ((Flags & CPF_SaveGame) != 0) AddFlag(String, "SaveGame");
	if ((Flags & CPF_NoClear) != 0) AddFlag(String, "NoClear");
	if ((Flags & CPF_ReferenceParm) != 0) AddFlag(String, "ReferenceParm");
	if ((Flags & CPF_BlueprintAssignable) != 0) AddFlag(String, "BlueprintAssignable");
	if ((Flags & CPF_Deprecated) != 0) AddFlag(String, "Deprecated");
	if ((Flags & CPF_IsPlainOldData) != 0) AddFlag(String, "IsPlainOldData");
	if ((Flags & CPF_RepSkip) != 0) AddFlag(String, "RepSkip");
	if ((Flags & CPF_RepNotify) != 0) AddFlag(String, "RepNotify");
	if ((Flags & CPF_Interp) != 0) AddFlag(String, "Interp");
	if ((Flags & CPF_NonTransactional) != 0) AddFlag(String, "NonTransactional");
	if ((Flags & CPF_EditorOnly) != 0) AddFlag(String, "EditorOnly");
	if ((Flags & CPF_NoDestructor) != 0) AddFlag(String, "NoDestructor");
	if ((Flags & CPF_AutoWeak) != 0) AddFlag(String, "AutoWeak");
	if ((Flags & CPF_ContainsInstancedReference) != 0) AddFlag(String, "ContainsInstancedReference");
	if ((Flags & CPF_AssetRegistrySearchable) != 0) AddFlag(String, "AssetRegistrySearchable");
	if ((Flags & CPF_SimpleDisplay) != 0) AddFlag(String, "SimpleDisplay");
	if ((Flags & CPF_AdvancedDisplay) != 0) AddFlag(String, "AdvancedDisplay");
	if ((Flags & CPF_Protected) != 0) AddFlag(String, "Protected");
	if ((Flags & CPF_BlueprintCallable) != 0) AddFlag(String, "BlueprintCallable");
	if ((Flags & CPF_BlueprintAuthorityOnly) != 0) AddFlag(String, "BlueprintAuthorityOnly");
	if ((Flags & CPF_TextExportTransient) != 0) AddFlag(String, "TextExportTransient");
	if ((Flags & CPF_NonPIEDuplicateTransient) != 0) AddFlag(String, "NonPIEDuplicateTransient");
	if ((Flags & CPF_ExposeOnSpawn) != 0) AddFlag(String, "ExposeOnSpawn");
	if ((Flags & CPF_PersistentInstance) != 0) AddFlag(String, "PersistentInstance");
	if ((Flags & CPF_UObjectWrapper) != 0) AddFlag(String, "UObjectWrapper");
	if ((Flags & CPF_HasGetValueTypeHash) != 0) AddFlag(String, "HasGetValueTypeHash");
	if ((Flags & CPF_NativeAccessSpecifierPublic) != 0) AddFlag(String, "NativeAccessSpecifierPublic");
	if ((Flags & CPF_NativeAccessSpecifierProtected) != 0) AddFlag(String, "NativeAccessSpecifierProtected");
	if ((Flags & CPF_NativeAccessSpecifierPrivate) != 0) AddFlag(String, "NativeAccessSpecifierPrivate");
	if ((Flags & CPF_SkipSerialization) != 0) AddFlag(String, "SkipSerialization");
	// ReSharper restore StringLiteralTypo

	return String;
}

static void SetJsonNumberField(TSharedPtr<FJsonObject> Object, const FString Key, const int64 Value)
{
	auto ValueString = FString::Printf(TEXT("%lld"), Value);
	Object->SetField(Key, MakeShared<FJsonValueNumberString>(ValueString));
}

static TCHAR* EnumToString(EBuildModuleType::Type ModuleType)
{
	switch (ModuleType)
	{
	case EBuildModuleType::Program: return TEXT("Program");
	case EBuildModuleType::EngineRuntime: return TEXT("EngineRuntime");
	case EBuildModuleType::EngineUncooked: return TEXT("EngineUncooked");
	case EBuildModuleType::EngineDeveloper: return TEXT("EngineDeveloper");
	case EBuildModuleType::EngineEditor: return TEXT("EngineEditor");
	case EBuildModuleType::EngineThirdParty: return TEXT("EngineThirdParty");
	case EBuildModuleType::GameRuntime: return TEXT("GameRuntime");
	case EBuildModuleType::GameUncooked: return TEXT("GameUncooked");
	case EBuildModuleType::GameDeveloper: return TEXT("GameDeveloper");
	case EBuildModuleType::GameEditor: return TEXT("GameEditor");
	case EBuildModuleType::GameThirdParty: return TEXT("GameThirdParty");
	default: return TEXT("Unknown Module Type");
	}
}
