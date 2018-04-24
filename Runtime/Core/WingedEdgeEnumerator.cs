using UnityEngine;
using System;
using System.Collections;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Enumerator for WingedEdge.
	/// Enumerates by walking the WingedEdge.next property.
	/// </summary>
	public class WingedEdgeEnumerator : IEnumerator
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
			if(m_Current == null)
			{
				m_Current = m_Start;
				return m_Current != null;
			}
			else
			{
				m_Current = m_Current.next;
		    	return m_Current != null && m_Current != m_Start;
			}
		}

		public void Reset()
		{
		    m_Current = null;
		}

		object IEnumerator.Current
		{
		    get
		    {
		        return Current;
		    }
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
	}
}
