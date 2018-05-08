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

		public WingedEdgeEnumerator(WingedEdge start)
		{
			m_Start = start;
			m_Current = null;
		}

		public bool MoveNext()
		{
			if (m_Current == null)
			{
				m_Current = m_Start;
				return m_Current != null;
			}

			m_Current = m_Current.next;

			return ReferenceEquals(m_Current, m_Start);
		}

		public void Reset()
		{
			m_Current = null;
		}

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

		object IEnumerator.Current
		{
			get { return Current; }
		}

		public void Dispose() { }
	}
}