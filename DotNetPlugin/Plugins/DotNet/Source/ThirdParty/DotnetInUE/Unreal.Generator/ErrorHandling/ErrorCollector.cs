// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Unreal.ErrorHandling
{
    public struct ErrorCollector
    {
        private List<GenerationException> m_exceptions;

        public void Add(GenerationException ex)
        {
            m_exceptions ??= new();
            m_exceptions.Add(ex);

            // Throw all exceptions so far.
            if (ex.IsFatal)
                ThrowIfNeeded();
        }

        public void ThrowIfNeeded()
        {
            if (m_exceptions != null)
                throw new AggregateGenerationException(m_exceptions);
        }
    }
}