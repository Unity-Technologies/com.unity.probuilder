using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.Collections;

namespace ProBuilder2.EditorCommon
{

	/**
	 * Contains methods specific to aiding in the upgrade process.
	 */
	public class pb_Upgrade : Editor
	{
	#region 2.3
		 
		/**
		 * Upgrades a project to ProBuilder 2.3
		 */
		public static void Upgrade_2_3()
		{
			if( EditorUtility.DisplayDialog("Upgrade to 2.3", "The UV overhaul means some changes for existing UVs.  Namely, the scale parameter is inverted.\n\nTo fix this, open each scene with ProBuilder objects present and run\n\n\"Tools -> ProBuilder -> Repair -> ProBuilder -> Invert UV Scale (Scene)\"\n\nClick \"Okay\" to repair UVs in the current scene.", "Okay", "Cancel") )
			{
				InvertUVScale_Scene();
			}
		}

		public static void InvertUVScale_Scene()
		{
			InvertUVScale( (pb_Object[])FindObjectsOfType(typeof(pb_Object)), true );
			pb_EditorSceneUtility.SaveCurrentSceneIfUserWantsTo();
		}

		public static void InvertUVScale_SelectedFaces()
		{
			InvertUVScale( pbUtil.GetComponents<pb_Object>(Selection.transforms), false );
		}

		public static void InvertUVScale_Selection()
		{
			InvertUVScale( pbUtil.GetComponents<pb_Object>(Selection.transforms), true );
		}

		public static void InvertUVScale(pb_Object[] selection, bool allFaces)
		{
			pbUndo.RecordObjects(selection, "Invert UV Scale");
			int fc = 0;

			foreach(pb_Object pb in selection)
			{
				foreach(pb_Face face in allFaces ? pb.faces : pb.SelectedFaces)
				{
					face.uv.scale = new Vector2(1f/face.uv.scale.x, 1f/face.uv.scale.y);
				}

				fc += allFaces ? pb.faces.Length : pb.SelectedFaceCount;

				pb.RefreshUV();
			}

			SceneView scn = pb_EditorUtility.GetSceneView();
			if(fc > 0)
				scn.ShowNotification(new GUIContent("Success! Inverted UV Scale of\n" + selection.Length + (selection.Length > 1 ? " objects" : " object") + " and " + fc + " faces.", ""));
			else
				scn.ShowNotification(new GUIContent("Nothing Selected", ""));
		}
	#endregion
	}
}
