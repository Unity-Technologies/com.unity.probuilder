using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Helper functions for working with Unity object selection & ProBuilder element selection.
	 */
	public static class pb_Selection
	{
		private static pb_Object[] selection { get { return pb_Editor.instance != null ? pb_Editor.instance.selection : pbUtil.GetComponents<pb_Object>(Selection.transforms); } }

		public static void AddToSelection(GameObject t)
		{
			if(t == null || Selection.objects.Contains(t))
				return;

			Object[] temp = new Object[Selection.objects.Length + 1];

			temp[0] = t;

			for(int i = 1; i < temp.Length; i++)
				temp[i] = Selection.objects[i-1];

			Selection.objects = temp;
		}

		public static void RemoveFromSelection(GameObject t)
		{
			int ind = System.Array.IndexOf(Selection.objects, t);
			if(ind < 0)
				return;

			Object[] temp = new Object[Selection.objects.Length - 1];

			for(int i = 1; i < temp.Length; i++) {
				if(i != ind)
					temp[i] = Selection.objects[i];
			}

			Selection.objects = temp;
		}

		public static void SetSelection(IList<GameObject> newSelection)
		{
			pbUndo.RecordSelection(selection, "Change Selection");

			ClearElementAndObjectSelection();

			// if the previous tool was set to none, use Tool.Move
			if(Tools.current == Tool.None)
				Tools.current = Tool.Move;

			if(newSelection != null && newSelection.Count > 0) {
				Selection.activeTransform = newSelection[0].transform;
				Selection.objects = newSelection.ToArray();
			}
			else
			{
				Selection.activeTransform = null;
			}
		}

		public static void SetSelection(GameObject go)
		{
			pbUndo.RecordSelection(selection, "Change Selection");
			ClearElementAndObjectSelection();
			AddToSelection(go);
		}

		/**
		 *	Clears all `selected` caches associated with each pb_Object in the current selection.  The means triangles, faces, and edges.
		 */
		public static void ClearElementSelection()
		{
			if(pb_Editor.instance)
				pb_Editor.instance.ClearElementSelection();
		}

		public static void ClearElementAndObjectSelection()
		{
			ClearElementSelection();
			Selection.objects = new Object[0];
		}
	}
}
