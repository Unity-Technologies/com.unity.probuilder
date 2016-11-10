using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Utility methods for working with UnityEngine.Selection.
	 */
	public static class pb_SelectionUtility
	{
		/**
		 * Remove a pb_Object (or it's GameObject) from the current Selection.
		 */
		public static void Remove(pb_Object pb)
		{
			if(pb == null)
				return;

			Selection.objects = Selection.objects.Where(x => !ObjectMatchesComponent(x, pb)).ToArray();
		}

		private static bool ObjectMatchesComponent(Object o, Component c)
		{
			if( o is GameObject )
			{
				GameObject g = o as GameObject;

				if(g == null)
					return false;

				return g.GetComponent(c.GetType()) == c;
			}
			else if(o is Component)
			{
				return o as Component == c;
			}
			else
			{
				return false;
			}
		}
	}
}
