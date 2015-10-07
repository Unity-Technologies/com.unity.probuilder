using UnityEngine;
using System.Reflection;

namespace ProBuilder2.Common
{
	/**
	 *	Helper functions for working with Reflection.
	 */
	public static class pb_Reflection
	{
		/**
		 *	Fetch a value using GetProperty or GetField.
		 */
		public static object GetValue(object target, string member, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
		{
			if(target == null)
				return null;

			PropertyInfo pi = target.GetType().GetProperty(member, flags);
				
			if(pi != null)
				return pi.GetValue(target, null);

			FieldInfo fi = target.GetType().GetField(member, flags);
				
			if(fi != null)
				return fi.GetValue(target);

			return null;
		}
	}
}
