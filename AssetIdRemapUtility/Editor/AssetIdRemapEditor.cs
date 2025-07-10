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

namespace UnityEngine.ProBuilder.AssetIdRemapUtility
{
    sealed class AssetIdRemapEditor : EditorWindow
    {
        const string k_ProBuilder2DllName = "ProBuilderCore-Unity5.dll";
        const string k_ProBuilder3DllName = "ProBuilderCore.dll";
        const string k_ConversionLogPath = "Temp/ProBuilderConversionLog.txt";

        static readonly string[] k_RemapFilePaths = new string[]
        {
            "Packages/com.unity.probuilder/Content/Upgrade/AssetIdRemap-4_0_0.json"
        };

        static readonly string[] k_AssetExtensionsToRemap = new string[]
        {
            "*.meta",
            "*.unity",
            "*.asset",
            "*.anim",
            "*.animset",
            "*.blendtree",
            "*.buildreport",
            "*.colors",
            "*.controller",
            "*.cubemap",
            "*.curves",
            "*.curvesNormalized",
            "*.flare",
            "*.fontsettings",
            "*.giparams",
            "*.gradients",
            "*.guiskin",
            "*.ht",
            "*.mask",
            "*.mat",
            "*.mesh",
            "*.mixer",
            "*.overrideController",
            "*.particleCurves",
            "*.particleCurvesSigned",
            "*.particleDoubleCurves",
            "*.particleDoubleCurvesSigned",
            "*.physicMaterial",
            "*.physicsMaterial2D",
            "*.playable",
            "*.prefab",
            "*.preset",
            "*.renderTexture",
            "*.shadervariants",
            "*.spriteatlas",
            "*.state",
            "*.statemachine",
            "*.texture2D",
            "*.transition",
            "*.webCamTexture",
        };

        static readonly string[] k_AssetStoreDirectorySuggestedDeleteIgnoreFilter = new string[]
        {
            "(^|(?<=/))Data(/|)$",
            "(^|(?<=/))ProBuilderMeshCache(/|)$",
            ".meta$",
            "^\\."
        };

        static readonly string[] k_AssetStoreSuggestedFileDeleteIgnoreFilter = new string[]
        {
            ".meta$",
            ".asset$",
            "^\\."
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
            NoActionRequired = 1 << 0,
            SerializationError = 1 << 1,
            AssetStoreDeleteError = 1 << 2,
            AssetStoreDeleteWarning = 1 << 3,
            AssetStoreInstallFound = 1 << 4,
            DeprecatedAssetIdsFound = 1 << 5,
            MissingRemapFile = 1 << 6,
            ConversionRan = 1 << 7
        };

        TextAsset m_RemapFile = null;

        [SerializeField]
        string m_DeprecatedProBuilderDirectory;
        [SerializeField]
        bool m_DeprecatedProBuilderFound;
        [SerializeField]
        ConversionReadyState m_ConversionReadyState = ConversionReadyState.Ready;

        string m_ConversionLog;

        AssetTreeView m_AssetsToDeleteTreeView;
        MultiColumnHeader m_MultiColumnHeader;
        Rect m_AssetTreeRect = new Rect(0, 0, 0, 0);
#pragma warning disable CS0618 // Type or member is obsolete
        [SerializeField]
        TreeViewState m_TreeViewState = null;
#pragma warning restore CS0618
        [SerializeField]
        MultiColumnHeaderState m_MultiColumnHeaderState = null;
        GUIContent m_AssetTreeSettingsContent = null;
        Vector2 m_ConversionLogScroll = Vector2.zero;

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
                m_ConvertButton = new GUIStyle(GUI.skin.GetStyle("AC Button"));
                m_ConvertButton.margin.bottom += 4;
            }
        }

        internal static void OpenConversionEditor()
        {
            GetWindow<AssetIdRemapEditor>(true, "ProBuilder Update Utility", true);
        }

        void OnEnable()
        {
            m_AssetTreeSettingsContent = EditorGUIUtility.IconContent("_Popup");

            for (int i = 0, c = k_RemapFilePaths.Length; m_RemapFile == null && i < c; i++)
                m_RemapFile = AssetDatabase.LoadAssetAtPath<TextAsset>(k_RemapFilePaths[i]);

            if (m_RemapFile == null)
            {
                Debug.LogWarning("Could not find a valid asset id remap file!");
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();
#pragma warning restore CS0618

            if (m_MultiColumnHeaderState == null)
                m_MultiColumnHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Obsolete Files to Delete")
                    }
                });

            m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState) { height = 0 };
            m_AssetsToDeleteTreeView = new AssetTreeView(m_TreeViewState, m_MultiColumnHeader);

            ResetAssetsToDelete();

            try
            {
                if (File.Exists(k_ConversionLogPath))
                    m_ConversionLog = File.ReadAllText(k_ConversionLogPath);
                else
                    m_ConversionLog = null;
            }
            catch
            {
                m_ConversionLog = null;
            }

            // if the project just contains deprecated guids, and is already in text serialization mode, we can skip the dialog
            // and just run the conversion immediately.
            if (m_ConversionReadyState == (ConversionReadyState.Ready | ConversionReadyState.DeprecatedAssetIdsFound))
                DoConversion();
        }

        void OnGUI()
        {
            Styles.Init();

            if (m_ConversionReadyState == ConversionReadyState.NoActionRequired ||
                m_ConversionReadyState == ConversionReadyState.ConversionRan)
            {
                if (!string.IsNullOrEmpty(m_ConversionLog))
                {
                    m_ConversionLogScroll = EditorGUILayout.BeginScrollView(m_ConversionLogScroll);
                    GUILayout.Label(m_ConversionLog, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    GUI.Label(new Rect(0, 0, position.width, position.height), "ProBuilder is up to date!",
                        EditorStyles.centeredGreyMiniLabel);

                    if (Event.current.type == EventType.ContextClick)
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Find and replace deprecated Asset IDs"), false, () =>
                            {
                                var log = new StringBuilder();
                                RemapAssetIds(log);
                                Debug.Log(log);
                            });
                        menu.ShowAsContext();
                    }
                }
                return;
            }
            else if ((m_ConversionReadyState & ConversionReadyState.AssetStoreInstallFound) > 0)
            {
                GUILayout.Label("Obsolete Files to Delete", EditorStyles.boldLabel);

                m_AssetTreeRect = GUILayoutUtility.GetRect(position.width, 128, GUILayout.ExpandHeight(true));

                EditorGUI.BeginChangeCheck();

                DrawTreeSettings();

                m_AssetsToDeleteTreeView.OnGUI(m_AssetTreeRect);

                if (EditorGUI.EndChangeCheck())
                    m_ConversionReadyState = GetReadyState();
            }
            else if ((m_ConversionReadyState & ConversionReadyState.DeprecatedAssetIdsFound) > 0)
            {
                var deprecatedIdsRect = GUILayoutUtility.GetRect(position.width, 32, GUILayout.ExpandHeight(true));
                GUI.Label(deprecatedIdsRect, "References to old ProBuilder install found.\n\nProject is ready for conversion.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            if ((m_ConversionReadyState & ConversionReadyState.SerializationError) > 0)
            {
                EditorGUILayout.HelpBox(
                    "Cannot update project with binary or mixed serialization.\n\nPlease swith to ForceText serialization to proceed (you may switch back to ForceBinary or Mixed after the conversion process).",
                    MessageType.Error);

                SerializationMode serializationMode = EditorSettings.serializationMode;

                EditorGUI.BeginChangeCheck();

                serializationMode = (SerializationMode)EditorGUILayout.EnumPopup("Serialization Mode", serializationMode);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorSettings.serializationMode = serializationMode;
                    m_ConversionReadyState = GetReadyState();
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

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Convert to ProBuilder 4", Styles.convertButton))
            {
                DoConversion();
                GUIUtility.ExitGUI();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
            GUI.enabled = true;
        }

        void DoConversion()
        {
            var log = new StringBuilder();

            try
            {
                // Set serialization mode to mixed then back to force-text to reserialize any assets in binary form
                // that may have somehow persisted (version control makes a state where "Force Text" is enabled with
                // some existing binary assets persisting possible). this happened during testing.
                // mixed doesn't refresh anything, but setting to ForceText re-iterates all assets and double-checks
                // that they are in the correct format.
                EditorSettings.serializationMode = SerializationMode.Mixed;
                EditorSettings.serializationMode = SerializationMode.ForceText;

                EditorApplication.LockReloadAssemblies();

                log.AppendLine("ProBuilder 4 Conversion Log");
                log.AppendLine("");

                if ((m_ConversionReadyState & ConversionReadyState.AssetStoreInstallFound) == ConversionReadyState.AssetStoreInstallFound)
                {
                    log.AppendLine("Removing existing ProBuilder files...");

                    if (RemoveAssetStoreFiles(m_AssetsToDeleteTreeView.GetRoot(), log))
                    {
                        log.AppendLine("\nRemapping Asset Ids...");
                        RemapAssetIds(log);
                    }
                }
                else
                {
                    log.AppendLine("Remapping Asset Ids...");
                    RemapAssetIds(log);
                }
            }
            finally
            {
                m_ConversionLog = log.ToString();

                try
                {
                    Directory.CreateDirectory("Temp");
                    File.WriteAllText(k_ConversionLogPath, m_ConversionLog);
                }
                catch
                {
                    Debug.Log(m_ConversionLog);
                }

                EditorApplication.UnlockReloadAssemblies();
                m_ConversionReadyState = ConversionReadyState.ConversionRan;
                EditorApplication.delayCall += AssetDatabase.Refresh;
            }
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

        ConversionReadyState GetReadyState()
        {
            var state = ConversionReadyState.Ready;

            if (ProjectContainsOldAssetIds())
                state |= ConversionReadyState.DeprecatedAssetIdsFound;

            if (PackageImporter.IsPreProBuilder4InProject())
                state |= ConversionReadyState.AssetStoreInstallFound;

            if (state == ConversionReadyState.Ready && PackageImporter.IsProBuilder4OrGreaterLoaded())
                return ConversionReadyState.NoActionRequired;

            state |= ValidateProjectTextSerialized();
            state |= ValidateAssetStoreRemoval();

            return state;
        }

        const int k_DialogOkay = 0;
        const int k_DialogAlt = 1;
        const int k_DialogCancel = 2;

        void ResetAssetsToDelete()
        {
            m_DeprecatedProBuilderFound = PackageImporter.IsPreProBuilder4InProject();

            if (m_DeprecatedProBuilderFound && !ValidateAssetStoreProBuilderRoot(m_DeprecatedProBuilderDirectory))
                m_DeprecatedProBuilderDirectory = FindAssetStoreProBuilderInstall();

            // If still no old folder found (and PackageImporter tells us one exists), ask the user to point it out
            if (m_DeprecatedProBuilderFound && !ValidateAssetStoreProBuilderRoot(m_DeprecatedProBuilderDirectory))
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
                            UnityEngine.Debug.LogWarning("Canceling ProBuilder Asset Store to Package Manager conversion. You may start this process again at any time by accessing the 'Tools>ProBuilder>Repair>Convert to ProBuilder 4' menu item.");
                            EditorApplication.delayCall += Close;
                            break;
                        }

                        if (ValidateAssetStoreProBuilderRoot(m_DeprecatedProBuilderDirectory))
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
                    UnityEngine.Debug.LogWarning("Canceling ProBuilder Asset Store to Package Manager conversion. You may start this process again at any time by accessing the 'Tools>ProBuilder>Repair>Convert to ProBuilder 4' menu item.");
                    EditorApplication.delayCall += Close;
                }
            }

            m_AssetsToDeleteTreeView.directoryRoot = m_DeprecatedProBuilderDirectory;
            m_AssetsToDeleteTreeView.SetDirectoryIgnorePatterns(k_AssetStoreDirectorySuggestedDeleteIgnoreFilter);
            m_AssetsToDeleteTreeView.SetFileIgnorePatterns(k_AssetStoreSuggestedFileDeleteIgnoreFilter);
            m_AssetsToDeleteTreeView.Reload();
            m_AssetsToDeleteTreeView.ExpandAll();
            m_MultiColumnHeader.ResizeToFit();
            m_ConversionReadyState = GetReadyState();
        }

#pragma warning disable CS0618 // Type or member is obsolete
        bool RemoveAssetStoreFiles(TreeViewItem root, StringBuilder log)
#pragma warning restore CS0618
        {
            AssetTreeItem node = root as AssetTreeItem;

            if (node != null && (node.enabled && !node.isMixedState))
            {
                if (!AssetDatabase.MoveAssetToTrash(node.fullPath))
                {
                    if (!AssetDatabase.DeleteAsset(node.fullPath))
                    {
                        if (Directory.Exists(node.fullPath))
                        {
                            Directory.Delete(node.fullPath, true);

                            if (Directory.Exists(node.fullPath))
                            {
                                Debug.LogError("Directory.Delete failed, giving up. (" + node.fullPath + ")");
                                return false;
                            }
                            else
                            {
                                File.Delete(node.fullPath.Trim('/') + ".meta");
                            }
                        }
                        else if (File.Exists(node.fullPath))
                        {
                            File.Delete(node.fullPath);

                            if (File.Exists(node.fullPath))
                            {
                                Debug.LogError("File.Delete failed, giving up (" + node.fullPath + ")");
                                return true;
                            }
                            else
                            {
                                File.Delete(node.fullPath + ".meta");
                            }
                        }
                    }
                }

                log.AppendLine("  - " + node.fullPath);

                return true;
            }

            if (node.children != null)
            {
                bool success = true;

                foreach (var branch in node.children)
                    if (!RemoveAssetStoreFiles(branch, log))
                        success = false;

                return success;
            }

            return true;
        }

        void RemapAssetIds(StringBuilder log)
        {
            AssetIdRemapObject remapObject = new AssetIdRemapObject();
            JsonUtility.FromJsonOverwrite(m_RemapFile.text, remapObject);

            int remappedReferences = 0;
            int modifiedFiles = 0;
            string[] assets = k_AssetExtensionsToRemap.SelectMany(x => Directory.GetFiles("Assets", x, SearchOption.AllDirectories)).ToArray();
            var failures = new StringBuilder();
            var successes = new StringBuilder();
            int failCount = 0;

            for (int i = 0, c = assets.Length; i < c; i++)
            {
                EditorUtility.DisplayProgressBar("Asset Id Remap", assets[i], i / (float)c);

                int modified;

                if (!DoAssetIdentifierRemap(assets[i], remapObject.map, out modified))
                {
                    failures.AppendLine("  - " + assets[i]);
                    failCount++;
                }
                else
                {
                    if (modified > 0)
                    {
                        successes.AppendLine(string.Format("  - ({0}) references in {1}", modified.ToString(), assets[i]));
                        modifiedFiles++;
                    }
                }

                remappedReferences += modified;
            }

            EditorUtility.ClearProgressBar();

            log.AppendLine(string.Format("Remapped {0} references in {1} files.", remappedReferences.ToString(), modifiedFiles.ToString()));

            log.AppendLine(string.Format("\nFailed remapping {0} files:", failCount.ToString()));

            log.AppendLine(failCount > 0 ? failures.ToString() : "  - (none)");

            log.AppendLine("\nSuccessfully remapped files:");

            log.Append(successes);

            PackageImporter.Reimport(PackageImporter.EditorCorePackageManager);
            AssetDatabase.Refresh();
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

        bool ProjectContainsOldAssetIds()
        {
            // todo this should only check with the loaded remap file, but for now it's hard-coded
            return PackageImporter.IsPreProBuilder4InProject() || PackageImporter.DoesProjectContainDeprecatedGUIDs();
        }

        static string FindAssetStoreProBuilderInstall()
        {
            string[] matches = Directory.GetDirectories("Assets", "ProBuilder", SearchOption.AllDirectories);

            foreach (var match in matches)
            {
                string dir = match.Replace("\\", "/") +  "/";

                if (ValidateAssetStoreProBuilderRoot(dir))
                    return dir;
            }

            return null;
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

        ConversionReadyState ValidateAssetStoreRemoval()
        {
            ConversionReadyState state = (ConversionReadyState)0;

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
