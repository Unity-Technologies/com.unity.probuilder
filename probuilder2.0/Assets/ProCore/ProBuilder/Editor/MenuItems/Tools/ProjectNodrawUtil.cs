#if UNITY_5_0_0
#define UNITY_5
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.Actions
{
	public class ProjectNodrawUtil : MonoBehaviour
	{
		// Hides Nodraw faces in every scene.

		// [MenuItem("Tools/ProBuilder/Actions/Project-wide Hide Nodraw")]
		public static void ProjectWideHideNodraw()
		{
	#if UNITY_WEB_PLAYER
			if(EditorUtility.DisplayDialog("Switch Build Platform", "This Action requires that your build platform NOT be set to Webplayer.  Just change it to Standalone.  After running the script, you may switch back to whatever build platform you'd like.", "Okay"))
				return;
			else
				return;
	#endif
			HideNodrawProjectWide();
		}

		// [MenuItem("Tools/ProBuilder/Actions/Project-wide Show Nodraw")]
		public static void ProjectWideShowNodraw()
		{
	#if UNITY_WEB_PLAYER
			if(EditorUtility.DisplayDialog("Switch Build Platform", "This Action requires that your build platform NOT be set to Webplayer.  Just change it to Standalone.  After running the script, you may switch back to whatever build platform you'd like.", "Okay"))
				return;
			else
				return;
	#endif
			ShowNodrawProjectWide();
		}


		public static void ShowNodrawProjectWide()
		{
			string curScene = EditorApplication.currentScene;

			// Get all scenes in project
			foreach(string cheese in GetScenes())
			{
				EditorApplication.OpenScene(cheese);

				pb_Object[] pbs = (pb_Object[])FindObjectsOfType(typeof(pb_Object));
				foreach(pb_Object pb in pbs)
					pb.ShowNodraw();

				#if UNITY_5
				EditorUtility.UnloadUnusedAssetsImmediate();
				#else
				EditorUtility.UnloadUnusedAssets();
				#endif

				EditorApplication.SaveScene(cheese);
			}

			EditorApplication.OpenScene(curScene);
		}

		public static void HideNodrawProjectWide()
		{
			string curScene = EditorApplication.currentScene;

			// Get all scenes in project
			foreach(string cheese in GetScenes())
			{
				EditorApplication.OpenScene(cheese);

				pb_Object[] pbs = (pb_Object[])FindObjectsOfType(typeof(pb_Object));
				foreach(pb_Object pb in pbs)
				{
					pb.HideNodraw();
				}

				#if UNITY_5
				EditorUtility.UnloadUnusedAssetsImmediate();
				#else
				EditorUtility.UnloadUnusedAssets();
				#endif
				EditorApplication.SaveScene(cheese);
			}

			EditorApplication.OpenScene(curScene);
		}

		public static string[] GetScenes()
		{
			string[] allFiles = System.IO.Directory.GetFiles("Assets/", "*.*", System.IO.SearchOption.AllDirectories);
			string[] allScenes = System.Array.FindAll(allFiles, name => name.EndsWith(".unity"));
			return allScenes;
		}	
	}
}