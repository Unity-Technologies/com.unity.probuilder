using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

namespace FbxExporters.EditorTools {

    [CustomEditor(typeof(ExportSettings))]
    public class ExportSettingsEditor : UnityEditor.Editor {
        Vector2 scrollPos = Vector2.zero;
        const float LabelWidth = 225;
        const float SelectableLabelMinWidth = 100;
        const float BrowseButtonWidth = 55;

        public override void OnInspectorGUI() {
            ExportSettings exportSettings = (ExportSettings)target;

            // Increasing the label width so that none of the text gets cut off
            EditorGUIUtility.labelWidth = LabelWidth;

            scrollPos = GUILayout.BeginScrollView (scrollPos);

            exportSettings.weldVertices = EditorGUILayout.Toggle ("Weld Vertices:", exportSettings.weldVertices);
            exportSettings.embedTextures = EditorGUILayout.Toggle ("Embed Textures:", exportSettings.embedTextures);
            exportSettings.mayaCompatibleNames = EditorGUILayout.Toggle (
                new GUIContent ("Convert to Maya Compatible Naming:",
                    "In Maya some symbols such as spaces and accents get replaced when importing an FBX " +
                    "(e.g. \"foo bar\" becomes \"fooFBXASC032bar\"). " +
                    "On export, convert the names of GameObjects so they are Maya compatible." +
                    (exportSettings.mayaCompatibleNames ? "" :
                        "\n\nWARNING: Disabling this feature may result in lost material connections," +
                    " and unexpected character replacements in Maya.")
                ),
                exportSettings.mayaCompatibleNames);

            exportSettings.centerObjects = EditorGUILayout.Toggle (
                new GUIContent("Center Objects:",
                    "Export objects centered around the union of the bounding box of selected objects"),
                exportSettings.centerObjects
            );

            GUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent (
                "Export Path:",
                "Relative path for saving Model Prefabs."));

            var pathLabel = ExportSettings.GetRelativeSavePath();
            if (pathLabel == ".") { pathLabel = "(Assets root)"; }
            EditorGUILayout.SelectableLabel(pathLabel,
                EditorStyles.textField,
                GUILayout.MinWidth(SelectableLabelMinWidth),
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (GUILayout.Button ("Browse", EditorStyles.miniButton, GUILayout.Width (BrowseButtonWidth))) {
                string initialPath = ExportSettings.GetAbsoluteSavePath();
                string fullPath = EditorUtility.OpenFolderPanel (
                        "Select Model Prefabs Path", initialPath, null
                        );

                // Unless the user canceled, make sure they chose something in the Assets folder.
                if (!string.IsNullOrEmpty (fullPath)) {
                    var relativePath = ExportSettings.ConvertToAssetRelativePath(fullPath);
                    if (string.IsNullOrEmpty(relativePath)) {
                        Debug.LogWarning ("Please select a location in the Assets folder");
                    } else {
                        ExportSettings.SetRelativeSavePath(relativePath);
                    }
                }
            }
            GUILayout.EndHorizontal ();
            GUILayout.BeginHorizontal ();

            GUILayout.Label (new GUIContent (
                "Turn Table Scene:",
                "Scene to use for reviewing models. If none, a scene will be created on review."));
            
            exportSettings.turntableScene = EditorGUILayout.ObjectField (
                exportSettings.turntableScene, typeof(SceneAsset), false
            );

            GUILayout.EndHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndScrollView ();

            if (GUI.changed) {
                EditorUtility.SetDirty (exportSettings);
                exportSettings.Save ();
            }
        }

    }

    [FilePath("ProjectSettings/FbxExportSettings.asset",FilePathAttribute.Location.ProjectFolder)]
    public class ExportSettings : ScriptableSingleton<ExportSettings>
    {
        public const string kDefaultSavePath = "Objects";

        // Note: default values are set in LoadDefaults().
        public bool weldVertices;
        public bool embedTextures;
        public bool mayaCompatibleNames;
        public bool centerObjects;

        [SerializeField]
        public UnityEngine.Object turntableScene;

        /// <summary>
        /// The path where Convert To Model will save the new fbx and prefab.
        ///
        /// To help teams work together, this is stored to be relative to the
        /// Application.dataPath, and the path separator is the forward-slash
        /// (e.g. unix and http, not windows).
        ///
        /// Use GetRelativeSavePath / SetRelativeSavePath to get/set this
        /// value, properly interpreted for the current platform.
        /// </summary>
        [SerializeField]
        string convertToModelSavePath;

        protected override void LoadDefaults()
        {
            weldVertices = true;
            embedTextures = false;
            mayaCompatibleNames = true;
            centerObjects = true;
            convertToModelSavePath = kDefaultSavePath;
            turntableScene = null;
        }

        public static string GetTurnTableSceneName(){
            if (instance.turntableScene) {
                return instance.turntableScene.name;
            }
            return null;
        }

        public static string GetTurnTableScenePath(){
            if (instance.turntableScene) {
                return AssetDatabase.GetAssetPath (instance.turntableScene);
            }
            return null;
        }

        /// <summary>
        /// The path where Convert To Model will save the new fbx and prefab.
        /// This is relative to the Application.dataPath ; it uses '/' as the
        /// separator on all platforms.
        /// </summary>
        public static string GetRelativeSavePath() {
            var relativePath = instance.convertToModelSavePath;
            if (string.IsNullOrEmpty(relativePath)) {
                relativePath = kDefaultSavePath;
            }
            return NormalizePath(relativePath, isRelative: true);
        }

        /// <summary>
        /// The path where Convert To Model will save the new fbx and prefab.
        /// This is an absolute path, with platform separators.
        /// </summary>
        public static string GetAbsoluteSavePath() {
            var relativePath = GetRelativeSavePath();
            var absolutePath = Path.Combine(Application.dataPath, relativePath);
            return NormalizePath(absolutePath, isRelative: false,
                    separator: Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Set the path where Convert To Model will save the new fbx and prefab.
        /// This is interpreted as being relative to the Application.dataPath
        /// </summary>
        public static void SetRelativeSavePath(string newPath) {
            instance.convertToModelSavePath = NormalizePath(newPath, isRelative: true);
        }

        /// <summary>
        /// Convert an absolute path into a relative path like what you would
        /// get from GetRelativeSavePath.
        ///
        /// This uses '/' as the path separator.
        ///
        /// If 'requireSubdirectory' is the default on, return empty-string if the full
        /// path is not in a subdirectory of assets.
        /// </summary>
        public static string ConvertToAssetRelativePath(string fullPathInAssets, bool requireSubdirectory = true)
        {
            var relativePath = GetRelativePath(Application.dataPath, fullPathInAssets);
            if (requireSubdirectory && relativePath.StartsWith("..")) {
                if (relativePath.Length == 2 || relativePath[2] == '/') {
                    // The relative path has us pop out to another directory,
                    // so return an empty string as requested.
                    return "";
                }
            }
            return relativePath;
        }

        /// <summary>
        /// Compute how to get from 'fromDir' to 'toDir' via a relative path.
        /// </summary>
        public static string GetRelativePath(string fromDir, string toDir,
                char separator = '/')
        {
            // https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
            // Except... the MakeRelativeUri that ships with Unity is buggy.
            // e.g. https://bugzilla.xamarin.com/show_bug.cgi?id=5921
            // among other bugs. So we roll our own.

            // Normalize the paths, assuming they're absolute paths (if they
            // aren't, they get normalized as relative paths)
            fromDir = NormalizePath(fromDir, isRelative: false);
            toDir = NormalizePath(toDir, isRelative: false);

            // Break them into path components.
            var fromDirs = fromDir.Split('/');
            var toDirs = toDir.Split('/');

            // Find the least common ancestor
            int lca = -1;
            for(int i = 0, n = System.Math.Min(fromDirs.Length, toDirs.Length); i < n; ++i) {
                if (fromDirs[i] != toDirs[i]) { break; }
                lca = i;
            }

            // Step up from the fromDir to the lca, then down from lca to the toDir.
            // If from = /a/b/c/d
            // and to  = /a/b/e/f/g
            // Then we need to go up 2 and down 3.
            var nStepsUp = (fromDirs.Length - 1) - lca;
            var nStepsDown = (toDirs.Length - 1) - lca;
            if (nStepsUp + nStepsDown == 0) {
                return ".";
            }

            var relDirs = new string[nStepsUp + nStepsDown];
            for(int i = 0; i < nStepsUp; ++i) {
                relDirs[i] = "..";
            }
            for(int i = 0; i < nStepsDown; ++i) {
                relDirs[nStepsUp + i] = toDirs[lca + 1 + i];
            }

            return string.Join("" + separator, relDirs);
        }

        /// <summary>
        /// Normalize a path, cleaning up path separators, resolving '.' and
        /// '..', removing duplicate and trailing path separators, etc.
        ///
        /// If the path passed in is a relative path, we remove leading path separators.
        /// If it's an absolute path we don't.
        ///
        /// If you claim the path is absolute but actually it's relative, we
        /// treat it as a relative path.
        /// </summary>
        public static string NormalizePath(string path, bool isRelative,
                char separator = '/')
        {
            // Use slashes to simplify the code (we're going to clobber them all anyway).
            path = path.Replace('\\', '/');

            // If we're supposed to be an absolute path, but we're actually a
            // relative path, ignore the 'isRelative' flag.
            if (!isRelative && !Path.IsPathRooted(path)) {
                isRelative = true;
            }

            // Build up a list of directory items.
            var dirs = path.Split('/');

            // Modify dirs in-place, reading from readIndex and remembering
            // what index we've written to.
            int lastWriteIndex = -1;
            for (int readIndex = 0, n = dirs.Length; readIndex < n; ++readIndex) {
                var dir = dirs[readIndex];

                // Skip duplicate path separators.
                if (dir == "") {
                    // Skip if it's not a leading path separator.
                   if (lastWriteIndex >= 0) {
                       continue; }

                   // Also skip if it's leading and we have a relative path.
                   if (isRelative) {
                       continue;
                   }
                }

                // Skip '.'
                if (dir == ".") {
                    continue;
                }

                // Erase the previous directory we read on '..'.
                // Exception: we can start with '..'
                // Exception: we can have multiple '..' in a row.
                //
                // Note: this ignores the actual file system and the funny
                // results you see when there are symlinks.
                if (dir == "..") {
                    if (lastWriteIndex == -1) {
                        // Leading '..' => handle like a normal directory.
                    } else if (dirs[lastWriteIndex] == "..") {
                        // Multiple ".." => handle like a normal directory.
                    } else {
                        // Usual case: delete the previous directory.
                        lastWriteIndex--;
                        continue;
                    }
                }

                // Copy anything else to the next index.
                ++lastWriteIndex;
                dirs[lastWriteIndex] = dirs[readIndex];
            }

            if (lastWriteIndex == -1 || (lastWriteIndex == 0 && dirs[lastWriteIndex] == "")) {
                // If we didn't keep anything, we have the empty path.
                // For an absolute path that's / ; for a relative path it's .
                if (isRelative) {
                    return ".";
                } else {
                    return "" + separator;
                }
            } else {
                // Otherwise print out the path with the proper separator.
                return String.Join("" + separator, dirs, 0, lastWriteIndex + 1);
            }
        }

        [MenuItem("Edit/Project Settings/Fbx Export", priority = 300)]
        static void ShowManager()
        {
            instance.name = "Fbx Export Settings";
            Selection.activeObject = instance;
            instance.Load();
        }

        public void Save()
        {
            instance.Save (true);
        }
    }

    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
    {
        private static T s_Instance;
        public static T instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = ScriptableObject.CreateInstance<T>();
                    s_Instance.Load();
                }
                return s_Instance;
            }
        }

        protected ScriptableSingleton()
        {
            if (s_Instance != null)
            {
                Debug.LogError(typeof(T) + " already exists. Did you query the singleton in a constructor?");
            }
        }

        protected abstract void LoadDefaults();

        protected virtual void Load()
        {
            string filePath = GetFilePath();
            if (!System.IO.File.Exists(filePath)) {
                LoadDefaults();
            } else {
                try {
                    var fileData = System.IO.File.ReadAllText(filePath);
                    EditorJsonUtility.FromJsonOverwrite(fileData, s_Instance);
                } catch(Exception xcp) {
                    // Quash the exception and take the default settings.
                    Debug.LogException(xcp);
                    LoadDefaults();
                }
            }
        }

        protected virtual void Save(bool saveAsText)
        {
            if (s_Instance == null)
            {
                Debug.Log("Cannot save ScriptableSingleton: no instance!");
                return;
            }
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                System.IO.File.WriteAllText(filePath, EditorJsonUtility.ToJson(s_Instance, true));
            }
        }

        private static string GetFilePath()
        {
            foreach(var attr in typeof(T).GetCustomAttributes(true)) {
                FilePathAttribute filePathAttribute = attr as FilePathAttribute;
                if (filePathAttribute != null)
                {
                    return filePathAttribute.filepath;
                }
            }
            return null;
        }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class FilePathAttribute : Attribute
    {
        public enum Location
        {
            PreferencesFolder,
            ProjectFolder
        }
        public string filepath
        {
            get;
            set;
        }
        public FilePathAttribute(string relativePath, FilePathAttribute.Location location)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogError("Invalid relative path! (its null or empty)");
                return;
            }
            if (relativePath[0] == '/')
            {
                relativePath = relativePath.Substring(1);
            }
            if (location == FilePathAttribute.Location.PreferencesFolder)
            {
                this.filepath = InternalEditorUtility.unityPreferencesFolder + "/" + relativePath;
            }
            else
            {
                this.filepath = relativePath;
            }
        }
    }

}
