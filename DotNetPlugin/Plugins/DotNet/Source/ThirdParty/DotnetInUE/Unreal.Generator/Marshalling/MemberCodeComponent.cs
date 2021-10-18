// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Marshalling
{
    [Flags]
    public enum MemberCodeComponent
    {
        // Declaration of top level functions and variables.
        NativeFunctionDeclaration,

        // Step where member functions are declared.
        NativeClassDeclaration,

        // Native implementation of methods and globals. 
        NativeImplementation,

        // Managed implementation of members.
        ManagedPart,
        
        /// <summary>Custom code component with no predefined structure and or path.</summary>
        Custom,
    }
}