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

//		/// <summary>
//		/// Given a list of vertex indexes (local), return all indexes that are coincident.
//		/// </summary>
//		/// <param name="intArray">The shared index arrays. See @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes".</param>
//		/// <param name="indexes">A collection of the vertex indexes to include.</param>
//		/// <returns>A comprehensive list of all indexesindexesindexesindexes that are coincident with any of the indexes in the indexes argument.</returns>
//		public static List<int> AllIndexesWithValues(this IList<SharedVertex> intArray, IEnumerable<int> indexes)
//		{
//            if (intArray == null)
//                throw new ArgumentNullException("intArray");
//
//            if (indexes == null)
//                throw new ArgumentNullException("indexes");
//
//			List<int> shared = new List<int>();
//
//			foreach (var common in GetCommonIndexes(intArray, indexes))
//				shared.AddRange(intArray[common].array);
//
//			return shared;
//		}
//
//		internal static List<int> AllIndexesWithValues(this IList<SharedVertex> intArray, Dictionary<int, int> lookup, IEnumerable<int> indexes)
//		{
//            if (intArray == null)
//                throw new ArgumentNullException("intArray");
//
//            int[] universal = GetCommonIndexes(lookup, indexes).ToArray();
//
//			List<int> shared = new List<int>();
//
//			for(int i = 0; i < universal.Length; i++)
//				shared.AddRange(intArray[universal[i]].array);
//
//			return shared;
//		}
//
//		/// <summary>
//		/// Given triangles, return a distinct list of the indexes in the shared indexes array (common index).
//		/// </summary>
//		/// <param name="array"></param>
//		/// <param name="indexes"></param>
//		/// <returns></returns>
//		internal static HashSet<int> GetCommonIndexes(this IEnumerable<SharedVertex> array, IEnumerable<int> indexes)
//		{
//			return GetCommonIndexes(array.ToDictionary(), indexes);
//		}
//
//		internal static HashSet<int> GetCommonIndexes(Dictionary<int, int> lookup, IEnumerable<int> indexes)
//		{
//			HashSet<int> common = new HashSet<int>();
//
//			foreach(int i in indexes)
//				common.Add( lookup[i] );
//
//			return common;
//		}

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
//
//		/// <summary>
//		/// Associates all passed indexes with a single shared index. Does not perfrom any additional operations to repair triangle structure or vertex placement.
//		/// </summary>
//		/// <param name="sharedIndexes"></param>
//		/// <param name="indexes"></param>
//		/// <returns></returns>
//		internal static int MergeSharedIndexes(ref SharedVertex[] sharedIndexes, int[] indexes)
//		{
//			if(indexes.Length < 2) return -1;
//			if(sharedIndexes == null)
//			{
//				sharedIndexes = new SharedVertex[1] { new SharedVertex(indexes) };
//				return 0;
//			}
//
//			List<int> used = new List<int>();
//			List<int> newSharedIndex = new List<int>();
//
//			// Create a new int[] composed of all indexes in shared selection
//			for(int i = 0; i < indexes.Length; i++)
//			{
//				int si = sharedIndexes.IndexOf(indexes[i]);
//
//				if(!used.Contains(si))
//				{
//					if( si > -1 )
//					{
//						newSharedIndex.AddRange( sharedIndexes[si].array );
//						used.Add(si);
//					}
//					else
//					{
//						newSharedIndex.Add( indexes[i] );
//					}
//
//				}
//			}
//
//			// Now remove the old entries
//			int rebuiltSharedIndexLength = sharedIndexes.Length - used.Count;
//			SharedVertex[] rebuild = new SharedVertex[rebuiltSharedIndexLength];
//
//			int n = 0;
//			for(int i = 0; i < sharedIndexes.Length; i++)
//			{
//				if(!used.Contains(i))
//					rebuild[n++] = sharedIndexes[i];
//			}
//
//			sharedIndexes = rebuild.Add( new SharedVertex(newSharedIndex.ToArray()) );
//
//			return sharedIndexes.Length-1;
//		}
//
//		/// <summary>
//		/// Associates indexes with a single shared index.  Does not perfrom any additional operations to repair triangle structure or vertex placement.
//		/// </summary>
//		/// <param name="sharedIndexes"></param>
//		/// <param name="a"></param>
//		/// <param name="b"></param>
//		internal static void MergeSharedIndexes(ref SharedVertex[] sharedIndexes, int a, int b)
//		{
//			int aIndex = sharedIndexes.IndexOf(a);
//			int oldBIndex = sharedIndexes.IndexOf(b);
//
//			SharedVertexesUtility.AddValueAtIndex(ref sharedIndexes, aIndex, b);
//
//			int[] arr = sharedIndexes[oldBIndex].array;
//			sharedIndexes[oldBIndex].array = arr.RemoveAt(System.Array.IndexOf(arr, b));
//			sharedIndexes = SharedVertex.RemoveEmptyOrNull(sharedIndexes);
//		}
//
//		/// <summary>
//		/// Add a value to the array at index.
//		/// </summary>
//		/// <param name="sharedIndexes"></param>
//		/// <param name="sharedIndex"></param>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		internal static int AddValueAtIndex(ref SharedVertex[] sharedIndexes, int sharedIndex, int value)
//		{
//			if(sharedIndex > -1)
//				sharedIndexes[sharedIndex].array = sharedIndexes[sharedIndex].array.Add(value);
//			else
//				sharedIndexes = (SharedVertex[])sharedIndexes.Add( new SharedVertex(new int[]{value}) );
//
//			return sharedIndex > -1 ? sharedIndex : sharedIndexes.Length-1;
//		}
//
//		/// <summary>
//		/// Removes the specified indexes from the array, and shifts all values down to account for removal in the vertex array.
//		/// Only use when deleting faces or vertexes.
//		/// </summary>
//		/// <remarks>For general moving around and modification of shared index array, use #RemoveValuesAtIndex.</remarks>
//		/// <param name="sharedIndexes"></param>
//		/// <param name="indexesToRemove"></param>
//		internal static void RemoveValuesAndShift(ref SharedVertex[] sharedIndexes, IEnumerable<int> indexesToRemove)
//		{
//			if(sharedIndexes == null)
//				throw new ArgumentNullException("sharedIndexes");
//
//			if(indexesToRemove == null)
//				throw new ArgumentNullException("indexesToRemove");
//
//			Dictionary<int, int> lookup = sharedIndexes.ToDictionary();
//
//			foreach(int i in indexesToRemove)
//				lookup[i] = -1;
//
//			sharedIndexes = ToSharedVertexes(lookup.Where(x => x.Value > -1));
//
//			List<int> removed_values = new List<int>(indexesToRemove);
//
//			removed_values.Sort();
//
//			for(int i = 0; i < sharedIndexes.Length; i++)
//			{
//				for(int n = 0; n < sharedIndexes[i].length; n++)
//				{
//					int index = ArrayUtility.NearestIndexPriorToValue(removed_values, sharedIndexes[i][n]);
//					// add 1 because index is zero based
//					sharedIndexes[i][n] -= index + 1;
//				}
//			}
//		}
	}
}
