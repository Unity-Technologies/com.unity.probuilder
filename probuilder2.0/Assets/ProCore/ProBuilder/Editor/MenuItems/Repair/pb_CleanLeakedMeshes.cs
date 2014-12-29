using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ProBuilder2.Actions
{
	public class pb_CleanLeakedMeshes : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Clean Leaked Meshes", false, pb_Constant.MENU_REPAIR)]
		public static void CleanUp()
		{
			// if(EditorUtility.DisplayDialog("Clean Leaked Meshes?",
			// 	"Cleaning leaked meshes will permenantly delete any deleted pb_Objects, are you sure you don't want to undo?", "Clean Up", "Stay Dirty"))
			// {
				#if !UNITY_5
				EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
				#else
				EditorUtility.UnloadUnusedAssetsImmediate();
				#endif
			// }
		}
	}
}