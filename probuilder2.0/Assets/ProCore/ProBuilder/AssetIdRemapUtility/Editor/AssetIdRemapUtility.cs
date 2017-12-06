using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Tizen;
using UObject = UnityEngine.Object;

namespace ProBuilder.AssetUtility
{
	class AssetIdRemapUtility : EditorWindow
	{
		const string k_RemapFileDefaultPath = "Assets/ProBuilder/Upgrade/AssetIdRemap.json";

		static readonly string[] k_AssetStoreDirectorySuggestedDeleteIgnoreFilter = new string[]
		{
			"(^|(?<=/))Data(/|)$",
			"(^|(?<=/))ProBuilderMeshCache(/|)$",
			".meta$"
		};

		static readonly string[] k_AssetStoreSuggestedFileDeleteIgnoreFilter = new string[]
		{
			".meta$",
			".asset$"
		};

		static readonly string[] k_AssetStoreMustDelete = new string[]
		{
			"pb_Object.cs",
			"pb_Entity.cs",
			"ProBuilder/Classes",
			"ProBuilder/Editor",
			"ProBuilderCore-Unity5.dll",
			"ProBuilderMeshOps-Unity5.dll",
			"ProBuilderEditor-Unity5.dll"
		};

		// @todo show a warning when any code is not getting deleted
		static readonly string[] k_AssetStoreShouldDelete = new string[]
		{
			"ProBuilder/API Examples",
			"API Examples/Editor",
			"ProBuilder/About",
			"ProBuilder/Shader",
		};

		[Flags]
		enum ConversionReadyState
		{
			Ready = 0,
			SerializationError = 1 << 1,
			AssetStoreDeleteError = 1 << 2,
			AssetStoreDeleteWarning = 1 << 3,
			AssetStoreInstallNotFound = 1 << 4,
			FileLocked = 1 << 4,
		};

		TextAsset m_RemapFile = null;
		[SerializeField] string m_DeprecatedProBuilderDirectory;
		[SerializeField] bool m_DeprecatedProBuilderFound;
		AssetTreeView m_AssetsToDeleteTreeView;
		MultiColumnHeader m_MultiColumnHeader;
		Rect m_AssetTreeRect = new Rect(0, 0, 0, 0);

		[SerializeField]
		TreeViewState m_TreeViewState = null;

		[SerializeField]
		MultiColumnHeaderState m_MultiColumnHeaderState = null;

		GUIContent m_AssetTreeSettingsContent = null;

		static class Styles
		{
			public static GUIStyle settingsIcon { get { return m_SettingsIcon; } }
			public static GUIStyle convertButton { get { return m_ConvertButton; } }

			static bool m_Init = false;

			static GUIStyle m_SettingsIcon;
			static GUIStyle m_ConvertButton;

			public static void Init()
			{
				if (!m_Init)
					m_Init = true;
				else
					return;

				m_SettingsIcon = GUI.skin.GetStyle("IconButton");
				m_ConvertButton = new GUIStyle(GUI.skin.button);
				m_ConvertButton.margin.bottom += 4;
				m_ConvertButton.margin.top += 4;
			}
		}

		ConversionReadyState m_ConversionReadyState = ConversionReadyState.Ready;

		[MenuItem("Tools/ProBuilder/Repair/Convert to Package Manager")]
		internal static void OpenConversionEditor()
		{
			GetWindow<AssetIdRemapUtility>(true, "Package Manager Conversion Utility", true);
		}

		void OnEnable()
		{
			m_AssetTreeSettingsContent = EditorGUIUtility.IconContent("_Popup");

			if (m_RemapFile == null)
				m_RemapFile = AssetDatabase.LoadAssetAtPath<TextAsset>(k_RemapFileDefaultPath);
#if DEBUG
			if (m_RemapFile == null)
				m_RemapFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ProCore/ProBuilder/Upgrade/AssetIdRemap.json");
#endif

			if (m_TreeViewState == null)
				m_TreeViewState = new TreeViewState();

			if (m_MultiColumnHeaderState == null)
				m_MultiColumnHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
				{
					new MultiColumnHeaderState.Column()
					{
						headerContent = new GUIContent("Asset Store Files to Delete")
					}
				});

			m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState)
			{
				height = 0
			};

			m_AssetsToDeleteTreeView = new AssetTreeView(m_TreeViewState, m_MultiColumnHeader);
			ResetAssetsToDelete();
			m_ConversionReadyState = ValidateSettings();
		}

		void OnGUI()
		{
			Styles.Init();

//			m_RemapFile = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapFile, typeof(TextAsset), false);

			GUILayout.Label("Asset Store Files to Delete", EditorStyles.boldLabel);

			m_AssetTreeRect = GUILayoutUtility.GetRect(position.width, 128, GUILayout.ExpandHeight(true));

			EditorGUI.BeginChangeCheck();

			DrawTreeSettings();

			m_AssetsToDeleteTreeView.OnGUI(m_AssetTreeRect);

			if (EditorGUI.EndChangeCheck())
				m_ConversionReadyState = ValidateSettings();

			if ((m_ConversionReadyState & ConversionReadyState.SerializationError) > 0)
			{
				EditorGUILayout.HelpBox(
					"Cannot update project with binary or mixed serialization.\n\nPlease swith to ForceText serialization to proceed (you may switch back to ForceBinary or Mixed after the conversion process).",
					MessageType.Error);

				SerializationMode serializationMode = EditorSettings.serializationMode;

				EditorGUI.BeginChangeCheck();

				serializationMode = (SerializationMode) EditorGUILayout.EnumPopup("Serialization Mode", serializationMode);

				if (EditorGUI.EndChangeCheck())
				{
					EditorSettings.serializationMode = serializationMode;
					m_ConversionReadyState = ValidateSettings();
				}
			}

			if ((m_ConversionReadyState & ConversionReadyState.AssetStoreDeleteError) > 0)
				EditorGUILayout.HelpBox(
					"Cannot update project without removing ProBuilder/Classes and ProBuilder/Editor directories.", MessageType.Error);

			if ((m_ConversionReadyState & ConversionReadyState.AssetStoreDeleteWarning) > 0)
				EditorGUILayout.HelpBox(
					"Some old ProBuilder files are not marked for deletion. This may cause errors after the conversion process is complete.\n\nTo clear this error use the settings icon to reset the Assets To Delete tree.",
					MessageType.Warning);


			GUI.enabled =
				(m_ConversionReadyState & (ConversionReadyState.AssetStoreDeleteError | ConversionReadyState.SerializationError)) ==
				ConversionReadyState.Ready;

			if (GUILayout.Button("Convert to Package Manager", Styles.convertButton))
			{
				EditorApplication.LockReloadAssemblies();

				Debug.Log("Remove existing asset store install: " + RemoveAssetStoreFiles(m_AssetsToDeleteTreeView.GetRoot()).ToString());

//				if(RemoveAssetStoreFiles(m_AssetsToDeleteTreeView.GetRoot()))
//					RemapAssetIds(m_RemapFile);

				EditorApplication.UnlockReloadAssemblies();
			}

			GUI.enabled = true;
		}

		void DrawTreeSettings()
		{
			Vector2 iconSize = Styles.settingsIcon.CalcSize(m_AssetTreeSettingsContent);

			Rect settingsRect = new Rect(
				position.width - iconSize.x - 4,
				4,
				iconSize.x,
				iconSize.y);

			if (EditorGUI.DropdownButton(settingsRect, m_AssetTreeSettingsContent, FocusType.Passive, Styles.settingsIcon))
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Reset"), false, ResetAssetsToDelete);
				menu.ShowAsContext();
			}
		}

		ConversionReadyState ValidateSettings()
		{
			return ValidateProjectTextSerialized() | ValidateAssetStoreRemoval();
		}

		const int k_DialogOkay = 0;
		const int k_DialogAlt = 1;
		const int k_DialogCancel = 2;

		void ResetAssetsToDelete()
		{
			m_DeprecatedProBuilderFound = PackageImporter.IsPreUpmProBuilderInProject();

			// todo condense the validate calls to a single one
			if (m_DeprecatedProBuilderFound && !ValidateAssetStoreProBuilderRoot(m_DeprecatedProBuilderDirectory) &&
			    !ValidatePreUpmProBuilderRoot(m_DeprecatedProBuilderDirectory))
				m_DeprecatedProBuilderDirectory = FindAssetStoreProBuilderInstall();

			if (m_DeprecatedProBuilderFound &&
				!ValidateAssetStoreProBuilderRoot(m_DeprecatedProBuilderDirectory) &&
			    !ValidatePreUpmProBuilderRoot(m_DeprecatedProBuilderDirectory))
			{
				int res = EditorUtility.DisplayDialogComplex(
					"Could Not Find Existing ProBuilder Directory",
					"Would you like to manually select the Asset Store installed ProBuilder folder and continue with the conversion process, or continue without removing old ProBuilder files (not recommended)?",
					"Select Folder",
					"Continue",
					"Cancel");

				if (res == k_DialogOkay)
				{
					// if they don't get it right after 3 attempts it's probably not going to happen and they'll be annoyed at
					// an inescapable dialog loop
					int userDirectoryPickAttempts = 0;

					while (userDirectoryPickAttempts++ < 3)
					{
						m_DeprecatedProBuilderDirectory = EditorUtility.OpenFolderPanel("Select ProBuilder Directory", "Assets", "");

						if (string.IsNullOrEmpty(m_DeprecatedProBuilderDirectory))
						{
							Debug.LogWarning("Canceling ProBuilder Asset Store to Package Manager conversion. You may start this process again at any time by accessing the Tools>ProBuilder>Repair>Convert to Package Manager menu item.");
							EditorApplication.delayCall += Close;
							break;
						}

						if (ValidateAssetStoreProBuilderRoot(m_DeprecatedProBuilderDirectory) ||
						    ValidatePreUpmProBuilderRoot(m_DeprecatedProBuilderDirectory))
						{
							// got a good directory, continue with process
							break;
						}
						else
						{
							if (!EditorUtility.DisplayDialog("Selected Folder is not a ProBuilder Install",
								"The folder selected not an old version of ProBuilder. Would you like to select a different directory?", "Yes",
								"Cancel"))
							{
								EditorApplication.delayCall += Close;
								break;
							}
						}
					}
				}
				else if (res == k_DialogCancel)
				{
					Debug.LogWarning("Canceling ProBuilder Asset Store to Package Manager conversion. You may start this process again at any time by accessing the Tools>ProBuilder>Repair>Convert to Package Manager menu item.");
					EditorApplication.delayCall += Close;
				}
			}

			m_AssetsToDeleteTreeView.directoryRoot = m_DeprecatedProBuilderDirectory;
			m_AssetsToDeleteTreeView.SetDirectoryIgnorePatterns(k_AssetStoreDirectorySuggestedDeleteIgnoreFilter);
			m_AssetsToDeleteTreeView.SetFileIgnorePatterns(k_AssetStoreSuggestedFileDeleteIgnoreFilter);
			m_AssetsToDeleteTreeView.Reload();
			m_AssetsToDeleteTreeView.ExpandAll();
			m_MultiColumnHeader.ResizeToFit();
			m_ConversionReadyState = ValidateSettings();
		}

		bool RemoveAssetStoreFiles(TreeViewItem root)
		{
			AssetTreeItem node = root as AssetTreeItem;

			// todo only need to call delete on top level node
			// todo need to filter out hidden files (.DS_Store)
			Debug.Log("Removing directory: " + node.fullPath);

			if (node != null && node.enabled)
			{
				return AssetDatabase.MoveAssetToTrash(node.fullPath);
			}
			else if(node.children != null)
			{
				bool success = true;

				foreach (var branch in node.children)
					if (!RemoveAssetStoreFiles(branch))
						success = false;

				return success;
			}

			return false;
		}

		void RemapAssetIds(TextAsset jsonAsset)
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
			string[] matches = Directory.GetDirectories("Assets", "ProBuilder", SearchOption.AllDirectories);
			string bestMatch = null;

			foreach (var match in matches)
			{
				string dir = match.Replace("\\", "/") +  "/";

				if (ValidateAssetStoreProBuilderRoot(dir))
					return dir;

				if (ValidatePreUpmProBuilderRoot(dir))
					bestMatch = dir;
			}

			return bestMatch;
		}

		/// <summary>
		/// Is the ProBuilder folder an Asset Store installed version?
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		static bool ValidateAssetStoreProBuilderRoot(string dir)
		{
			return !string.IsNullOrEmpty(dir) &&
			       File.Exists(dir + "/Classes/ProBuilderCore-Unity5.dll") &&
			       File.Exists(dir + "/Editor/ProBuilderEditor-Unity5.dll");
		}

		/// <summary>
		/// Is the selected folder a ProBuilder version of any install source prior to packman update?
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		static bool ValidatePreUpmProBuilderRoot(string dir)
		{
			bool isProBuilderRoot = !string.IsNullOrEmpty(dir) &&
			       Directory.Exists(dir + "/Classes") &&
			       (File.Exists(dir + "/Classes/ProBuilderCore-Unity5.dll")
			        || File.Exists(dir + "/Classes/ClassesCore/pb_Object.cs")
			        || File.Exists(dir + "/Classes/ProBuilderCore.dll")) &&
			       Directory.Exists(dir + "/Editor") &&
			       (File.Exists(dir + "/Editor/ProBuilderEditor-Unity5.dll")
			        || File.Exists(dir + "/Editor/ProBuilderEditor.dll")
			        || File.Exists(dir + "/Editor/EditorCore/pb_Editor.cs"));

			if (!isProBuilderRoot)
				return false;

			string[] assetIdRemapSource = Directory.GetFiles(dir, "AssetId.cs", SearchOption.AllDirectories);
			string[] assetIdRemapDll = Directory.GetFiles(dir, "AssetIdRemapUtility.dll", SearchOption.AllDirectories);

			// don't let user mark the newly imported upm install for deletion
			return assetIdRemapSource.Length <= 0 && assetIdRemapDll.Length <= 0;
		}

		ConversionReadyState ValidateAssetStoreRemoval()
		{
			ConversionReadyState state = (ConversionReadyState) 0;

			List<AssetTreeItem> assets = m_AssetsToDeleteTreeView.GetAssetList();

			if (assets.Any(x => !x.enabled && k_AssetStoreMustDelete.Any(y => x.fullPath.Contains(y))))
				state |= ConversionReadyState.AssetStoreDeleteError;
			else
				state |= ConversionReadyState.Ready;

			if (assets.Any(x => !x.enabled && k_AssetStoreShouldDelete.Any(y => x.fullPath.Contains(y))))
				state |= ConversionReadyState.AssetStoreDeleteWarning;

			return state;
		}

		static ConversionReadyState ValidateProjectTextSerialized()
		{
			return EditorSettings.serializationMode == SerializationMode.ForceText
				? ConversionReadyState.Ready
				: ConversionReadyState.SerializationError;
		}
	}
}
