// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Unreal.ErrorHandling
{
    public class IOGenerationException : GenerationException
    {
        private bool m_write;

        private readonly string m_file;

        public IOGenerationException(Exception e, string file, bool write)
            : base(e.Message)
        {
            m_file = file;
            m_write = write;
        }

        public override bool IsFatal => false;

        public override Diagnostic CreateDiagnostic()
        {
            var action = m_write ? Diagnostics.IOWriteError : Diagnostics.IOReadError;

            return Diagnostic.Create(action, Location.None, m_file, Message);
        }
    }
}