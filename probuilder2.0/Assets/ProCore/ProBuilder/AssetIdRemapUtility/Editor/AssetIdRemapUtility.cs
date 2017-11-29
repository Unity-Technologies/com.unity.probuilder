using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ProBuilder.AssetUtility
{
	class AssetIdRemapUtility : EditorWindow
	{
		TextAsset m_RemapFile = null;

		[MenuItem("Tools/ProBuilder/Repair/Convert to Package Manager")]
		static void MenuInitRemapGuidEditor()
		{
			GetWindow<AssetIdRemapUtility>(true, "Package Manager Conversion Utility", true);
		}

		void OnGUI()
		{
			m_RemapFile = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapFile, typeof(TextAsset), false);

			SerializationMode serializationMode = EditorSettings.serializationMode;

			GUI.enabled = serializationMode == SerializationMode.ForceText;

			if (GUILayout.Button("Convert to Package Manager"))
				DoIt();

			GUI.enabled = true;
		}

		void DoIt()
		{
			AssetIdRemapObject remapObject = new AssetIdRemapObject();
			JsonUtility.FromJsonOverwrite(m_RemapFile.text, remapObject);

			string[] extensionsToScanForGuidRemap = new string[]
			{
				"*.meta",
				"*.asset",
				"*.mat",
				"*.unity",
			};

			string assetStoreProBuilderDirectory = FindAssetStoreProBuilderInstall();

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

		static void DoAssetIdentifierRemap(string path, IEnumerable<AssetIdentifierTuple> map)
		{
			var sr = new StreamReader(path);
			var sw = new StreamWriter(path + ".remap", false);

			List<StringTuple> replace = new List<StringTuple>();

			// order is important - {fileId, guid} in asset files needs to be applied first
			IEnumerable<AssetIdentifierTuple> assetIdentifierTuples = map as AssetIdentifierTuple[] ?? map.ToArray();

			foreach (var kvp in assetIdentifierTuples)
			{
				replace.Add(new StringTuple(
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.source.fileId, kvp.source.guid),
					string.Format("{{fileId: {0}, guid: {1}, type:", kvp.destination.fileId, kvp.destination.guid)));
			}

			HashSet<string> used = new HashSet<string>();

			foreach (var kvp in assetIdentifierTuples)
			{
				// AssetIdentifier list will contain duplicate guids (assets can contain sub-assets, separated by fileId)
				// when swapping meta file guids we don't need multiple entries
				if(used.Add(kvp.source.guid))
					replace.Add(new StringTuple(
						string.Format("guid: {0}", kvp.source.guid),
						string.Format("guid: {0}", kvp.destination.guid)));
			}

			int modified = 0;

			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();

				foreach (var kvp in replace)
				{
					if (line.Contains(kvp.key))
					{
						modified++;
						line = line.Replace(kvp.key, kvp.value);
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

		static string FindAssetStoreProBuilderInstall()
		{
			string dir = null;

			string[] matches = Directory.GetDirectories("Assets", "ProBuilder", SearchOption.AllDirectories);

			foreach (var match in matches)
			{
				dir = match.Replace("\\", "/") +  "/";
				if (dir.Contains("ProBuilder") && ValidateProBuilderRoot(dir))
					break;
			}

			return dir;
		}

		static bool ValidateProBuilderRoot(string dir)
		{
			return !string.IsNullOrEmpty(dir) &&
			       Directory.Exists(dir + "/Classes") &&
			       Directory.Exists(dir + "/Icons") &&
			       Directory.Exists(dir + "/Editor") &&
			       Directory.Exists(dir + "/Shader");
		}
	}
}
