#pragma warning disable 0168

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for manually regenerating all ProBuilder mesh references in scene.
	 */
	public class pb_RepairMeshReferences : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Repair Mesh References", false, pb_Constant.MENU_REPAIR)]
		public static void MenuRefreshMeshReferences()
		{	
			RefreshMeshReferences(true);
		}

		/**
		 *	\brief Force refreshes all meshes in scene.
		 */
		private static void RefreshMeshReferences(bool interactive)
		{
			pb_Object[] all = (pb_Object[])GameObject.FindObjectsOfType(typeof(pb_Object));
			for(int i = 0; i < all.Length; i++)
			{
				if(interactive)
				EditorUtility.DisplayProgressBar(
					"Refreshing ProBuilder Objects",
					"Reshaping pb_Object " + all[i].id + ".",
					((float)i / all.Length));
		 		
				pb_Object pb = all[i];

				pb_EditorUtility.VerifyMesh(pb);
			}
			if(interactive)
			{
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Refresh ProBuilder Objects", "Successfully refreshed all ProBuilder objects in scene.", "Okay");
			}
		}
	}
}
