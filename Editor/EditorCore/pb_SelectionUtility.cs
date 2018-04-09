using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Utility methods for working with UnityEngine.Selection.
	/// </summary>
	static class pb_SelectionUtility
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
				GameObject g = (GameObject) o;

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
