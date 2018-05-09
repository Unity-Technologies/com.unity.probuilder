using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace UnityEngine.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Defines associations between vertex indices that are coincident.
	/// <br />
	/// <br />
	/// Coincident vertices are vertices that despite sharing the same coordinate position, are separate entries in the vertex array.
	/// </summary>
	[Serializable]
	public sealed class IntArray : IEnumerable<int>
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
		/// Create a new IntArray from an int array.
		/// </summary>
		/// <param name="intArray">The array to copy.</param>
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
		/// <param name="intArray">The array to copy.</param>
		public IntArray(IntArray intArray)
		{
            if (intArray == null)
                throw new ArgumentNullException("intArray");
			array = new int[intArray.length];
			System.Array.Copy(intArray.array, array, array.Length);
		}

		/// <summary>
		/// Index accessor.
		/// </summary>
		/// <param name="i">The index to access.</param>
		public int this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <value>
		/// Returns the number of values contained in this array.
		/// </value>
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
		/// Explicit conversion from int[] to pb_IntArray.
		/// </summary>
		/// <param name="array"></param>
		/// <returns>A new IntArray copy off array.</returns>
		public static explicit operator IntArray(int[] array)
		{
			return new IntArray(array);
		}

		public IEnumerator<int> GetEnumerator()
		{
			return (IEnumerator<int>) array.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return array.ToString(",");
		}

		/// <summary>
		/// Test if this array is null or empty.
		/// </summary>
		/// <returns>True if the array is null or empty, false otherwise.</returns>
		public bool IsEmpty()
		{
			return (array == null || array.Length < 1);
		}

		/// <summary>
		/// Remove any arrays that are null or empty.
		/// </summary>
		/// <param name="array">The IntArray[] to scan for null or empty entries.</param>
		/// <returns>A new IntArray[] with no null or empty entries</returns>
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
