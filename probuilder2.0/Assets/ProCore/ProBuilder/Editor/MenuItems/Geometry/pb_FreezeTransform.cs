using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{

	/**
	 * Set the pivot point of a pb_Object mesh to 0,0,0 while retaining current world space.
	 */
	public class pb_FreezeTransform : Editor
	{

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Freeze Transforms", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
		public static bool MenuVerifyFreezeTransforms()
		{
			return Selection.transforms.GetComponents<pb_Object>().Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Freeze Transforms", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
		public static void MenuFreezeTransforms()
		{
			pb_Menu_Commands.MenuFreezeTransforms( pbUtil.GetComponents<pb_Object>(Selection.transforms) );
		}
	}
}
