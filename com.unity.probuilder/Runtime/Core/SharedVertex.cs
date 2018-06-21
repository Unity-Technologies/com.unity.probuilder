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
	/// Defines associations between vertex positions that are coincident. The indexes stored in this collection correspond to the ProBuilderMesh.positions array.
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
		int[] m_Vertexes;

		internal int[] arrayInternal
		{
			get { return m_Vertexes; }
		}

		/// <summary>
		/// Create a new SharedVertex from an int array.
		/// </summary>
		/// <param name="indexes">The array to copy.</param>
		public SharedVertex(IEnumerable<int> indexes)
		{
			if (indexes == null)
				throw new ArgumentNullException("indexes");
			m_Vertexes = indexes.ToArray();
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="sharedVertex">The array to copy.</param>
		public SharedVertex(SharedVertex sharedVertex)
		{
			if (sharedVertex == null)
				throw new ArgumentNullException("sharedVertex");
			m_Vertexes = new int[sharedVertex.Count];
			Array.Copy(sharedVertex.m_Vertexes, m_Vertexes, m_Vertexes.Length);
		}

		/// <summary>
		/// Index accessor.
		/// </summary>
		/// <param name="i">The index to access.</param>
		public int this[int i]
		{
			get { return m_Vertexes[i]; }
			set { m_Vertexes[i] = value; }
		}

		/// <inheritdoc />
		public IEnumerator<int> GetEnumerator()
		{
			return ((IEnumerable<int>) m_Vertexes).GetEnumerator();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return m_Vertexes.ToString(",");
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public void Add(int item)
		{
			m_Vertexes = ArrayUtility.Add(m_Vertexes, item);
		}

		/// <inheritdoc />
		public void Clear()
		{
			m_Vertexes = new int[0];
		}

		/// <inheritdoc />
		public bool Contains(int item)
		{
			return Array.IndexOf(m_Vertexes, item) > -1;
		}

		/// <inheritdoc />
		public void CopyTo(int[] array, int arrayIndex)
		{
			m_Vertexes.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc />
		public bool Remove(int item)
		{
			int ind = Array.IndexOf(m_Vertexes, item);
			if (ind < 0)
				return false;
			m_Vertexes = m_Vertexes.RemoveAt(item);
			return true;
		}

		/// <inheritdoc />
		public int Count
		{
			get { return m_Vertexes.Length; }
		}

		/// <inheritdoc />
		public bool IsReadOnly
		{
			get { return m_Vertexes.IsReadOnly; }
		}

		internal static void GetSharedVertexLookup(IEnumerable<SharedVertex> sharedVertexes, Dictionary<int, int> lookup)
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

		internal void ShiftIndexes(int offset)
		{
			for (int i = 0, c = Count; i < c; i++)
				m_Vertexes[i] += offset;
		}

		/// <summary>
		/// Convert a lookup dictionary (<see cref="SharedVertex.GetSharedVertexLookup"/>) back to <see cref="SharedVertex"/>[].
		/// </summary>
		/// <param name="lookup">A Dictionary where Key corresponds to a vertex index, and Value to a common index.</param>
		/// <returns>A new IntArray[] converted from the lookup dictionary.</returns>
		internal static SharedVertex[] ToSharedVertexes(IEnumerable<KeyValuePair<int, int>> lookup)
		{
			if(lookup == null)
				return new SharedVertex[0];

			Dictionary<int, int> map = new Dictionary<int, int>();
			List<List<int>> shared = new List<List<int>>();

			foreach(var kvp in lookup)
			{
				if(kvp.Value < 0)
				{
					shared.Add(new List<int>() { kvp.Key });
				}
				else
				{
					int index = -1;

					if(map.TryGetValue(kvp.Value, out index))
					{
						shared[index].Add(kvp.Key);
					}
					else
					{
						map.Add(kvp.Value, shared.Count);
						shared.Add(new List<int>() { kvp.Key });
					}
				}
			}

			return ToSharedVertexes(shared);
		}

		static SharedVertex[] ToSharedVertexes(List<List<int>> list)
		{
            if (list == null)
                throw new ArgumentNullException("list");
			SharedVertex[] arr = new SharedVertex[list.Count];
			for(int i = 0; i < arr.Length; i++)
				arr[i] = new SharedVertex(list[i]);
			return arr;
		}

		/// <summary>
		/// Create a new array of SharedVertex objects by comparing points in the passed positions collection.
		/// </summary>
		/// <example>
		/// ```
		/// <![CDATA[var mesh = gameObject.AdComponent<ProBuilderMesh>();]]>
		/// mesh.SetPositions(myNewPositions);
		/// mesh.SetFaces(myNewFaces);
		/// mesh.SetSharedIndexes(SharedVertex.GetSharedVertexesWithPositions(myNewPositions));
		/// ```
		/// </example>
		/// <param name="positions">A collection of Vector3 positions to be tested for equality.</param>
		/// <returns>A new SharedVertex[] where each SharedIndex is a list of indexes that are sharing the same position.</returns>
		public static SharedVertex[] GetSharedVertexesWithPositions(IList<Vector3> positions)
		{
            if (positions == null)
                throw new ArgumentNullException("positions");

			Dictionary<IntVec3, List<int>> sorted = new Dictionary<IntVec3, List<int>>();

			for(int i = 0; i < positions.Count; i++)
			{
				List<int> ind;
				if( sorted.TryGetValue(positions[i], out ind) )
					ind.Add(i);
				else
					sorted.Add(new IntVec3(positions[i]), new List<int>() { i });
			}

			SharedVertex[] share = new SharedVertex[sorted.Count];

			int t = 0;
			foreach(KeyValuePair<IntVec3, List<int>> kvp in sorted)
				share[t++] = new SharedVertex( kvp.Value.ToArray() );

			return share;
		}

		internal static SharedVertex[] RemoveAndShift(Dictionary<int, int> lookup, IEnumerable<int> remove)
		{
			var removedVertexes = new List<int>(remove);
			removedVertexes.Sort();
			return SortedRemoveAndShift(lookup, removedVertexes);
		}

		internal static SharedVertex[] SortedRemoveAndShift(Dictionary<int, int> lookup, List<int> remove)
		{
			foreach(int i in remove)
				lookup[i] = -1;

			var shared = ToSharedVertexes(lookup.Where(x => x.Value > -1));

			for(int i = 0, c = shared.Length; i < c; i++)
			{
				for(int n = 0, l = shared[i].Count; n < l; n++)
				{
					int index = ArrayUtility.NearestIndexPriorToValue(remove, shared[i][n]);
					// add 1 because index is zero based
					shared[i][n] -= index + 1;
				}
			}

			return shared;
		}

		internal static void SetCoincident(Dictionary<int, int> lookup, IEnumerable<int> vertexes)
		{
			int index = lookup.Count;
			foreach (var v in vertexes)
				lookup[v] = index;
		}
	}
}
