using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Defines associations between vertex indices that are coincident.
	/// </summary>
	[System.Serializable]
	public class IntArray
	{
		/// <summary>
		/// An array of vertex indices that are coincident.
		/// </summary>
		public int[] array;

		/// <summary>
		/// Convert the array to a list.
		/// </summary>
		/// <returns></returns>
		internal List<int> ToList()
		{
			return new List<int>(array);
		}

		/// <summary>
		/// Create a new pb_IntArray from an array.
		/// </summary>
		/// <param name="intArray"></param>
		public IntArray(int[] intArray)
		{
			array = intArray;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="intArray"></param>
		public IntArray(IntArray intArray)
		{
			array = new int[intArray.length];
			System.Array.Copy(intArray.array, array, array.Length);
		}

		/// <summary>
		/// Indexer.
		/// </summary>
		/// <param name="i"></param>
		public int this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <summary>
		/// Returns the number of indices contained in this array.
		/// </summary>
		public int length
		{
			get { return array.Length; }
		}

		/// <summary>
		/// Implicit conversion from pb_IntArray to int[].
		/// </summary>
		/// <param name="intArr"></param>
		/// <returns></returns>
		public static implicit operator int[](IntArray intArr)
		{
			return intArr.array;
		}

		/// <summary>
		/// Implicit conversion from int[] to pb_IntArray.
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		public static explicit operator IntArray(int[] arr)
		{
			return new IntArray(arr);
		}

		public override string ToString()
		{
			return array.ToString(",");
		}

		/// <summary>
		/// Test if this array is null or contains no indices.
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty()
		{
			return (array == null || array.Length < 1);
		}

		/// <summary>
		/// Remove any arrays that are null or empty.
		/// </summary>
		/// <param name="val"></param>
		public static void RemoveEmptyOrNull(ref IntArray[] val)
		{
			List<IntArray> valid = new List<IntArray>();

			foreach(var par in val)
			{
				if(par != null && !par.IsEmpty())
					valid.Add(par);
			}

			val = valid.ToArray();
		}
	}
}
