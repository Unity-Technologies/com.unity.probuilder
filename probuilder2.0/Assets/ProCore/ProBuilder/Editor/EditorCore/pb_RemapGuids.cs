using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProBuilder.Core;
using UnityEditor;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	class pb_RemapGuids : EditorWindow
	{
		TextAsset m_RemapFile = null;

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Convert to Package Manager")]
		static void MenuInitRemapGuidEditor()
		{
			GetWindow<pb_RemapGuids>(true, "Package Manager Conversion Utility", true);
		}

		void OnGUI()
		{
			m_RemapFile = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapFile, typeof(TextAsset), false);

			SerializationMode serializationMode = EditorSettings.serializationMode;

			GUI.enabled = serializationMode == SerializationMode.ForceText;

			if (GUILayout.Button("Convert to Package Manager"))
				DoIt();

			if (GUILayout.Button("Show me guids and fileids"))
			{
				GUID guid = new GUID(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));

				ObjectIdentifier[] ids =
					BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(guid, EditorUserBuildSettings.activeBuildTarget);

				StringBuilder sb = new StringBuilder();

				sb.AppendLine(Selection.activeObject.name);
				sb.AppendLine(Selection.activeObject.GetInstanceID().ToString());
				sb.AppendLine(AssetDatabase.GetAssetPath(Selection.activeObject));
				sb.AppendLine(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
				sb.AppendLine("----");
				sb.AppendLine("GetGUIDAndLocalIdentifierInFile");
				foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)))
				{
					GUID g;
					long file;
					if (AssetDatabase.GetGUIDAndLocalIdentifierInFile(o.GetInstanceID(), out g, out file))
						sb.AppendLine("  " + o.name + "\n    " + o.GetInstanceID() + "\n    " + g + "\n    " + file);
				}

				sb.AppendLine("----");
				sb.AppendLine("LookupInstanceIDFromPathAndFileID");

				foreach (var id in ids)
				{
					int inst = AssetDatabase.LookupInstanceIDFromPathAndFileID(AssetDatabase.GetAssetPath(Selection.activeObject),
						(int) id.localIdentifierInFile);

					UnityEngine.Object o = EditorUtility.InstanceIDToObject(inst);

					sb.AppendLine("  " + (o != null ? o.name : "null") + "\n    " + inst + "\n    " + id.guid + "\n    " + id.localIdentifierInFile);
				}
				Debug.Log(sb.ToString());
			}

			GUI.enabled = true;
		}

		void DoIt()
		{
			pb_GuidRemapObject remapObject = new pb_GuidRemapObject();
			JsonUtility.FromJsonOverwrite(m_RemapFile.text, remapObject);

			string[] extensionsToScanForGuidRemap = new string[]
			{
				"*.meta",
				"*.asset",
				"*.mat",
				"*.unity",
			};

			string assetStoreProBuilderDirectory = pb_FileUtil.FindAssetStoreProBuilderInstall();

			if (string.IsNullOrEmpty(assetStoreProBuilderDirectory))
			{
				// todo pop up modal dialog asking user to point to ProBuilder directory (and validate before proceeding)
				Debug.LogWarning("Couldn't find an Asset Store install of ProBuilder. Aborting conversion process.");
				return;
			}

			foreach (string extension in extensionsToScanForGuidRemap)
			{
				foreach (string str in Directory.GetFiles("Assets", extension, SearchOption.AllDirectories))
					DoAssetIdentifierRemap(str, remapObject.map);
			}
		}

		static void DoAssetIdentifierRemap(string path, IEnumerable<pb_AssetIdentifierTuple> map)
		{
			var sr = new StreamReader(path);
			var sw = new StreamWriter(path + ".remap", false);

			System.Collections.Generic.List<pb_Tuple<string, string>> replace = new System.Collections.Generic.List<pb_Tuple<string, string>>();

			// order is important - {fileId, guid} in asset files needs to be applied first
			IEnumerable<pb_AssetIdentifierTuple> assetIdentifierTuples = map as pb_AssetIdentifierTuple[] ?? map.ToArray();

			foreach (var kvp in assetIdentifierTuples)
			{
				replace.Add(new pb_Tuple<string, string>(
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.source.fileId, kvp.source.guid),
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.destination.fileId, kvp.destination.guid)));
			}

			HashSet<string> used = new HashSet<string>();

			foreach (var kvp in assetIdentifierTuples)
			{
				// AssetIdentifier list will contain duplicate guids (assets can contain sub-assets, separated by fileId)
				// when swapping meta file guids we don't need multiple entries
				if(used.Add(kvp.source.guid))
					replace.Add(new pb_Tuple<string, string>(
						string.Format("guid: {0}", kvp.source.guid),
						string.Format("guid: {0}", kvp.destination.guid)));
			}

			int modified = 0;

			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();

				foreach (var kvp in replace)
				{
					if (line.Contains(kvp.Item1))
					{
						modified++;
						line = line.Replace(kvp.Item1, kvp.Item2);
						break;
					}
				}

				sw.WriteLine(line);
			}

			sr.Close();
			sw.Close();

			if (modified > 0)
			{
				File.Delete(path);
				File.Move(path + ".remap", path);
			}
		}
	}
}