using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{
	/**
	 * Triangulates a ProBuilder object.
	 *
	 * MenuItem: Tools -> ProBuilder -> Geometry -> Triangulate Selection 
	 */
	public class pb_Triangulate : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Triangulate Object", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
		public static bool MenuVerifyTriangulateSelection()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Triangulate Object", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
		public static void MenuTriangulatePbObjects()
		{
			pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			pbUndo.RecordObjects(selection, "Triangulate Objects");

			for(int i = 0; i < selection.Length; i++)
			{

				pbTriangleOps.Triangulate(selection[i]);

				selection[i].ToMesh();
				selection[i].Refresh();
				selection[i].Optimize();
			}

			if(pb_Editor.instance)
			{
				pb_Editor.instance.UpdateSelection();
			}

			pb_Editor_Utility.ShowNotification(selection.Length > 0 ? "Triangulate" : "Nothing Selected");
		}
	}
}