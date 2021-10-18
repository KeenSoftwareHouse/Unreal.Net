// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Unreal.Generation
{
    public sealed class CodeWriter : IDisposable
    {
        public const string NativeIndent = "\t";
        public const string ManagedIndent = "    ";

        private readonly TextWriter m_writer;

        private readonly StringBuilder m_indent = new();
        private readonly List<int> m_indentLengths = new();

        private readonly string m_defaultIndent;

        private readonly Stack<TrailInfo> m_userTrails = new();

        private readonly Dictionary<string, object> m_userVariables = new();

        /// <summary>
        /// Whether the current writer stands at a line ending. 
        /// </summary>
        private bool m_atLineEnd = true;

        public CodeWriter(TextWriter writer, string defaultIndent = "  ")
        {
            m_writer = writer;
            m_defaultIndent = defaultIndent;
        }

        public void Write(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (m_atLineEnd)
                m_writer.Write(m_indent);

            m_atLineEnd = text.EndsWith("\n", StringComparison.Ordinal);
            if (m_indent.Length == 0)
            {
                m_writer.Write(text);
            }
            else
            {
                var start = 0;
                do
                {
                    var nl = text.IndexOf("\n", start, StringComparison.Ordinal);

                    var end = nl == -1 ? text.Length : nl + 1;

                    if (start > 0)
                        m_writer.Write(m_indent);
                    m_writer.Write(text.Substring(start, end - start));

                    start = end;
                } while (start < text.Length);
            }
        }

        public void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        public void WriteLine()
        {
            m_writer.WriteLine();
            m_atLineEnd = true;
        }

        #region Variables

        public ref T GetVariable<T>(string name)
        {
            if (!m_userVariables.TryGetValue(name, out var value))
                m_userVariables[name] = value = new StrongBox<T>();

            var box = (StrongBox<T>) value;
            return ref box.Value!;
        }

        #endregion

        #region Indent

        /// <summary>Increase the indent</summary>
        /// <param name="indent">indent string</param>
        public void PushIndent(string? indent = null)
        {
            indent ??= m_defaultIndent;

            m_indent.Append(indent);
            m_indentLengths.Add(indent.Length);
        }

        /// <summary>Remove the last indent that was added with PushIndent</summary>
        /// <returns>The removed indent string</returns>
        public void PopIndent()
        {
            if (m_indentLengths.Count > 0)
            {
                int indentLength = PopIndentLength();

                if (indentLength > 0)
                    m_indent.Remove(m_indent.Length - indentLength, indentLength);
            }
        }

        private int PopIndentLength()
        {
            var lastIndent = m_indentLengths.Count - 1;
            int indentLength = m_indentLengths[lastIndent];

            m_indentLengths.RemoveAt(lastIndent);

            return indentLength;
        }

        /// <summary>Remove the last indent that was added with PushIndent</summary>
        /// <returns>The removed indent string</returns>
        public void PopIndent(out string indent)
        {
            if (m_indentLengths.Count > 0)
            {
                int indentLength = PopIndentLength();

                // Collect removed indent.
                indent = m_indent.ToString(m_indent.Length - indentLength, indentLength);

                if (indentLength > 0)
                    m_indent.Remove(m_indent.Length - indentLength, indentLength);
            }
            else
            {
                indent = string.Empty;
            }
        }

        /// <summary>Remove any indentation</summary>
        public void ClearIndent()
        {
            m_indent.Clear();
            m_indentLengths.Clear();
        }

        #endregion

        #region UserContext

        private readonly struct TrailInfo
        {
            private readonly TrailAction m_contextAction;

            private readonly object? m_argument;

            public TrailInfo(TrailAction contextAction, object? argument)
            {
                m_argument = argument;
                m_contextAction = contextAction;
            }

            public void Dispatch(CodeWriter writer)
            {
                switch (m_contextAction)
                {
                    case TrailAction.PushIndent:
                        writer.PushIndent((string) m_argument!);
                        break;
                    case TrailAction.PopIndent:
                        writer.PopIndent();
                        break;
                    case TrailAction.Action:
                        ((Action<CodeWriter>) m_argument!)(writer);
                        break;
                    case TrailAction.Write:
                        writer.Write((string) m_argument!);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private enum TrailAction
        {
            PushIndent,
            PopIndent,
            Action,
            Write
        }

        /// <summary>
        /// Get a trail token for the current state.
        /// </summary>
        /// <remarks>this can be used to allow a child call to push trailing actions without popping them.</remarks>
        /// <returns></returns>
        public CodeWriterToken GetCurrentToken() => new(m_userTrails.Count, this);

        /// <summary>
        /// Get the current top index for the trail stack.
        /// </summary>
        /// <remarks>Advanced usage only.</remarks>
        /// <returns></returns>
        public int GetCurrentTrailIndex() => m_userTrails.Count - 1;

        public CodeWriterToken PushIndentTrail(string indent)
        {
            m_userTrails.Push(new TrailInfo(TrailAction.PushIndent, indent));
            return new CodeWriterToken(m_userTrails.Count - 1, this);
        }

        public CodeWriterToken PushUnIndentTrail()
        {
            m_userTrails.Push(new TrailInfo(TrailAction.PopIndent, null));
            return new CodeWriterToken(m_userTrails.Count - 1, this);
        }

        public CodeWriterToken PushActionTrail(Action<CodeWriter> action)
        {
            m_userTrails.Push(new TrailInfo(TrailAction.Action, action));
            return new CodeWriterToken(m_userTrails.Count - 1, this);
        }

        public CodeWriterToken PushTextTrail(string text)
        {
            m_userTrails.Push(new TrailInfo(TrailAction.Write, text));
            return new CodeWriterToken(m_userTrails.Count - 1, this);
        }

        public void PopContext(int index)
        {
            while (m_userTrails.Count > index)
            {
                var ctx = m_userTrails.Pop();
                ctx.Dispatch(this);
            }
        }

        #endregion

        public void Clear()
        {
            ClearIndent();
            m_userVariables.Clear();
            m_writer.Flush();
        }

        public void Flush()
        {
            m_writer.Flush();
        }

        public void Dispose()
        {
            m_writer.Dispose();
        }
    }
}