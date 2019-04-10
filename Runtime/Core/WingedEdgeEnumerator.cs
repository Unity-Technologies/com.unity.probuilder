using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Enumerator for WingedEdge.
    /// Enumerates by walking the WingedEdge.next property.
    /// </summary>
    public sealed class WingedEdgeEnumerator : IEnumerator<WingedEdge>
    {
        WingedEdge m_Start = null;
        WingedEdge m_Current = null;

        /// <inheritdoc />
        public WingedEdgeEnumerator(WingedEdge start)
        {
            m_Start = start;
            m_Current = null;
        }

        /// <summary>
        /// Move the current value to the next WingedEdge.
        /// </summary>
        /// <returns>True if next is valid, false if not.</returns>
        /// <inheritdoc />
        public bool MoveNext()
        {
            if (ReferenceEquals(m_Current, null))
            {
                m_Current = m_Start;
                return !ReferenceEquals(m_Current, null);
            }

            m_Current = m_Current.next;

            return !ReferenceEquals(m_Current, null) && !ReferenceEquals(m_Current, m_Start);
        }

        /// <inheritdoc />
        public void Reset()
        {
            m_Current = null;
        }

        /// <inheritdoc />
        public WingedEdge Current
        {
            get
            {
                try
                {
                    return m_Current;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <inheritdoc />
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <inheritdoc />
        public void Dispose() {}
    }
}
