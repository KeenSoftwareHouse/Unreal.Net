// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.ErrorHandling
{
    public class FatalGenerationException : Exception
    {
        public FatalGenerationException(GenerationException fatalException)
            : base(fatalException.Message, fatalException)
        { }
    }
}