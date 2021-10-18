// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Marshalling
{
    [Flags]
    public enum MemberCodeComponentFlags
    {
        // Declaration of top level functions and variables.
        NativeFunctionDeclaration = 1 << MemberCodeComponent.NativeFunctionDeclaration,

        // Step where member functions are declared.
        NativeClassDeclaration = 1 << MemberCodeComponent.NativeClassDeclaration,

        // Native implementation of methods and globals. 
        NativeImplementation = 1 << MemberCodeComponent.NativeImplementation,

        /// Managed implementation of members.
        ManagedPart = 1 << MemberCodeComponent.ManagedPart,
        
        /// <summary>Custom code component not abiding to the rules above.</summary>
        Custom = 1 << MemberCodeComponent.Custom,

        None = 0,
        All = 0b1111,
    }
}