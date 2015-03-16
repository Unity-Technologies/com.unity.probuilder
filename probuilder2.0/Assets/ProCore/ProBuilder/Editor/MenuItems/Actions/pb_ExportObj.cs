using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for OBJ export.
	 */
	public class pb_ExportObj : Editor
	{

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Export Selected to OBJ", true, pb_Constant.MENU_ACTIONS + 40)]
		public static bool VerifyExportOBJ()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Export Selected to OBJ", false, pb_Constant.MENU_ACTIONS + 40)]
		public static void ExportOBJ()
		{
			pb_Editor_Utility.ExportOBJ(pbUtil.GetComponents<pb_Object>(Selection.transforms)); 
		}
	}
}