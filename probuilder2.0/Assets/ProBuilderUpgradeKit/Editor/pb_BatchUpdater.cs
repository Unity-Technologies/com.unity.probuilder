using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using ProBuilder2.Common;

namespace ProBuilder2.UpgradeKit
{
	/**
	 * Editor script looks in the project for scenes to update and automatically does so.
	 */
	public class pb_BatchUpdater : EditorWindow
	{
		const string SCENE_BACKUP_FOLDER = "ProBuilderUpgradeSceneBackup";

		/**
		 * Iterate all scenes in project and run the updater.
		 */
		[MenuItem("Tools/ProBuilder/Upgrade/Batch Prepare Scenes for Upgrade %g")]
		static void MenuBatchUpdate()
		{
			string[] scenes = FindAssetsWithExtension(".unity");

			/// If the current scene is dirty and the user opts to cancel instead of discarding or saving,
			/// exit the batch update.
			if(EditorApplication.isSceneDirty && !EditorApplication.SaveCurrentSceneIfUserWantsTo() )
				return;

			UpdateScenes(scenes);
		}

		static System.Text.StringBuilder sb = null;

		static void UpdateScenes(string[] scenePaths)
		{
			if(!Directory.Exists("Assets/" + SCENE_BACKUP_FOLDER))
				AssetDatabase.CreateFolder("Assets", SCENE_BACKUP_FOLDER);

			sb = new System.Text.StringBuilder();
			sb.AppendLine("A backup of every scene has been placed in \"Assets/" + SCENE_BACKUP_FOLDER + "\".  Note that failed object serialization may simply mean that the object is a prefab and already serialized.\n======\n");

			for(int i = 0; i < scenePaths.Length; i++)
			{
				string sceneName = Path.GetFileName(scenePaths[i]);

				sb.AppendLine("Open scene: " + sceneName + "\n");

				EditorApplication.OpenScene(scenePaths[i]);
				string backup_path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + SCENE_BACKUP_FOLDER + "/" + sceneName);
				EditorApplication.SaveScene(backup_path, true);

				pb_UpgradeBridgeEditor.SceneInfo info = pb_UpgradeBridgeEditor.PrepareScene(UpgradeLog, pb_UpgradeBridgeEditor.DisplayProgress);

				sb.AppendLine(sceneName + " results: " + info.ToString() );
				sb.AppendLine("----");

				EditorApplication.SaveScene();
			}

			EditorUtility.ClearProgressBar();

			Debug.Log("ProBuilder batch scene upgrade log written to \"Assets/ProBuilderUpgradeLog.txt\"");
			File.WriteAllText("ProBuilderUpgradeLog.txt", sb.ToString());
		}

		static void UpgradeLog(string msg, MessageType type)
		{
			// pb_UpgradeBridgeEditor.DisplayLog(msg, type);
			sb.AppendLine(type + ": " + msg);
		}

		/**
		 * Deep search the Assets/ folder for any file matching extension.
		 */
		static string[] FindAssetsWithExtension(string extension)
		{
			string current_directory = Directory.GetCurrentDirectory();
			return Directory.GetFiles("Assets\\", "*" + extension, SearchOption.AllDirectories).Select(x => x.Replace(current_directory, "")).ToArray();
		}
	}

}