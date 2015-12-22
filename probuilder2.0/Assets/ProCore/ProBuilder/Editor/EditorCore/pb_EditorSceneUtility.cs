#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define PRE_UNITY_5_3
#endif

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Helper methods for working with scenes in the editor.
	 */
	public static class pb_EditorSceneUtility
	{
		/**
		 *	Prompt user to save all current scenes.  Returns false if user cancels.
		 *	\sa EditorApplication.SaveCurrentSceneIfUserWantsTo(), EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo().
		 */
		public static bool SaveCurrentSceneIfUserWantsTo()
		{
#if PRE_UNITY_5_3
			return EditorApplication.SaveCurrentSceneIfUserWantsTo();
#else
			return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
#endif				
		}

		/**
		 *	Open @sceneName (non-additively)
		 */
		public static void OpenScene(string sceneName)
		{
#if PRE_UNITY_5_3
			EditorApplication.OpenScene(sceneName);
#else
			EditorSceneManager.OpenScene(sceneName);
#endif
		}

		/**
		 *	Save the current active scene to path, optionally as a copy.
		 */
		public static void SaveScene(string path, bool saveAsCopy)
		{
#if PRE_UNITY_5_3
			EditorApplication.SaveScene(path, saveAsCopy);
#else
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path, saveAsCopy);
#endif
		}

		/**
		 *	Save all open scenes.
		 */
		public static void SaveOpenScenes()
		{
#if PRE_UNITY_5_3
			EditorApplication.SaveScene();
#else
			EditorSceneManager.SaveOpenScenes();
#endif
		}

		/**
		 *	Save currently active scene.
		 */
		public static void SaveScene()
		{
#if PRE_UNITY_5_3
			EditorApplication.SaveScene();
#else
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
#endif
		}
	}
}
