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
			ForceRefresh(true);
		}

		/**
		 *	\brief Force refreshes all meshes in scene.
		 */
		private static void ForceRefresh(bool interactive)
		{
			pb_Object[] all = (pb_Object[])GameObject.FindObjectsOfType(typeof(pb_Object));
			for(int i = 0; i < all.Length; i++)
			{
				if(interactive)
				EditorUtility.DisplayProgressBar(
					"Refreshing ProBuilder Objects",
					"Reshaping pb_Object " + all[i].id + ".",
					((float)i / all.Length));

				all[i].ToMesh();
				all[i].Refresh();
				all[i].GenerateUV2(true);
			}
			if(interactive)
			{
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Refresh ProBuilder Objects", "Successfully refreshed all ProBuilder objects in scene.", "Okay");
			}
		}
	}
}