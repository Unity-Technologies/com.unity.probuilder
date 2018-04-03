using UnityEngine;
using System;
using System.Collections;

namespace ProBuilder.Core
{
	/// <summary>
	/// Enumerator for pb_WingedEdge.
	/// </summary>
	public class pb_WingedEdgeEnumerator : IEnumerator
	{
		private pb_WingedEdge _start = null;
		private pb_WingedEdge _current = null;

		public pb_WingedEdgeEnumerator(pb_WingedEdge start)
		{
		 	_start = start;
		 	_current = null;
		}

		public bool MoveNext()
		{
			if(_current == null)
			{
				_current = _start;
				return _current != null;
			}
			else
			{
				_current = _current.next;
		    	return _current != null && _current != _start;
			}
		}

		public void Reset()
		{
		    _current = null;
		}

		object IEnumerator.Current
		{
		    get
		    {
		        return Current;
		    }
		}

		public pb_WingedEdge Current
		{
		    get
		    {
		        try
		        {
		            return _current;
		        }
		        catch (IndexOutOfRangeException)
		        {
		            throw new InvalidOperationException();
		        }
		    }
		}
	}
}
