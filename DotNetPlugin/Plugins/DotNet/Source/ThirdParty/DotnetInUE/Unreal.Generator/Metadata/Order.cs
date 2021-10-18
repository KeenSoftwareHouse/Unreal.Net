// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Metadata
{
    /// <summary>
    /// Represents the order of execution with respect to another object.
    /// </summary>
    public enum Order
    {
        Before,
        After
    }
    
    /// <summary>
    /// Represents the order of execution with respect to another object.
    /// </summary>
    public enum MarshalOrder
    {
        Before,
        Marshalled,
        After
    }
}