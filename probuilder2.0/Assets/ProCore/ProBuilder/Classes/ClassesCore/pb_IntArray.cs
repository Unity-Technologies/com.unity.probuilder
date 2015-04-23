/*	This exists because Unity can't
 *	serialize jaggaed arrays.  Also, it 
 *	has a couple of handy methods that make
 *	dealing with shared vertex indices easier.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Common {

[System.Serializable]
/**
 *	\brief Used as a substitute for a jagged int array.  
 *	Also contains some ProBuilder specific extensions for 
 *	dealing with jagged int arrays.  Note that this class
 *	exists because Unity does not serialize jagged arrays.
 */
public class pb_IntArray
{
#region Members

	public int[] array;
#endregion

#region Constructor / Operators

	public List<int> ToList()
	{
		return new List<int>(array);
	}

	public pb_IntArray(int[] intArray)
	{
		array = intArray;
	}

	// Copy constructor
	public pb_IntArray(pb_IntArray intArray)
	{
		array = intArray.array;
	}

	public int this[int i]
	{	
		get { return array[i]; }
		set { array[i] = value; }
	}

	public int Length
	{
		get { return array.Length; }
	}

	public static implicit operator int[](pb_IntArray intArr)
	{
		return intArr.array;
	}

	public static explicit operator pb_IntArray(int[] arr)
	{
		return new pb_IntArray(arr);
	}
#endregion

#region Override

	public override string ToString()
	{
		string str = "";
		for(int i = 0; i < array.Length - 1; i++)
			str += array[i] + ", ";

		if(array.Length > 0)
			str += array[array.Length-1];

		return str;
	}
	
	public bool IsEmpty()
	{
		return (array == null || array.Length < 1);
	}

	public static void RemoveEmptyOrNull(ref pb_IntArray[] val)
	{
		List<pb_IntArray> valid = new List<pb_IntArray>();
		foreach(pb_IntArray par in val)
		{
			if(par != null && !par.IsEmpty())
				valid.Add(par);
		}
		val = valid.ToArray();
	}
#endregion
}	

public static class pb_IntArrayUtility
{
	// Returns a jagged int array
	public static int[][] ToArray(this pb_IntArray[] val)
	{
		int[][] arr = new int[val.Length][];
		for(int i = 0; i < arr.Length; i++)
			arr[i] = val[i].array;
		return arr;
	}

	/**
	 * Returns a dictionary where Key is equal to sharedIndices index, and Value
	 * is equal to the triangle value.
	 */
	public static Dictionary<int, int> ToDictionary(this pb_IntArray[] array)
	{
		Dictionary<int, int> dic = new Dictionary<int, int>();

		for(int i = 0; i < array.Length; i++)
		{
			for(int n = 0; n < array[i].array.Length; n++)
			{
				dic.Add(array[i][n], i);
			}
		}

		return dic;
	}

	public static pb_IntArray[] ToSharedIndices(this IEnumerable<KeyValuePair<int, int>> lookup)
	{
		Dictionary<int, int> indexes = new Dictionary<int, int>();
		List<List<int>> shared = new List<List<int>>();

		foreach(KeyValuePair<int, int> pair in lookup)
		{
			if( indexes.ContainsKey(pair.Value) )
			{
				shared[indexes[pair.Value]].Add(pair.Key);
			}
			else
			{
				shared.Add( new List<int>() { pair.Key } );
				indexes.Add(pair.Value, shared.Count-1);
			}
		}

		return shared.ToPbIntArray();
	}

	/**
	 * Convert a jagged int array to a pb_IntArray.
	 */
	public static pb_IntArray[] ToPbIntArray(this int[][] val)
	{
		pb_IntArray[] arr = new pb_IntArray[val.Length];
		for(int i = 0; i < arr.Length; i++)
			arr[i] = (pb_IntArray)val[i];
		return arr;
	}

	public static pb_IntArray[] ToPbIntArray(this List<List<int>> val)
	{
		pb_IntArray[] arr = new pb_IntArray[val.Count];
		for(int i = 0; i < arr.Length; i++)
			arr[i] = (pb_IntArray)val[i].ToArray();
		return arr;
	}

	public static List<List<int>> ToList(this pb_IntArray[] val)
	{
		List<List<int>> l = new List<List<int>>();
		for(int i = 0; i < val.Length; i++)
			l.Add( val[i].ToList() );
		return l;
	}

	public static string ToFormattedString(this pb_IntArray[] arr)
	{
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < arr.Length; i++)
			sb.Append( "[" + arr[i].array.ToFormattedString(", ") + "] " );
		
		return sb.ToString();
	}

	/**
	 * Checks if an array contains a value and also compares shared indices using sharedIndices.
	 */
	public static int IndexOf(this int[] array, int val, pb_IntArray[] sharedIndices)
	{
		int indInShared = sharedIndices.IndexOf(val);
		if(indInShared < 0) return -1;

		int[] allValues = sharedIndices[indInShared];

		for(int i = 0; i < array.Length; i++)
			if(System.Array.IndexOf(allValues, array[i]) > -1)
				return i;

		return -1;
	}

	// Scans an array of pb_IntArray and returns the index of that int[] that holds the index.
	// Aids in removing duplicate vertex indices.
	public static int IndexOf(this pb_IntArray[] intArray, int index)
	{
		if(intArray == null) return -1;

		for(int i = 0; i < intArray.Length; i++)
		{
			// for some reason, this is about 2x faster than System.Array.IndexOf
			for(int n = 0; n < intArray[i].Length; n++)
				if(intArray[i][n] == index)
					return i;
		}
		return -1;
	}

	/**
	 * Returns all indices given a spattering of triangles.  Guaranteed to be all inclusive and 
	 * distinct.
	 */
	public static IList<int> AllIndicesWithValues(this pb_IntArray[] pbIntArr, IList<int> indices)
	{
		int[] universal = pbIntArr.GetUniversalIndices(indices).ToArray();

		List<int> shared = new List<int>();

		for(int i = 0; i < universal.Length; i++)
		{
			shared.AddRange(pbIntArr[universal[i]].array);
		}

		return shared;
	}

	public static IList<int> AllIndicesWithValues(this pb_IntArray[] pbIntArr, Dictionary<int, int> lookup, IList<int> indices)
	{
		int[] universal = GetUniversalIndices(lookup, indices).ToArray();

		List<int> shared = new List<int>();

		for(int i = 0; i < universal.Length; i++)
		{
			shared.AddRange(pbIntArr[universal[i]].array);
		}

		return shared;
	}

	/**
	 *	Given triangles, this returns a distinct array containing the first value of each sharedIndex array entry.
	 */
	public static IList<int> UniqueIndicesWithValues(this pb_IntArray[] pbIntArr, IList<int> indices)
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

	/**
	 * Given triangles, return a distinct list of the indices in the sharedIndices[] array (universal index).
	 */
	public static ICollection<int> GetUniversalIndices(this pb_IntArray[] pbIntArr, ICollection<int> indices)
	{
		Dictionary<int, int> lookup = pbIntArr.ToDictionary();
		HashSet<int> universal = new HashSet<int>();

		foreach(int i in indices)
		{
			universal.Add( lookup[i] );
		}

		return universal;
	}

	public static ICollection<int> GetUniversalIndices(Dictionary<int, int> lookup, ICollection<int> indices)
	{
		HashSet<int> universal = new HashSet<int>();

		foreach(int i in indices) {
			universal.Add( lookup[i] );
		}

		return universal;
	}

	/**
	 *	\brief Cycles through a mesh and returns a pb_IntArray[] of 
	 *	triangles that point to the same point in world space.
	 *	@param _mesh The mesh to examine.
	 *	\sa pb_IntArray
	 *	\notes pbIntArray exists because Unity cannot serialize jagged arrays.
	 *	\returns A pb_IntArray[] (basically just an int[][] with some added functionality).
	 */
	public static pb_IntArray[] ExtractSharedIndices(Vector3[] v)
	{
		int len = v.Length;
		bool[] assigned = pbUtil.FilledArray(false, len);

		List<pb_IntArray> shared = new List<pb_IntArray>();
		for(int i = 0; i < len-1; i++)
		{
			if(assigned[i])	// already assigned this vertex to a sharedIndex
				continue;

			List<int> indices = new List<int>(1) {i};
			for(int n = i+1; n < len; n++)
			{
				if( v[i] == v[n] )
				{
					indices.Add(n);
					assigned[n] = true;
				}
			}

			shared.Add(new pb_IntArray(indices.ToArray()));
		}

		if(!assigned[len-1])
			shared.Add(new pb_IntArray(new int[1]{len-1}));

		return shared.ToArray();
	}
#region ArrayUtil

	/**
	 *	Associates all passed indices with a single shared index.  Does not perfrom any additional operations 
	 *	to repair triangle structure or vertex placement.
	 */
	public static int MergeSharedIndices(ref pb_IntArray[] sharedIndices, int[] indices)
	{	
		if(indices.Length < 2) return -1;
		if(sharedIndices == null)
		{
			sharedIndices = new pb_IntArray[1] { (pb_IntArray)indices };
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
		pb_IntArray[] rebuild = new pb_IntArray[rebuiltSharedIndexLength];
		
		int n = 0;
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			if(!used.Contains(i))
				rebuild[n++] = sharedIndices[i];
		}

		sharedIndices = rebuild.Add( new pb_IntArray(newSharedIndex.ToArray()) );
		// SetSharedIndices( rebuild.Add( new pb_IntArray(newSharedIndex.ToArray()) ) );

		return sharedIndices.Length-1;
	}

	/**
	 *	Associates indices with a single shared index.  Does not perfrom any additional operations 
	 *	to repair triangle structure or vertex placement.
	 */
	public static void MergeSharedIndices(ref pb_IntArray[] sharedIndices, int a, int b)
	{
		int aIndex = sharedIndices.IndexOf(a);
		int oldBIndex = sharedIndices.IndexOf(b);
	
		pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, aIndex, b);

		int[] arr = sharedIndices[oldBIndex].array;
		sharedIndices[oldBIndex].array = arr.RemoveAt(System.Array.IndexOf(arr, b));
		pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
	}	

	/**
	 * Add a value to the array at index.
	 */
	public static int AddValueAtIndex(ref pb_IntArray[] sharedIndices, int sharedIndex, int value)
	{
		if(sharedIndex > -1)
			sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.Add(value);
		else
			sharedIndices = (pb_IntArray[])sharedIndices.Add( new pb_IntArray(new int[]{value}) );
		
		return sharedIndex > -1 ? sharedIndex : sharedIndices.Length-1;
	}

	/**
	 * Adds a range of values to the array at index.
	 */
	public static int AddRangeAtIndex(ref pb_IntArray[] sharedIndices, int sharedIndex, int[] indices)
	{
		if(sharedIndex > -1)
			sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.AddRange(indices);
		else
			sharedIndices = (pb_IntArray[])sharedIndices.Add( new pb_IntArray(indices) );
		
		return sharedIndex > -1 ? sharedIndex : sharedIndices.Length-1;
	}

	/**
	 * Removes all passed values from the sharedIndices jagged array. Does NOT perform any
	 * index shifting to account for removed vertices.  Use RemoveValuesAndShift
	 * for that purpose.
	 */
	public static void RemoveValues(ref pb_IntArray[] sharedIndices, int[] remove)
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
		pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
	}



	/**
	 *	\brief Removes the specified indices from the array, and shifts all values 
	 *	down to account for removal in the vertex array.  Only use when deleting
	 *	faces or vertices.  For general moving around and modification of shared 
	 *	index array, use #RemoveValuesAtIndex.
	 */
	public static void RemoveValuesAndShift(ref pb_IntArray[] sharedIndices, IList<int> remove)
	{
		Dictionary<int, int> lookup = sharedIndices.ToDictionary();

		for(int i = 0; i < remove.Count; i++)
			lookup[remove[i]] = -1;

		sharedIndices = lookup.Where(x => x.Value > -1).ToSharedIndices();

		List<int> removed_values = new List<int>(remove);

		removed_values.Sort();
	
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			for(int n = 0; n < sharedIndices[i].Length; n++)
			{
				int index = pbUtil.NearestIndexPriorToValue(removed_values, sharedIndices[i][n]);
				// add 1 because index is zero based
				sharedIndices[i][n] -= index+1;
			}
		}

	}
#endregion
}
}