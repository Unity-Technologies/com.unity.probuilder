/**
*  @ Matt1988
*  This extension was built by @Matt1988
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	public class pb_SetPivot : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Set Pivot _%j", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		public static bool VerifySetPivot()
		{
			return pb_Editor.instance != null && pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Set Pivot _%j", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		static void init()
		{
			pb_Menu_Commands.MenuSetPivot(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	}
}
