using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    // <inheritdoc cref="IEnumerator"/> (can't inherit from class that isn't in the same package)
    /// <summary>
    /// Provides a way to enumerate a WingedEdge by walking the <see cref="WingedEdge.next">WingedEdge.next</see> property.
    /// </summary>
    public sealed class WingedEdgeEnumerator : IEnumerator<WingedEdge>
    {
        WingedEdge m_Start = null;
        WingedEdge m_Current = null;

        // <inheritdoc />
        /// <summary>
        /// Initializes the enumeration by specifying which WingedEdge object to start with.
        /// </summary>
        /// <param name="start">Specify which WingedEdge object to start walking from.</param>
        public WingedEdgeEnumerator(WingedEdge start)
        {
            m_Start = start;
            m_Current = null;
        }

        // <inheritdoc />
        /// <summary>
        /// Advances the enumerator to the next WingedEdge in the collection.
        /// </summary>
        /// <returns>True if the MoveNext succeeded; false if not (for example, if the enumerator passed the end of the collection).</returns>
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

        // <inheritdoc />
        /// <summary>
        /// Sets the enumerator to its initial position: before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            m_Current = null;
        }

        // <inheritdoc />
        /// <summary>
        /// Gets the WingedEdge in the collection at the current position of the enumerator.
        /// </summary>
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

        // <inheritdoc />
        object IEnumerator.Current
        {
            get { return Current; }
        }

        // <inheritdoc />
        /// <summary>
        /// Releases all resources used by the WingedEdgeEnumerator.
        /// </summary>
        public void Dispose() {}
    }
}
