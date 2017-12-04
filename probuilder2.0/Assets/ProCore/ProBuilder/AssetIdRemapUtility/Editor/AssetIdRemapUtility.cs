using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ProBuilder.AssetUtility
{
	class AssetIdRemapUtility : EditorWindow
	{
		const string k_RemapFileDefaultPath = "Assets/ProBuilder/Upgrade/AssetIdRemap.json";

		TextAsset m_RemapFile = null;
		AssetTreeView m_AssetsToDeleteTreeView;
		[SerializeField] TreeViewState m_TreeViewState = null;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState = null;

		[MenuItem("Tools/ProBuilder/Repair/Convert to Package Manager")]
		internal static void OpenConversionEditor()
		{
			GetWindow<AssetIdRemapUtility>(true, "Package Manager Conversion Utility", true);
		}

		void OnEnable()
		{
			if (m_RemapFile == null)
				m_RemapFile = AssetDatabase.LoadAssetAtPath<TextAsset>(k_RemapFileDefaultPath);
#if DEBUG
			if (m_RemapFile == null)
				m_RemapFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ProCore/ProBuilder/Upgrade/AssetIdRemap");
#endif

			if(m_TreeViewState == null)
				m_TreeViewState = new TreeViewState();

			if(m_MultiColumnHeaderState == null)
				m_MultiColumnHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
				{
					new MultiColumnHeaderState.Column()
					{
						headerContent = new GUIContent("Asset Store Files to Delete")
					}
				});

			MultiColumnHeader m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState)
			{
				height = 0
			};

			m_AssetsToDeleteTreeView = new AssetTreeView(m_TreeViewState, m_MultiColumnHeader);
			m_AssetsToDeleteTreeView.directory = FindAssetStoreProBuilderInstall();
			m_AssetsToDeleteTreeView.Reload();
			m_AssetsToDeleteTreeView.ExpandAll();
		}

		void OnGUI()
		{
			m_RemapFile = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapFile, typeof(TextAsset), false);

			SerializationMode serializationMode = EditorSettings.serializationMode;

			if (serializationMode != SerializationMode.ForceText)
			{
				EditorGUILayout.HelpBox("Cannot Convert Binary Serialized Project\n\nPlease enable ForceText serialization to proceed with the conversion process.\n\nYou may re-enable Binary or Mixed serialization after the conversion is complete.", MessageType.Warning);

				EditorGUI.BeginChangeCheck();
				serializationMode = (SerializationMode) EditorGUILayout.EnumPopup("Serialization Mode", serializationMode);
				if (EditorGUI.EndChangeCheck())
					EditorSettings.serializationMode = serializationMode;
			}

			if (GUILayout.Button("Convert to Package Manager"))
			{
				if (!IsProjectTextSerialized())
					Debug.LogError("Cannot update project with binary serialization!");
				else
					DoIt(m_RemapFile);
			}

			Rect lastRect = GUILayoutUtility.GetLastRect();

			m_AssetsToDeleteTreeView.OnGUI(new Rect(
				lastRect.x,
				lastRect.yMax + 4,
				position.width - lastRect.x * 2f,
				position.height - lastRect.yMax - 8));

			GUI.enabled = true;
		}

		static void DoIt(TextAsset jsonAsset)
		{
			AssetIdRemapObject remapObject = new AssetIdRemapObject();
			JsonUtility.FromJsonOverwrite(jsonAsset.text, remapObject);

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

			EditorApplication.LockReloadAssemblies();

			var log = new StringBuilder();
			int remappedReferences = 0;
			int modifiedFiles = 0;
			string[] assets = extensionsToScanForGuidRemap.SelectMany(x => Directory.GetFiles("Assets", x, SearchOption.AllDirectories)).ToArray();

			for (int i = 0, c = assets.Length; i < c; i++)
			{
				EditorUtility.DisplayProgressBar("Asset Id Remap", assets[i], i / (float) c);

				int modified;

				if(!DoAssetIdentifierRemap(assets[i], remapObject.map, out modified))
					log.AppendLine("Failed scanning asset: " + assets[i]);

				remappedReferences += modified;
				if (modified > 0)
					modifiedFiles++;
			}

			EditorUtility.ClearProgressBar();

			Debug.Log(string.Format("Remapped {0} references in {1} files.\n\n{2}", remappedReferences, modifiedFiles, log.ToString()));

			EditorApplication.UnlockReloadAssemblies();

			PackageImporter.Reimport(PackageImporter.EditorCorePackageManager);
		}

		static bool DoAssetIdentifierRemap(string path, IEnumerable<AssetIdentifierTuple> map, out int modified, bool remapSourceGuid = false)
		{
			modified = 0;

			try
			{
				var sr = new StreamReader(path);
				var sw = new StreamWriter(path + ".remap", false);

				List<StringTuple> replace = new List<StringTuple>();

				IEnumerable<AssetIdentifierTuple> assetIdentifierTuples = map as AssetIdentifierTuple[] ?? map.ToArray();

				foreach (var kvp in assetIdentifierTuples)
				{
					if (kvp.source.fileId.Equals(kvp.destination.fileId) && kvp.source.guid.Equals(kvp.destination.guid))
						continue;

					replace.Add(new StringTuple(
						string.Format("{{fileID: {0}, guid: {1}, type:", kvp.source.fileId, kvp.source.guid),
						string.Format("{{fileID: {0}, guid: {1}, type:", kvp.destination.fileId, kvp.destination.guid)));
				}

				// If remapping in-place (ie, changing a package guids) then also apply to metadata
				if (remapSourceGuid)
				{
					HashSet<string> used = new HashSet<string>();
					foreach (var kvp in assetIdentifierTuples)
					{
						// AssetIdentifier list will contain duplicate guids (assets can contain sub-assets, separated by fileId)
						// when swapping meta file guids we don't need multiple entries
						if (used.Add(kvp.source.guid))
							replace.Add(new StringTuple(
								string.Format("guid: {0}", kvp.source.guid),
								string.Format("guid: {0}", kvp.destination.guid)));
					}
				}

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
				else
				{
					File.Delete(path + ".remap");
				}

			}
			catch
			{
				return false;
			}

			return true;
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

		static bool IsProjectTextSerialized()
		{
			return EditorSettings.serializationMode == SerializationMode.ForceText;
		}
	}
}
