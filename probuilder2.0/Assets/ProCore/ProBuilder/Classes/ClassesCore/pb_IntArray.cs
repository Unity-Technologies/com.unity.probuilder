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

namespace ProBuilder2.Common
{
	/**
	 *	\brief Used as a substitute for a jagged int array.  
	 *	Also contains some ProBuilder specific extensions for 
	 *	dealing with jagged int arrays.  Note that this class
	 *	exists because Unity does not serialize jagged arrays.
	 */
	[System.Serializable]
	public class pb_IntArray
	{
		public int[] array;

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
	}	
}
