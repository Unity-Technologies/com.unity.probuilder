using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

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
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Upgrade/Batch Prepare Scenes for Upgrade", false, pb_Constant.MENU_REPAIR)]
		static void MenuBatchUpdate()
		{
			/// If the current scene is dirty and the user opts to cancel instead of discarding or saving,
			/// exit the batch update.
			if(!pb_EditorSceneUtility.SaveCurrentSceneIfUserWantsTo())
				return;

			if(!EditorUtility.DisplayDialog("Batch Prepare Scenes for Upgrade", "This tool will open every scene in your project and run the pre-upgrade process, and may take a few minutes.  Once complete, a log of the upgrade activity will be available in the Assets folder", "Okay", "Cancel"))
				return;

			string[] scenes = FindAssetsWithExtension(".unity");
			UpdateScenes(scenes);

			EditorUtility.DisplayDialog("Batch Prepare Scene for Upgrade", "A log of the upgrade has been placed in \"Assets/ProBuilderUpgradeLog.txt.\".\nPlease delete the ProCore/ProBuilder, import the new version, then run \"Batch Re-Attach ProBuilder Scripts\".", "Okay");
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Upgrade/Batch Re-Attach ProBuilder Scripts", false, pb_Constant.MENU_REPAIR)]
		static void MenuBatchReattach()
		{
			if(!pb_EditorSceneUtility.SaveCurrentSceneIfUserWantsTo())
				return;
			
			string[] scenes = FindAssetsWithExtension(".unity");
			ReattachScenes(scenes);
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

				sb.AppendLine("Open scene: " + sceneName);
				sb.AppendLine("----");

				pb_EditorSceneUtility.OpenScene(scenePaths[i]);
				string backup_path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + SCENE_BACKUP_FOLDER + "/" + sceneName);
				pb_EditorSceneUtility.SaveScene(backup_path, true);

				EditorUtility.DisplayProgressBar("Batch Prepare Scenes for Upgrade", sceneName, i / (float)scenePaths.Length);

				pb_UpgradeBridgeEditor.SceneInfo info = pb_UpgradeBridgeEditor.PrepareScene(UpgradeLog, null);

				sb.AppendLine("Results: " + info.ToString() );
				sb.AppendLine("\n");

				pb_EditorSceneUtility.SaveScene();
			}

			EditorUtility.ClearProgressBar();

			Debug.Log("ProBuilder batch scene upgrade log written to \"Assets/ProBuilderUpgradeLog_Prepare.txt\"");
			File.WriteAllText("Assets/ProBuilderUpgradeLog_Prepare.txt", sb.ToString());
		}

		static void ReattachScenes(string[] scenePaths)
		{
			sb = new System.Text.StringBuilder();

			for(int i = 0 ; i < scenePaths.Length; i++)
			{
				string sceneName = Path.GetFileName(scenePaths[i]);

				sb.AppendLine("Open scene: " + sceneName);
				sb.AppendLine("----");

				pb_EditorSceneUtility.OpenScene(scenePaths[i]);

				EditorUtility.DisplayProgressBar("Batch Re-attach ProBuilder Scripts", sceneName, i / (float)scenePaths.Length);

				pb_UpgradeBridgeEditor.SceneInfo info = pb_UpgradeBridgeEditor.DeserializeScene(UpgradeLog, null);

				sb.AppendLine("Results: " + info.ToString() );
				sb.AppendLine("\n");

				pb_EditorSceneUtility.SaveScene();

			}

			EditorUtility.ClearProgressBar();

			Debug.Log("ProBuilder batch scene upgrade log written to \"Assets/ProBuilderUpgradeLog_Reattach.txt\"");
			File.WriteAllText("Assets/ProBuilderUpgradeLog_Reattach.txt", sb.ToString());
		}

		static void UpgradeLog(string msg, MessageType type)
		{
			sb.AppendLine(type + ": " + msg);
		}

		/**
		 * Deep search the Assets/ folder for any file matching extension.
		 */
		static string[] FindAssetsWithExtension(string extension)
		{
			string current_directory = Directory.GetCurrentDirectory();
			return Directory.GetFiles("Assets\\", "*" + extension, SearchOption.AllDirectories).Where(x => !x.Contains("Assets/" + SCENE_BACKUP_FOLDER)).Select(x => x.Replace(current_directory, "")).ToArray();
		}
	}

}
