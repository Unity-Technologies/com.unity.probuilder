#if DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;
using UnityEditor.IMGUI.Controls;

namespace ProBuilder.AssetUtility
{
	/// <summary>
	/// Utility class for creating GUID remap files.
	/// </summary>
	class AssetIdRemapFileEditor : EditorWindow
	{
		const string k_RemapFilePath = "Upgrade/AssetIdRemap.json";
		const string k_NamespaceRemapFilePath = "Upgrade/NamespaceRemap.json";

		static string remapFilePath
		{
			get { return "Assets/ProCore/ProBuilder/" + k_RemapFilePath; }
		}

		static string namespaceRemapFilePath
		{
			get { return "Assets/ProCore/ProBuilder/" + k_NamespaceRemapFilePath; }
		}

		static readonly string[] k_DirectoryExcludeFilter = new string[]
		{
			"ProBuilder/About",
			"ProBuilder/AssetIdRemapUtility",
			"ProBuilder/API Examples",
			"ProBuilder/Data",
			"ProBuilder/Icons",
			"ProBuilder/Material",
			"ProBuilder/Upgrade",
		};

		static TextAsset m_RemapObject = null;
		static TextAsset m_NamespaceRemap = null;
		static bool m_DoClean = false;
		static GUIContent m_DoCleanGuiContent = new GUIContent("Clean", "If enabled both Source and Destination actions" +
		                                                                " will clear the remap file and start from scratch.");

		static GUIContent m_SourceGuiContent = new GUIContent("Source", "The old GUID and FileId.");
		static GUIContent m_DestinationGuiContent = new GUIContent("Destination", "The new GUID and FileId.");

		[SerializeField] TreeViewState m_TreeViewState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		MultiColumnHeader m_MultiColumnHeader;
		AssetIdRemapTreeView m_TreeView;
		SearchField m_SearchField;

		[MenuItem("Tools/GUID Remap Editor")]
		static void MenuOpenGuidEditor()
		{
			GetWindow<AssetIdRemapFileEditor>(true, "GUID Remap Editor", true);
		}

		void OnEnable()
		{
			// Check whether there is already a serialized view state (state
			// that survived assembly reloading)
			if (m_TreeViewState == null)
				m_TreeViewState = new TreeViewState();

			if(m_MultiColumnHeaderState == null)
				m_MultiColumnHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
				{
					new MultiColumnHeaderState.Column() {
						headerContent = m_SourceGuiContent,
						autoResize = true
						},
					new MultiColumnHeaderState.Column() {
						headerContent = m_DestinationGuiContent,
						autoResize = true
						},
				});

			m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState);
			m_MultiColumnHeader.ResizeToFit();
			m_TreeView = new AssetIdRemapTreeView(m_TreeViewState, m_MultiColumnHeader);
			m_TreeView.remapObject = GetGuidRemapObject(
				m_RemapObject == null ? remapFilePath : AssetDatabase.GetAssetPath(m_RemapObject),
				m_NamespaceRemap == null ? namespaceRemapFilePath : AssetDatabase.GetAssetPath(m_NamespaceRemap));
			m_TreeView.Reload();

			m_SearchField = new SearchField();
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			m_RemapObject = (TextAsset) EditorGUILayout.ObjectField("Remap", m_RemapObject, typeof(TextAsset), false);
			m_NamespaceRemap = (TextAsset) EditorGUILayout.ObjectField("Namespace", m_NamespaceRemap, typeof(TextAsset), false);
			m_DoClean = EditorGUILayout.Toggle(m_DoCleanGuiContent, m_DoClean);

			if (GUILayout.Button("Collect Source (Old) Asset Identifiers"))
			{
				GetRemapSource(
					m_RemapObject == null ? remapFilePath : AssetDatabase.GetAssetPath(m_RemapObject),
					m_NamespaceRemap == null ? namespaceRemapFilePath : AssetDatabase.GetAssetPath(m_NamespaceRemap),
					m_DoClean);
			}

			if (GUILayout.Button("Collect Destination (New) Asset Identifiers"))
			{
				GetRemapDestination(
					m_RemapObject == null ? remapFilePath : AssetDatabase.GetAssetPath(m_RemapObject),
					m_NamespaceRemap == null ? namespaceRemapFilePath : AssetDatabase.GetAssetPath(m_NamespaceRemap),
					m_DoClean);
			}

			if (EditorGUI.EndChangeCheck())
			{
				m_TreeView.remapObject = GetGuidRemapObject(
					m_RemapObject == null ? remapFilePath : AssetDatabase.GetAssetPath(m_RemapObject),
					m_NamespaceRemap == null ? namespaceRemapFilePath : AssetDatabase.GetAssetPath(m_NamespaceRemap));
				m_TreeView.Reload();
				Repaint();
			}

			Rect last = GUILayoutUtility.GetLastRect();

			m_TreeView.searchString = m_SearchField.OnGUI(new Rect(last.x, last.y + last.height + 4, position.width - last.x * 2f, 20f),
				m_TreeView.searchString);

			Vector2 treeStart = new Vector2(last.x, last.y + last.height + 4 + 20f + 4f);

			m_TreeView.SetRowHeight();

			m_TreeView.OnGUI(new Rect(treeStart.x, treeStart.y, position.width - treeStart.x * 2, position.height - treeStart.y));
		}

		[MenuItem("Assets/GUID Remap Utility/Collect Old GUIDs")]
		static void GetRemapSource()
		{
			GetRemapSource(null, null);
		}

		[MenuItem("Assets/GUID Remap Utility/Collect New GUIDs")]
		static void MenuGetRemapDestination()
		{
			GetRemapSource(null, null);
		}

		static void GetRemapSource(string guidMapPath, string namespaceMapPath = null, bool clean = false)
		{
			if (string.IsNullOrEmpty(guidMapPath))
				guidMapPath = remapFilePath;

			if (string.IsNullOrEmpty(namespaceMapPath))
				namespaceMapPath = namespaceRemapFilePath;

			var remapObject = GetGuidRemapObject(guidMapPath, namespaceMapPath, clean);

			string localDirectory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
			if(!remapObject.sourceDirectory.Contains(localDirectory))
				remapObject.sourceDirectory.Add(localDirectory);
			List<AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiersInDirectory(GetSelectedDirectory(), k_DirectoryExcludeFilter))
			{
				if (map.Any(x => x.source.Equals(id)))
					continue;

				id.SetPathRelativeTo(localDirectory);

				// the only time where a destination can exist with a null source is when a single destination is in the
				// map, so it's okay to grab the first and not bother searching for more dangling destination entries
				AssetIdentifierTuple matchingDestination =
					map.FirstOrDefault(x =>
					{
						return x.destination != null &&
						       x.destination.AssetEquals(id, remapObject.namespaceMap);
					});

				if (matchingDestination != null)
				{
					if (matchingDestination.source != null)
						map.Add(new AssetIdentifierTuple(id, matchingDestination.destination));
					else
						matchingDestination.source = id;
				}
				else
				{
					map.Add(new AssetIdentifierTuple(id, null));
				}
			}

			File.WriteAllText(guidMapPath, JsonUtility.ToJson(remapObject, true));
			AssetDatabase.ImportAsset(guidMapPath);
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(guidMapPath));
		}

		static void GetRemapDestination(string guidMapPath, string namespaceMapPath = null, bool clean = false)
		{
			if (string.IsNullOrEmpty(guidMapPath))
				guidMapPath = remapFilePath;

			if (string.IsNullOrEmpty(namespaceMapPath))
				namespaceMapPath = namespaceRemapFilePath;

			AssetIdRemapObject remapObject = GetGuidRemapObject(guidMapPath, namespaceMapPath, clean);

			if (!string.IsNullOrEmpty(remapObject.destinationDirectory))
			{
				if (!EditorUtility.DisplayDialog("Destination Directory Already Mapped",
					"The destination directory has already been mapped. Continuing will overwrite the existing data. Are you sure you wish to continue?",
					"Continue", "Cancel"))
					return;
			}

			string localDirectory = GetSelectedDirectory().Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
			remapObject.destinationDirectory = localDirectory;
			List<AssetIdentifierTuple> map = remapObject.map;

			foreach (var id in GetAssetIdentifiersInDirectory(GetSelectedDirectory(), k_DirectoryExcludeFilter))
			{
				if (map.Any(x => x.destination.Equals(id)))
					continue;

				id.SetPathRelativeTo(localDirectory);

				IEnumerable<AssetIdentifierTuple> matchingSources =
					map.Where(x => x.source != null && x.source.AssetEquals(id, remapObject.namespaceMap));

				if (matchingSources.Any())
				{
					foreach (var tup in matchingSources)
						tup.destination = id;
				}
				else
				{
					map.Add(new AssetIdentifierTuple(null, id));
				}
			}

			File.WriteAllText(guidMapPath, JsonUtility.ToJson(remapObject, true));
			AssetDatabase.ImportAsset(guidMapPath);
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(guidMapPath));
		}

		/// <summary>
		/// Collect asset identifier information from all files in a directory.
		/// </summary>
		/// <param name="directory"></param>
		static List<AssetId> GetAssetIdentifiersInDirectory(string directory, string[] directoryIgnoreFilter = null)
		{
			List<AssetId> ids = new List<AssetId>();

			string unixPath = directory.Replace("\\", "/");

			if (directoryIgnoreFilter != null && directoryIgnoreFilter.Any(x => unixPath.Contains(x)))
				return ids;

			foreach (string file in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
			{
				if (file.EndsWith(".meta") || Path.GetFileName(file).StartsWith("."))
					continue;

				string localPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");
				ids.AddRange(GetAssetIdentifiers(localPath));
			}

			foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetDirectoryName(dir).StartsWith("."))
					continue;

				ids.AddRange(GetAssetIdentifiersInDirectory(dir, directoryIgnoreFilter));
			}

			return ids;
		}

		static List<AssetId> GetAssetIdentifiers(string assetPath)
		{
			List<AssetId> ids = new List<AssetId>();

			if (assetPath.EndsWith(".unity"))
				return ids;

			foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
			{
				string g;
				int file;

				if (AssetDatabase.GetGUIDAndLocalFileIdentifier(o.GetInstanceID(), out g, out file))
					ids.Add(new AssetId(o, file.ToString(), g.ToString(), assetPath));
			}

			return ids;
		}

		/// <summary>
		/// Load a remap json file from a relative path (Assets/MyRemapFile.json).
		/// </summary>
		/// <returns>A GuidRemapObject from the path, or if not found, a new GuidRemapObject</returns>
		static AssetIdRemapObject GetGuidRemapObject(string path, string namespacePath = null, bool clean = false)
		{
			AssetIdRemapObject remap = new AssetIdRemapObject();

			TextAsset o = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

			if (clean || o == null)
			{
				TextAsset namespaceRemapJson = AssetDatabase.LoadAssetAtPath<TextAsset>(namespaceRemapFilePath);

				if (namespaceRemapJson != null)
					remap.namespaceMap = JsonUtility.FromJson<NamespaceRemapObject>(namespaceRemapJson.text);
			}
			else
			{
				JsonUtility.FromJsonOverwrite(o.text, remap);
			}

			return remap;
		}

		static string GetSelectedDirectory()
		{
			UObject o = Selection.activeObject;

			if (o != null)
			{
				string path = AssetDatabase.GetAssetPath(o.GetInstanceID());

				if (!string.IsNullOrEmpty(path))
				{
					if (Directory.Exists(path))
						return Path.GetFullPath(path);

					string res = Path.GetDirectoryName(path);

					if (!string.IsNullOrEmpty(res) && System.IO.Directory.Exists(res))
						return Path.GetFullPath(res);
				}
			}

			return Path.GetFullPath("Assets");
		}
	}
}

#endif
