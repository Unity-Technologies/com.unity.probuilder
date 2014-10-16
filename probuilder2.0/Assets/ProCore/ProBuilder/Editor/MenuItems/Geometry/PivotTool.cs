/**
*  @ Matt1988
*  This extension was built by @Matt1988
*/

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class PivotTool : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Set Pivot _%j", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		public static bool VerifySetPivot()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Set Pivot _%j", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		static void init()
		{
			pb_Menu_Commands.MenuSetPivot(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	}
}