using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Utilities and extension methods for working with @"UnityEngine.ProBuilder.IntArray".
	/// </summary>
	public static class SharedVertexesUtility
	{
		/// <summary>
		/// Convert a lookup dictionary (<see cref="SharedVertex.GetSharedVertexLookup"/>) back to <see cref="SharedVertex"/>[].
		/// </summary>
		/// <param name="lookup">A Dictionary where Key corresponds to a vertex index, and Value to a common index.</param>
		/// <returns>A new IntArray[] converted from the lookup dictionary.</returns>
		internal static SharedVertex[] ToSharedVertexes(this IEnumerable<KeyValuePair<int, int>> lookup)
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

			return shared.ToSharedVertexes();
		}

		static SharedVertex[] ToSharedVertexes(this List<List<int>> list)
		{
            if (list == null)
                throw new ArgumentNullException("list");
			SharedVertex[] arr = new SharedVertex[list.Count];
			for(int i = 0; i < arr.Length; i++)
				arr[i] = new SharedVertex(list[i]);
			return arr;
		}

		/// <summary>
		/// Cycles through a mesh and returns an IntArray[] of vertex indexes that point to the same point in world space.
		/// <br />
		/// This is how many ProBuiilder shapes define coincident vertexes on creation.
		/// </summary>
		/// <example>
		/// ```
		/// <![CDATA[var mesh = gameObject.AdComponent<ProBuilderMesh>();]]>
		/// mesh.SetPositions(myNewPositions);
		/// mesh.SetFaces(myNewFaces);
		/// mesh.SetSharedIndexes(IntArrayUtility.GetSharedIndexesWithPositions(myNewPositions));
		/// ```
		/// </example>
		/// <param name="positions">A collection of Vector3 positions to be tested for equality.</param>
		/// <returns>A new IntArray[] where each contained array is a list of indexes that are sharing the same position.</returns>
		public static SharedVertex[] GetSharedIndexesWithPositions(Vector3[] positions)
		{
            if (positions == null)
                throw new ArgumentNullException("positions");

			Dictionary<IntVec3, List<int>> sorted = new Dictionary<IntVec3, List<int>>();

			List<int> ind;

			for(int i = 0; i < positions.Length; i++)
			{
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
	}
}
