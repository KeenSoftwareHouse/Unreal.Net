// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Generation
{
    /// <summary>
    /// How to handle types and methods that cannot be generated because some symbol could not be resolved or exported.
    /// </summary>
    public enum MissingSymbolHandling
    {
        Error,
        Warning,
        Skip
    }
}