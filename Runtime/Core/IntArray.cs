using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Defines associations between vertex indices that are coincident.
	/// </summary>
	[Serializable]
	public class IntArray : IEnumerable<int>
	{
		/// <summary>
		/// An array of vertex indices that are coincident.
		/// </summary>
		[SerializeField]
		internal int[] array;

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
            if (intArray == null)
                throw new ArgumentNullException("intArray");
            int len = intArray.Length;
            array = new int[len];
            Array.Copy(intArray, array, len);
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="intArray"></param>
		public IntArray(IntArray intArray)
		{
            if (intArray == null)
                throw new ArgumentNullException("intArray");
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
			return intArr != null ? intArr.array : null;
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

		public IEnumerator<int> GetEnumerator()
		{
			return (IEnumerator<int>) array.GetEnumerator();
		}

		public override string ToString()
		{
			return array.ToString(",");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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
		/// <param name="array"></param>
		public static IntArray[] RemoveEmptyOrNull(IntArray[] array)
		{
            if (array == null)
                throw new ArgumentNullException("array");

			List<IntArray> valid = new List<IntArray>();

			foreach(var par in array)
			{
				if(par != null && !par.IsEmpty())
					valid.Add(par);
			}

            return valid.ToArray();
		}
	}
}
