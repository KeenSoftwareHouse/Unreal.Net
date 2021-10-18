// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class UnknownErrorException : GenerationException
    {
        private readonly Exception m_exception;

        public UnknownErrorException(Exception ex)
            : base(ex.Message)
        {
            m_exception = ex;
        }

        public override bool IsFatal => true;

        public override Diagnostic CreateDiagnostic()
        {
            return Diagnostic.Create(Diagnostics.UnknownError, Location.None, m_exception.ToString());
        }
    }
}