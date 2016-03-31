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
			pb_Menu_Commands.MenuFacetizeObject(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	}
}
