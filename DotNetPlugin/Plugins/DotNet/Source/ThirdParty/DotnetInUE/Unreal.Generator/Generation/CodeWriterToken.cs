// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Generation
{
    public readonly struct CodeWriterToken : IDisposable
    {
        private readonly int m_stackIndex;
        private readonly CodeWriter m_writer;

        public CodeWriterToken(int stackIndex, CodeWriter writer)
        {
            m_stackIndex = stackIndex;
            m_writer = writer;
        }

        public void Dispose()
        {
            m_writer.PopContext(m_stackIndex);
        }
    }
}