#if PROBUILDER_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;
using UnityEditor.IMGUI.Controls;

namespace UnityEngine.ProBuilder.AssetIdRemapUtility
{
    /// <summary>
    /// Utility class for creating GUID remap files.
    /// </summary>
    sealed class AssetIdRemapBuilderEditor : EditorWindow
    {
        const string k_RemapFilePath = "AssetIdRemap.json";
        const string k_NamespaceRemapFilePath = "NamespaceRemap.json";

        static string remapFilePath
        {
            get { return "Assets/" + k_RemapFilePath; }
        }

        static string namespaceRemapFilePath
        {
            get { return "Assets/" + k_NamespaceRemapFilePath; }
        }

        static readonly string[] k_DirectoryExcludeFilter = new string[]
        {
            "ProBuilder/About",
            "ProBuilder/AssetIdRemapUtility",
            "ProBuilder/API Examples",
            "ProBuilder/Data",
            "ProBuilder/Icons",
            "ProBuilder/Upgrade",
        };

        static GUIContent m_SourceGuiContent = new GUIContent("Source", "The old GUID and FileId.");
        static GUIContent m_DestinationGuiContent = new GUIContent("Destination", "The new GUID and FileId.");

        [SerializeField] TextAsset m_RemapTextAsset = null;
        [SerializeField] TextAsset m_NamespaceRemapTextAsset = null;
        [SerializeField] string m_SourceDirectory;
        [SerializeField] string m_DestinationDirectory;

        [SerializeField] TreeViewState m_TreeViewState;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        [SerializeField] bool m_DetailsExpanded;

        MultiColumnHeader m_MultiColumnHeader;
        AssetIdRemapBuilderTreeView m_TreeView;
        SearchField m_SearchField;

        [MenuItem("Tools/GUID Remap Editor")]
        static void MenuOpenGuidEditor()
        {
            var win = GetWindow<AssetIdRemapBuilderEditor>(true, "GUID Remap Editor", true);
            win.m_DetailsExpanded = true;
        }

        static class Styles
        {
            static GUIStyle m_Container = null;

            public static GUIStyle container
            {
                get
                {
                    if (m_Container == null)
                    {
                        m_Container = new GUIStyle(EditorStyles.helpBox);
                        m_Container.padding = new RectOffset(4, 4, 4, 4);
                    }
                    return m_Container;
                }
            }
        }

        void OnEnable()
        {
            // Check whether there is already a serialized view state (state
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            if (m_MultiColumnHeaderState == null)
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
            m_TreeView = new AssetIdRemapBuilderTreeView(m_TreeViewState, m_MultiColumnHeader);
            m_TreeView.remapObject = GetGuidRemapObject();
            m_TreeView.Reload();

            m_SearchField = new SearchField();
        }

        void OnDestroy()
        {
            if (m_TreeView.isDirty)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes", "There are unsaved changes to the remap file. Save these changes?", "Save", "Discard"))
                    Save();
            }
        }

        string GetRemapFilePath()
        {
            if (m_RemapTextAsset != null)
                return AssetDatabase.GetAssetPath(m_RemapTextAsset);
            return remapFilePath;
        }

        void Save()
        {
            File.WriteAllText(GetRemapFilePath(), JsonUtility.ToJson(m_TreeView.remapObject, true));
            AssetDatabase.ImportAsset(GetRemapFilePath());
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(GetRemapFilePath()));
            m_TreeView.isDirty = false;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(m_DetailsExpanded ? "Hide" : "Show", EditorStyles.toolbarButton))
                m_DetailsExpanded = !m_DetailsExpanded;

            GUILayout.FlexibleSpace();

            GUI.enabled = m_TreeView.isDirty;

            if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
            {
                m_TreeView.remapObject = null;
                m_TreeView.remapObject = GetGuidRemapObject();
                m_TreeView.Reload();
                m_TreeView.isDirty = false;
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                Save();

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            if (m_DetailsExpanded)
            {
                EditorGUI.BeginChangeCheck();

                m_RemapTextAsset = (TextAsset)EditorGUILayout.ObjectField("Remap", m_RemapTextAsset, typeof(TextAsset), false);
                m_NamespaceRemapTextAsset = (TextAsset)EditorGUILayout.ObjectField("Namespace", m_NamespaceRemapTextAsset, typeof(TextAsset), false);

                if (EditorGUI.EndChangeCheck())
                {
                    m_TreeView.remapObject = null;
                    m_TreeView.remapObject = GetGuidRemapObject();
                    m_TreeView.Reload();
                    Repaint();
                }

                EditorGUILayout.BeginVertical(Styles.container);
                GUILayout.Label("Package Directories", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(Styles.container);
                m_SourceDirectory = DoDirectoryField("Source", m_SourceDirectory);

                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button("Collect Source (Old) Asset Identifiers"))
                    GetRemapSource(m_SourceDirectory);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(Styles.container);
                m_DestinationDirectory = DoDirectoryField("Destination", m_DestinationDirectory);

                if (GUILayout.Button("Collect Destination (New) Asset Identifiers"))
                    GetRemapDestination(m_DestinationDirectory);

                if (EditorGUI.EndChangeCheck())
                    m_TreeView.Reload();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Asset Id Mapping", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("..."))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Clear Source", ""), false, () =>
                    {
                        GetGuidRemapObject().Clear(Origin.Source);
                        m_TreeView.Reload();
                        m_TreeView.isDirty = true;
                    });

                menu.AddItem(new GUIContent("Clear Destination", ""), false, () =>
                    {
                        GetGuidRemapObject().Clear(Origin.Destination);
                        m_TreeView.Reload();
                        m_TreeView.isDirty = true;
                    });

                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();

            Rect last = GUILayoutUtility.GetLastRect();

            m_TreeView.searchString = m_SearchField.OnGUI(new Rect(last.x, last.y + last.height + 4, position.width - last.x * 2f, 20f),
                    m_TreeView.searchString);

            Vector2 treeStart = new Vector2(last.x, last.y + last.height + 4 + 20f + 4f);

            m_TreeView.SetRowHeight();

            m_TreeView.OnGUI(new Rect(treeStart.x, treeStart.y, position.width - treeStart.x * 2, position.height - treeStart.y));
        }

        static string DoDirectoryField(string title, string value)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            value = EditorGUILayout.TextField(title, value);

            if (GUILayout.Button("Select", GUILayout.MaxWidth(60)))
                value = GetSelectedDirectory();

            bool didChange = EditorGUI.EndChangeCheck();
            bool doOpenFolderPanel = GUILayout.Button("...", GUILayout.MaxWidth(32));

            GUILayout.EndHorizontal();

            if (doOpenFolderPanel)
            {
                value = EditorUtility.OpenFolderPanel(title, value, "");
                didChange = true;
            }

            return didChange ? value = value.Replace("\\", "/").Replace(Application.dataPath, "Assets") : value;
        }

        void GetRemapSource(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                Debug.LogWarning("No source directory selected.");
                return;
            }

            var remapObject = GetGuidRemapObject();

            string localDirectory = directory.Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";

            if (!remapObject.sourceDirectory.Contains(localDirectory))
                remapObject.sourceDirectory.Add(localDirectory);

            List<AssetIdentifierTuple> map = remapObject.map;

            foreach (var id in GetAssetIdentifiersInDirectory(localDirectory, k_DirectoryExcludeFilter))
            {
                id.SetPathRelativeTo(localDirectory);

                if (map.Any(x => x.source != null && x.source.Equals(id)))
                    continue;

                // the only time where a destination can exist with a null source is when a single destination is in the
                // map, so it's okay to grab the first and not bother searching for more dangling destination entries
                AssetIdentifierTuple matchingDestination =
                    map.FirstOrDefault(x =>
                    {
                        return x.destination != null &&
                        x.destination.AssetEquals(id);
                    });

                if (matchingDestination != null)
                {
                    if (AssetId.IsValid(matchingDestination.source))
                        map.Add(new AssetIdentifierTuple(id, matchingDestination.destination));
                    else
                        matchingDestination.source = id;
                }
                else
                {
                    map.Add(new AssetIdentifierTuple(id, null));
                }
            }

            m_TreeView.isDirty = true;
        }

        void GetRemapDestination(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                Debug.LogWarning("No destination directory selected.");
                return;
            }

            var remapObject = GetGuidRemapObject();

            if (!string.IsNullOrEmpty(remapObject.destinationDirectory))
            {
                if (!EditorUtility.DisplayDialog("Destination Directory Already Mapped",
                        "The destination directory has already been mapped. Continuing will overwrite the existing data. Are you sure you wish to continue?",
                        "Continue", "Cancel"))
                    return;
            }

            string localDirectory = directory.Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/";
            remapObject.destinationDirectory = localDirectory;
            List<AssetIdentifierTuple> map = remapObject.map;

            foreach (var id in GetAssetIdentifiersInDirectory(localDirectory, k_DirectoryExcludeFilter))
            {
                if (map.Any(x => x.destination.Equals(id)))
                    continue;

                id.SetPathRelativeTo(localDirectory);

                IEnumerable<AssetIdentifierTuple> matchingSources =
                    map.Where(x => x.source != null && x.source.AssetEquals(id));

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

            m_TreeView.isDirty = true;
        }

        /// <summary>
        /// Collect asset identifier information from all files in a directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="directoryIgnoreFilter"></param>
        static List<AssetId> GetAssetIdentifiersInDirectory(string directory, string[] directoryIgnoreFilter)
        {
            List<AssetId> ids = new List<AssetId>();
            string unixPath = directory.Replace("\\", "/");
            string packageAbsolutePath = null;
            string packageRelativePath = null;

            if (directory.StartsWith("Packages/"))
            {
                // +1 because we want the trailing "/" to be with the packageRelativePath
                var en = unixPath.IndexOf("/", "Packages/".Length, StringComparison.InvariantCulture) + 1;
                packageRelativePath = unixPath.Substring(0, en);
                var targetPathRelative = unixPath.Substring(en, unixPath.Length - en);
                packageAbsolutePath = Path.GetFullPath(unixPath).Replace("\\", "/");
                if (!string.IsNullOrEmpty(targetPathRelative))
                    packageAbsolutePath = packageAbsolutePath.Replace(targetPathRelative, "");
            }

            if (directoryIgnoreFilter != null && directoryIgnoreFilter.Any(x => unixPath.Contains(x)))
                return ids;

            foreach (string file in Directory.GetFiles(Path.GetFullPath(directory), "*", SearchOption.TopDirectoryOnly))
            {
                if (file.EndsWith(".meta") || Path.GetFileName(file).StartsWith("."))
                    continue;

                string localPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");

                if (!string.IsNullOrEmpty(packageAbsolutePath))
                    localPath = localPath.Replace(packageAbsolutePath, packageRelativePath);

                ids.AddRange(GetAssetIdentifiers(localPath));
            }

            foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetDirectoryName(dir).StartsWith("."))
                    continue;

                string path = !string.IsNullOrEmpty(packageAbsolutePath)
                    ? dir.Replace("\\", "/").Replace(packageAbsolutePath, packageRelativePath)
                    : dir;

                ids.AddRange(GetAssetIdentifiersInDirectory(path, directoryIgnoreFilter));
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
                long file;

                if (o != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out g, out file))
                    ids.Add(new AssetId(o, file.ToString(), g.ToString(), assetPath));
            }

            return ids;
        }

        /// <summary>
        /// Load a remap json file from a relative path (Assets/MyRemapFile.json).
        /// </summary>
        /// <returns>A GuidRemapObject from the path, or if not found, a new GuidRemapObject</returns>
        AssetIdRemapObject GetGuidRemapObject()
        {
            var remapObject = m_TreeView.remapObject;

            if (remapObject != null)
                return remapObject;

            if (m_RemapTextAsset == null)
                m_RemapTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(remapFilePath);

            remapObject = new AssetIdRemapObject();

            if (m_RemapTextAsset != null)
                JsonUtility.FromJsonOverwrite(m_RemapTextAsset.text, remapObject);

            return remapObject;
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
