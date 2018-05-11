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
	public static class IntArrayUtility
	{
		/// <summary>
		/// Returns a dictionary where Key is equal to triangle index, and Value is equal to the @"UnityEngine.ProBuilder.sharedIndexes" index.
		/// <br /><br />
		/// In this way you can quickly check which indices are sharing a vertex.
		/// </summary>
		/// <example>
		/// ```
		/// if(dictionary[triangles[0]] == dictionary[triangles[4]])
		///		Debug.Log("Triangles at mesh.triangles[0] and mesh.triangles[4] share a vertex");
		///	else
		///		Debug.Log("Triangles at mesh.triangles[0] and mesh.triangles[4] do not share a vertex");
		/// ```
		/// </example>
		/// <param name="array">The array to convert.</param>
		/// <returns>A dictionary that can be used to quickly look up the common index of a triangle.</returns>
		public static Dictionary<int, int> ToDictionary(this IEnumerable<IntArray> array)
		{
            if (array == null)
                throw new ArgumentNullException("array");

            Dictionary<int, int> dic = new Dictionary<int, int>();

			int commonIndex = 0;

			foreach (var common in array)
			{
				foreach (var index in common.array)
				{
					if(!dic.ContainsKey(index))
						dic.Add(index, commonIndex);
				}

				commonIndex++;
			}

			return dic;
		}

		/// <summary>
		/// Convert a lookup dictionary (<see cref="ToDictionary"/>) back to @"UnityEngine.ProBuilder.IntArray"[].
		/// </summary>
		/// <param name="lookup">A Dictionary where Key corresponds to a vertex index, and Value to a common index.</param>
		/// <returns>A new IntArray[] converted from the lookup dictionary.</returns>
		public static IntArray[] ToIntArray(this IEnumerable<KeyValuePair<int, int>> lookup)
		{
			if(lookup == null)
				return new IntArray[0];

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

			return shared.ToIntArray();
		}

		static IntArray[] ToIntArray(this List<List<int>> list)
		{
            if (list == null)
                throw new ArgumentNullException("list");
			IntArray[] arr = new IntArray[list.Count];
			for(int i = 0; i < arr.Length; i++)
				arr[i] = (IntArray)list[i].ToArray();
			return arr;
		}

		/// <summary>
		/// Find the index of a vertex index (triangle) in an IntArray[]. The index returned is called the common index, or shared index in some cases.
		/// </summary>
		/// <remarks>Aids in removing duplicate vertex indices.</remarks>
		/// <param name="intArray">The IntArray[] to search for a triangle value in.</param>
		/// <param name="index">The local index, or triangle to scan for. Local indices point correspond to the mesh vertices array.</param>
		/// <returns>The common (or shared) index.</returns>
		public static int IndexOf(this IntArray[] intArray, int index)
		{
			if(intArray == null) return -1;

			for(int i = 0; i < intArray.Length; i++)
			{
				// for some reason this is about 2x faster than System.Array.IndexOf
				for(int n = 0; n < intArray[i].length; n++)
					if(intArray[i][n] == index)
						return i;
			}
			return -1;
		}

		/// <summary>
		/// Given a list of vertex indexes (local), return all indices that are coincident.
		/// </summary>
		/// <param name="intArray">The shared index arrays. See @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes".</param>
		/// <param name="indexes">A collection of the vertex indices to include.</param>
		/// <returns>A comprehensive list of all indices that are coincident with any of the indices in the indexes argument.</returns>
		public static List<int> AllIndexesWithValues(this IList<IntArray> intArray, IEnumerable<int> indexes)
		{
            if (intArray == null)
                throw new ArgumentNullException("intArray");

            if (indexes == null)
                throw new ArgumentNullException("indexes");

			List<int> shared = new List<int>();

			foreach (var common in GetCommonIndices(intArray, indexes))
				shared.AddRange(intArray[common].array);

			return shared;
		}

		internal static List<int> AllIndexesWithValues(this IList<IntArray> intArray, Dictionary<int, int> lookup, IEnumerable<int> indexes)
		{
            if (intArray == null)
                throw new ArgumentNullException("intArray");

            int[] universal = GetCommonIndices(lookup, indexes).ToArray();

			List<int> shared = new List<int>();

			for(int i = 0; i < universal.Length; i++)
				shared.AddRange(intArray[universal[i]].array);

			return shared;
		}

		/// <summary>
		/// Given triangles, this returns a distinct array containing the first value of each sharedIndex array entry.
		/// </summary>
		/// <param name="pbIntArr"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static List<int> UniqueIndicesWithValues(this IntArray[] pbIntArr, IList<int> indices)
		{
			Dictionary<int, int> lookup = pbIntArr.ToDictionary();

			HashSet<int> shared = new HashSet<int>();

			foreach(int tri in indices)
				shared.Add(lookup[tri]);

			List<int> unique = new List<int>();

			foreach(int i in shared)
				unique.Add(pbIntArr[i][0]);

			return unique;
		}

		/// <summary>
		/// Given triangles, return a distinct list of the indices in the sharedIndices[] array (common index).
		/// </summary>
		/// <param name="array"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static HashSet<int> GetCommonIndices(this IEnumerable<IntArray> array, IEnumerable<int> indices)
		{
			return GetCommonIndices(array.ToDictionary(), indices);
		}

		internal static HashSet<int> GetCommonIndices(Dictionary<int, int> lookup, IEnumerable<int> indices)
		{
			HashSet<int> common = new HashSet<int>();

			foreach(int i in indices)
				common.Add( lookup[i] );

			return common;
		}

		/// <summary>
		/// Convert a list of common indices (indices of a group in the sharedIndices array) to actual triangle indices.
		/// Only returns the first index in a common index array.
		/// </summary>
		/// <param name="pbIntArr"></param>
		/// <param name="common"></param>
		/// <returns></returns>
		[System.Obsolete]
		internal static IEnumerable<int> GetIndicesWithCommon(this IntArray[] pbIntArr, IEnumerable<int> common)
		{
			return common.Select(x => pbIntArr[x][0]);
		}

		/// <summary>
		/// Cycles through a mesh and returns an IntArray[] of vertex indices that point to the same point in world space.
		/// <br />
		/// This is how many ProBuiilder shapes define coincident vertices on creation.
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
		/// <returns>A new IntArray[] where each contained array is a list of indices that are sharing the same position.</returns>
		public static IntArray[] GetSharedIndexesWithPositions(Vector3[] positions)
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

			IntArray[] share = new IntArray[sorted.Count];

			int t = 0;
			foreach(KeyValuePair<IntVec3, List<int>> kvp in sorted)
				share[t++] = new IntArray( kvp.Value.ToArray() );

			return share;
		}

		/// <summary>
		/// Associates all passed indices with a single shared index. Does not perfrom any additional operations to repair triangle structure or vertex placement.
		/// </summary>
		/// <param name="sharedIndices"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static int MergeSharedIndices(ref IntArray[] sharedIndices, int[] indices)
		{
			if(indices.Length < 2) return -1;
			if(sharedIndices == null)
			{
				sharedIndices = new IntArray[1] { (IntArray)indices };
				return 0;
			}

			List<int> used = new List<int>();
			List<int> newSharedIndex = new List<int>();

			// Create a new int[] composed of all indices in shared selection
			for(int i = 0; i < indices.Length; i++)
			{
				int si = sharedIndices.IndexOf(indices[i]);
				if(!used.Contains(si))
				{
					if( si > -1 )
					{
						newSharedIndex.AddRange( sharedIndices[si].array );
						used.Add(si);
					}
					else
					{
						newSharedIndex.Add( indices[i] );
					}

				}
			}

			// Now remove the old entries
			int rebuiltSharedIndexLength = sharedIndices.Length - used.Count;
			IntArray[] rebuild = new IntArray[rebuiltSharedIndexLength];

			int n = 0;
			for(int i = 0; i < sharedIndices.Length; i++)
			{
				if(!used.Contains(i))
					rebuild[n++] = sharedIndices[i];
			}

			sharedIndices = rebuild.Add( new IntArray(newSharedIndex.ToArray()) );
			// SetSharedIndices( rebuild.Add( new IntArray(newSharedIndex.ToArray()) ) );

			return sharedIndices.Length-1;
		}

		/// <summary>
		/// Associates indices with a single shared index.  Does not perfrom any additional operations to repair triangle structure or vertex placement.
		/// </summary>
		/// <param name="sharedIndices"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		internal static void MergeSharedIndices(ref IntArray[] sharedIndices, int a, int b)
		{
			int aIndex = sharedIndices.IndexOf(a);
			int oldBIndex = sharedIndices.IndexOf(b);

			IntArrayUtility.AddValueAtIndex(ref sharedIndices, aIndex, b);

			int[] arr = sharedIndices[oldBIndex].array;
			sharedIndices[oldBIndex].array = arr.RemoveAt(System.Array.IndexOf(arr, b));
			sharedIndices = IntArray.RemoveEmptyOrNull(sharedIndices);
		}

		/// <summary>
		/// Add a value to the array at index.
		/// </summary>
		/// <param name="sharedIndices"></param>
		/// <param name="sharedIndex"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		internal static int AddValueAtIndex(ref IntArray[] sharedIndices, int sharedIndex, int value)
		{
			if(sharedIndex > -1)
				sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.Add(value);
			else
				sharedIndices = (IntArray[])sharedIndices.Add( new IntArray(new int[]{value}) );

			return sharedIndex > -1 ? sharedIndex : sharedIndices.Length-1;
		}

		/// <summary>
		/// Adds a range of values to the array at index.
		/// </summary>
		/// <param name="sharedIndices"></param>
		/// <param name="sharedIndex"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static int AddRangeAtIndex(ref IntArray[] sharedIndices, int sharedIndex, int[] indices)
		{
			if(sharedIndex > -1)
				sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.AddRange(indices);
			else
				sharedIndices = (IntArray[])sharedIndices.Add( new IntArray(indices) );

			return sharedIndex > -1 ? sharedIndex : sharedIndices.Length-1;
		}

		/// <summary>
		/// Removes all passed values from the sharedIndices jagged array
		/// </summary>
		/// <remarks>
		/// Does NOT perform any index shifting to account for removed vertices. Use RemoveValuesAndShift for that purpose.
		/// </remarks>
		/// <param name="sharedIndices"></param>
		/// <param name="remove"></param>
		internal static void RemoveValues(ref IntArray[] sharedIndices, int[] remove)
		{
			// remove face indices from all shared indices caches
			for(int i = 0; i < sharedIndices.Length; i++)
			{
				for(int n = 0; n < remove.Length; n++)
				{
					int ind = System.Array.IndexOf(sharedIndices[i], remove[n]);

					if(ind > -1)
						sharedIndices[i].array = sharedIndices[i].array.RemoveAt(ind);
				}
			}

			// Remove empty or null entries caused by shifting around all them indices
			sharedIndices = IntArray.RemoveEmptyOrNull(sharedIndices);
		}

		/// <summary>
		/// Removes the specified indices from the array, and shifts all values down to account for removal in the vertex array.
		/// Only use when deleting faces or vertices.
		/// </summary>
		/// <remarks>For general moving around and modification of shared index array, use #RemoveValuesAtIndex.</remarks>
		/// <param name="sharedIndices"></param>
		/// <param name="indexesToRemove"></param>
		internal static void RemoveValuesAndShift(ref IntArray[] sharedIndices, IEnumerable<int> indexesToRemove)
		{
			if(sharedIndices == null)
				throw new ArgumentNullException("sharedIndices");

			if(indexesToRemove == null)
				throw new ArgumentNullException("indexesToRemove");

			Dictionary<int, int> lookup = sharedIndices.ToDictionary();

			foreach(int i in indexesToRemove)
				lookup[i] = -1;

			sharedIndices = ToIntArray(lookup.Where(x => x.Value > -1));

			List<int> removed_values = new List<int>(indexesToRemove);

			removed_values.Sort();

			for(int i = 0; i < sharedIndices.Length; i++)
			{
				for(int n = 0; n < sharedIndices[i].length; n++)
				{
					int index = ArrayUtility.NearestIndexPriorToValue(removed_values, sharedIndices[i][n]);
					// add 1 because index is zero based
					sharedIndices[i][n] -= index + 1;
				}
			}

		}
	}
}
