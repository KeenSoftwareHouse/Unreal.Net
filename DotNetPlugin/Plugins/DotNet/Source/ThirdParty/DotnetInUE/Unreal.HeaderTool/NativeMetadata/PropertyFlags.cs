// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.NativeMetadata
{
    [Flags]
    public enum PropertyFlags : ulong
    {
        None = 0,

        ///<Summary> Property is user-settable in the editor.</Summary>
        Edit = 0x0000000000000001,

        ///<Summary> This is a constant function parameter</Summary>
        ConstParm = 0x0000000000000002,

        ///<Summary> This property can be read by blueprint code</Summary>
        BlueprintVisible = 0x0000000000000004,

        ///<Summary> Object can be exported with actor.</Summary>
        ExportObject = 0x0000000000000008,

        ///<Summary> This property cannot be modified by blueprint code</Summary>
        BlueprintReadOnly = 0x0000000000000010,

        ///<Summary> Property is relevant to network replication.</Summary>
        Net = 0x0000000000000020,

        ///<Summary> Indicates that elements of an array can be modified, but its size cannot be changed.</Summary>
        EditFixedSize = 0x0000000000000040,

        ///<Summary> Function/When call parameter.</Summary>
        Parm = 0x0000000000000080,

        ///<Summary> Value is copied out after function call.</Summary>
        OutParm = 0x0000000000000100,

        ///<Summary> memset is fine for construction</Summary>
        ZeroConstructor = 0x0000000000000200,

        ///<Summary> Return value.</Summary>
        ReturnParm = 0x0000000000000400,

        ///<Summary> Disable editing of this property on an archetype/sub-blueprint</Summary>
        DisableEditOnTemplate = 0x0000000000000800,

        //      						= 0x0000000000001000,	

        ///<Summary> Property is transient: shouldn't be saved or loaded, except for Blueprint CDOs.</Summary>
        Transient = 0x0000000000002000,

        ///<Summary> Property should be loaded/saved as permanent profile.</Summary>
        Config = 0x0000000000004000,

        //								= 0x0000000000008000,	

        ///<Summary> Disable editing on an instance of this class</Summary>
        DisableEditOnInstance = 0x0000000000010000,

        ///<Summary> Property is uneditable in the editor.</Summary>
        EditConst = 0x0000000000020000,

        ///<Summary> Load config from base class, not subclass.</Summary>
        GlobalConfig = 0x0000000000040000,

        ///<Summary> Property is a component references.</Summary>
        InstancedReference = 0x0000000000080000,

        //								= 0x0000000000100000,	///<Summary></Summary>
        ///<Summary> Property should always be reset to the default value during any type of duplication (copy/paste, binary duplication, etc.)</Summary>
        DuplicateTransient = 0x0000000000200000,

        //								= 0x0000000000400000,	
        //    							= 0x0000000000800000,	
        ///<Summary> Property should be serialized for save games, this is only checked for game-specific archives with ArIsSaveGame</Summary>
        SaveGame = 0x0000000001000000,

        ///<Summary> Hide clear (and browse) button.</Summary>
        NoClear = 0x0000000002000000,

        //  							= 0x0000000004000000,	///<Summary></Summary>
        ///<Summary> Value is passed by reference; OutParam and Param should also be set.</Summary>
        ReferenceParm = 0x0000000008000000,

        ///<Summary> MC Delegates only.  Property should be exposed for assigning in blueprint code</Summary>
        BlueprintAssignable = 0x0000000010000000,

        ///<Summary> Property is deprecated.  Read it from an archive, but don't save it.</Summary>
        Deprecated = 0x0000000020000000,

        ///<Summary> If this is set, then the property can be memcopied instead of CopyCompleteValue / CopySingleValue</Summary>
        IsPlainOldData = 0x0000000040000000,

        ///<Summary> Not replicated. For non replicated properties in replicated structs </Summary>
        RepSkip = 0x0000000080000000,

        ///<Summary> Notify actors when a property is replicated</Summary>
        RepNotify = 0x0000000100000000,

        ///<Summary> interpolatable property for use with matinee</Summary>
        Interp = 0x0000000200000000,

        ///<Summary> Property isn't transacted</Summary>
        NonTransactional = 0x0000000400000000,

        ///<Summary> Property should only be loaded in the editor</Summary>
        EditorOnly = 0x0000000800000000,

        ///<Summary> No destructor</Summary>
        NoDestructor = 0x0000001000000000,

        //								= 0x0000002000000000,	///<Summary></Summary>
        ///<Summary> Only used for weak pointers, means the export type is autoweak</Summary>
        AutoWeak = 0x0000004000000000,

        ///<Summary> Property contains component references.</Summary>
        ContainsInstancedReference = 0x0000008000000000,

        ///<Summary> asset instances will add properties with this flag to the asset registry automatically</Summary>
        AssetRegistrySearchable = 0x0000010000000000,

        ///<Summary> The property is visible by default in the editor details view</Summary>
        SimpleDisplay = 0x0000020000000000,

        ///<Summary> The property is advanced and not visible by default in the editor details view</Summary>
        AdvancedDisplay = 0x0000040000000000,

        ///<Summary> property is protected from the perspective of script</Summary>
        Protected = 0x0000080000000000,

        ///<Summary> MC Delegates only.  Property should be exposed for calling in blueprint code</Summary>
        BlueprintCallable = 0x0000100000000000,

        ///<Summary> MC Delegates only.  This delegate accepts (only in blueprint) only events with BlueprintAuthorityOnly.</Summary>
        BlueprintAuthorityOnly = 0x0000200000000000,

        ///<Summary> Property shouldn't be exported to text format (e.g. copy/paste)</Summary>
        TextExportTransient = 0x0000400000000000,

        ///<Summary> Property should only be copied in PIE</Summary>
        NonPIEDuplicateTransient = 0x0000800000000000,

        ///<Summary> Property is exposed on spawn</Summary>
        ExposeOnSpawn = 0x0001000000000000,

        ///<Summary> A object referenced by the property is duplicated like a component. (Each actor should have an own instance.)</Summary>
        PersistentInstance = 0x0002000000000000,

        ///<Summary> Property was parsed as a wrapper class like TSubclassOf<T>, FScriptInterface etc., rather than a USomething*</Summary>
        UObjectWrapper = 0x0004000000000000,

        ///<Summary> This property can generate a meaningful hash value.</Summary>
        HasGetValueTypeHash = 0x0008000000000000,

        ///<Summary> Public native access specifier</Summary>
        NativeAccessSpecifierPublic = 0x0010000000000000,

        ///<Summary> Protected native access specifier</Summary>
        NativeAccessSpecifierProtected = 0x0020000000000000,

        ///<Summary> Private native access specifier</Summary>
        NativeAccessSpecifierPrivate = 0x0040000000000000,

        ///<Summary> Property shouldn't be serialized, can still be exported to text</Summary>
        SkipSerialization = 0x0080000000000000,
    }
}