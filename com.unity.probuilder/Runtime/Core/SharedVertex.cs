using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Defines associations between vertex positions that are coincident. This is a list of the indexes that share a common position in model space.
	/// <br />
	/// <br />
	/// Coincident vertexes are vertexes that despite sharing the same coordinate position, are separate entries in the vertex array.
	/// </summary>
	[Serializable]
	public sealed class SharedVertex : ICollection<int>
	{
		/// <summary>
		/// An array of vertex indexes that are coincident.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("array")]
		int[] m_Indexes;

		internal int[] arrayInternal
		{
			get { return m_Indexes; }
		}

		/// <summary>
		/// Create a new SharedVertex from an int array.
		/// </summary>
		/// <param name="indexes">The array to copy.</param>
		public SharedVertex(IEnumerable<int> indexes)
		{
			if (indexes == null)
				throw new ArgumentNullException("indexes");
			m_Indexes = indexes.ToArray();
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="sharedVertex">The array to copy.</param>
		public SharedVertex(SharedVertex sharedVertex)
		{
			if (sharedVertex == null)
				throw new ArgumentNullException("sharedVertex");
			m_Indexes = new int[sharedVertex.Count];
			Array.Copy(sharedVertex.m_Indexes, m_Indexes, m_Indexes.Length);
		}

		/// <summary>
		/// Index accessor.
		/// </summary>
		/// <param name="i">The index to access.</param>
		public int this[int i]
		{
			get { return m_Indexes[i]; }
			set { m_Indexes[i] = value; }
		}

		public IEnumerator<int> GetEnumerator()
		{
			return ((IEnumerable<int>) m_Indexes).GetEnumerator();
		}

		public override string ToString()
		{
			return m_Indexes.ToString(",");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(int item)
		{
			m_Indexes = ArrayUtility.Add(m_Indexes, item);
		}

		public void Clear()
		{
			m_Indexes = new int[0];
		}

		public bool Contains(int item)
		{
			return Array.IndexOf(m_Indexes, item) > -1;
		}

		public void CopyTo(int[] array, int arrayIndex)
		{
			m_Indexes.CopyTo(array, arrayIndex);
		}

		public bool Remove(int item)
		{
			int ind = Array.IndexOf(m_Indexes, item);
			if (ind < 0)
				return false;
			m_Indexes = m_Indexes.RemoveAt(item);
			return true;
		}

		public int Count
		{
			get { return m_Indexes.Length; }
		}

		public bool IsReadOnly
		{
			get { return m_Indexes.IsReadOnly; }
		}

		/// <summary>
		/// Remove any arrays that are null or empty.
		/// </summary>
		/// <param name="array">The IntArray[] to scan for null or empty entries.</param>
		/// <returns>A new IntArray[] with no null or empty entries</returns>
		public static SharedVertex[] RemoveEmptyOrNull(SharedVertex[] array)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			List<SharedVertex> valid = new List<SharedVertex>();

			foreach (var par in array)
			{
				if (par != null && par.Any())
					valid.Add(par);
			}

			return valid.ToArray();
		}

		public static void GetSharedVertexLookup(IEnumerable<SharedVertex> sharedVertexes, Dictionary<int, int> lookup)
		{
			lookup.Clear();
			int commonIndex = 0;

			foreach (var common in sharedVertexes)
			{
				foreach (var index in common)
				{
					if(!lookup.ContainsKey(index))
						lookup.Add(index, commonIndex);
				}

				commonIndex++;
			}
		}

		public void ShiftIndexes(int offset)
		{
			for (int i = 0, c = Count; i < c; i++)
				m_Indexes[i] += offset;
		}
	}
}
