using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.Actions
{
	public class pb_ForceSceneRefresh : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Force Refresh Scene", false, pb_Constant.MENU_REPAIR)]
		public static void MenuForceSceneRefresh()
		{
			pb_Editor_Utility.ForceRefresh(true);
		}
	}
}