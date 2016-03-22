using UnityEngine;
using System.Reflection;

namespace ProBuilder2.Common
{
	/**
	 *	Helper functions for working with Reflection.
	 */
	public static class pb_Reflection
	{
		const BindingFlags ALL_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		/**
		 *	Fetch a value using GetProperty or GetField.
		 */
		public static object GetValue(object target, string member, BindingFlags flags = ALL_FLAGS)
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

		public static bool SetValue(object target, string member, object value)
		{
			if(target == null)
				return false;

			PropertyInfo pi = target.GetType().GetProperty(member, ALL_FLAGS);

			if(pi != null)
				pi.SetValue(target, value, ALL_FLAGS, null, null, null);

			FieldInfo fi = target.GetType().GetField(member, ALL_FLAGS);

			if(fi != null)
				fi.SetValue(target, value);

			return pi != null || fi != null;
		}
	}
}
