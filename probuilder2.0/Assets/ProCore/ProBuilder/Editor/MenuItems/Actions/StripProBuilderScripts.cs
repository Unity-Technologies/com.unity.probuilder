using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class StripProBuilderScripts : Editor 
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Strip All ProBuilder Scripts in Scene")]
		public static void StripAllScenes()
		{
			
			if(!EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts in the scene.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
				return;

			pb_Object[] all = (pb_Object[])FindObjectsOfType(typeof(pb_Object));

			Strip(all);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Strip ProBuilder Scripts in Selection", true, 0)]
		public static bool VerifyStripSelection()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Strip ProBuilder Scripts in Selection")]
		public static void StripAllSelected()
		{
			if(!EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts on the selected objects.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
				return;

			pb_Object[] all = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			GameObject[] gos = new GameObject[all.Length];
			for(int i = 0; i < all.Length; i++) gos[i] = all[i].gameObject;

			Strip(all);
		}

		public static void Strip(pb_Object[] all)
		{
			for(int i = 0; i < all.Length; i++)
			{
				EditorUtility.DisplayProgressBar(
					"Stripping ProBuilder Scripts",
					"Working over " + all[i].id + ".",
					((float)i / all.Length));

				Mesh m = pbUtil.DeepCopyMesh(all[i].msh);

				GameObject go = all[i].gameObject;

				DestroyImmediate(all[i]);
				
				if(go.GetComponent<pb_Entity>())
					DestroyImmediate(go.GetComponent<pb_Entity>());

				go.GetComponent<MeshFilter>().sharedMesh = m;
				if(go.GetComponent<MeshCollider>())
					go.GetComponent<MeshCollider>().sharedMesh = m;
			}

			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "Successfully stripped out all ProBuilder components.", "Okay");

		}
	}
}