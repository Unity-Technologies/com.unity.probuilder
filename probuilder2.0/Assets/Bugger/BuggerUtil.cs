using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Parabox.DebugUtil
{
	public static class BuggerUtil
	{
		/**
		 *	\brief Returns a string formatted by the passed seperator parameter.
		 *	\code{cs}
		 *	int[] myArray = new int[3]{0, 1, 2};
		 *
		 *	// Prints "0, 1, 2"
		 *	Debug.Log(myArray.ToFormattedString(", "));
		 *	@param _delimiter Inserts this string between entries.
		 *	\returns Formatted string.
		 */
		public static string ToFormattedString<T>(this T[] t, string _delimiter)
		{
			if(t == null || t.Length < 1)
				return "Empty Array.";

			StringBuilder str = new StringBuilder();

			str.Append(t[0].ToString());
			for(int i = 1; i < t.Length; i++) {
				str.Append(_delimiter + ((t[i] == null) ? "null" : t[i].ToString()));
			}
			return str.ToString();		
		}

		/**
		 *	\brief Returns a string formatted by the passed seperator parameter.
		 *	\code{cs}
		 *	List<int> myList = new List<int>(){0, 1, 2};
		 *
		 *	// Prints "0, 1, 2"
		 *	Debug.Log(myList.ToFormattedString(", "));
		 *	@param _delimiter Inserts this string between entries.
		 *	\returns Formatted string.
		 */
		public static string ToFormattedString<T>(this List<T> t, string _delimiter)
		{
			return t.ToArray().ToFormattedString(_delimiter);
		}
	}
}