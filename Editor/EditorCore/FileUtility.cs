using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Helper functions for working with files and directories.
    /// </summary>
    static class FileUtility
    {
        // ProBuilder folder path.
        static string s_ProBuilderFolderPath = "Packages/com.unity.probuilder/";
        static string s_ProBuilderDataPath = "Assets/ProBuilder Data/";

        // The order is important - always search for the package manager installed version first
        static readonly string[] k_PossibleInstallDirectories = new string[]
        {
            "Packages/com.unity.probuilder/",
            "UnityPackageManager/com.unity.probuilder/",
            "Assets/",
        };

        /// <summary>
        /// Check that the directory contains a valid ProBuilder install.
        /// </summary>
        /// <param name="dir">Directory to check</param>
        /// <returns></returns>
        internal static bool ValidateProBuilderRoot(string dir)
        {
            return !string.IsNullOrEmpty(dir) &&
                Directory.Exists(dir + "/Editor/EditorCore") &&
                Directory.Exists(dir + "/Runtime/Core") &&
                Directory.Exists(dir + "/Runtime/MeshOperations");
        }

        /// <summary>
        /// Return a relative path to the ProBuilder directory. Can be in the packages cache or Assets folder.
        /// If the project is in the packman cache it is immutable.
        /// </summary>
        /// <returns></returns>
        internal static string GetProBuilderInstallDirectory()
        {
            if (ValidateProBuilderRoot(s_ProBuilderFolderPath))
                return s_ProBuilderFolderPath;

            foreach (var install in k_PossibleInstallDirectories)
            {
                s_ProBuilderFolderPath = install;

                if (ValidateProBuilderRoot(s_ProBuilderFolderPath))
                    return s_ProBuilderFolderPath;
            }

            // It's not in any of the usual haunts, start digging through Assets until we find it (likely an A$ install)
            s_ProBuilderFolderPath = FindAssetStoreProBuilderInstall();

            if (Directory.Exists(s_ProBuilderFolderPath))
                return s_ProBuilderFolderPath;

            // Things are dire. ProBuilder was nowhere to be found in the Assets directory, which means either the user
            // has renamed the folder, or something very spooky is going on.
            // Either way, just create a new ProBuilder folder in Assets and return that so at the very least
            // local preferences and the material/color palettes will still work.
            Log.Warning("Creating a new ProBuilder directory... was the ProBuilder folder renamed?\nIcons & preferences may not work in this state.");
            s_ProBuilderFolderPath = "Assets/ProBuilder";
            Directory.CreateDirectory(s_ProBuilderFolderPath);

            return s_ProBuilderFolderPath;
        }

        /// <summary>
        /// Scan the Assets directory for an install of ProBuilder.
        /// </summary>
        /// <returns></returns>
        internal static string FindAssetStoreProBuilderInstall()
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

        /// <summary>
        /// Get the path to the local ProBuilder/Data folder
        /// </summary>
        /// <returns></returns>
        internal static string GetLocalDataDirectory(bool initializeIfMissing = false)
        {
            if (Directory.Exists(s_ProBuilderDataPath))
                return s_ProBuilderDataPath;

            string root = GetProBuilderInstallDirectory();

            if (root.StartsWith("Assets"))
            {
                // Installed from Asset Store or manual package import
                s_ProBuilderDataPath = root + "Data/";
            }
            else
            {
                // Scan project for ProBuilder Data folder
                // none found? create one at root
                string[] matches = Directory.GetDirectories("Assets", "ProBuilder Data", SearchOption.AllDirectories);
                s_ProBuilderDataPath = matches.Length > 0 ? matches[0] : "Assets/ProBuilder Data/";
            }

            if (!Directory.Exists(s_ProBuilderDataPath) && initializeIfMissing)
                Directory.CreateDirectory(s_ProBuilderDataPath);

            return s_ProBuilderDataPath;
        }

        internal static string[] FindAssets<T>(string pattern) where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).ToString() + " " + pattern);
        }

        internal static T[] FindAndLoadAssets<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).ToString())
                .Select(x => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(x))).ToArray();
        }

        internal static T FindAssetOfType<T>() where T : UnityEngine.Object
        {
            foreach (var i in AssetDatabase.FindAssets("t:" + typeof(T).ToString()))
            {
                T o = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(i));
                if (o != null)
                    return o;
            }

            return null;
        }

        /// <summary>
        /// Get the selected directory relative to project root.
        /// </summary>
        /// <returns></returns>
        internal static string GetSelectedDirectory()
        {
            Object o = Selection.activeObject;

            if (o != null)
            {
                string path = AssetDatabase.GetAssetPath(o.GetObjectId());

                if (!string.IsNullOrEmpty(path))
                {
                    if (Directory.Exists(path))
                        return GetRelativePath(Path.GetFullPath(path));

                    string res = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(res) && Directory.Exists(res))
                        return GetRelativePath(Path.GetFullPath(res));
                }
            }

            return "Assets";
        }

        /// <summary>
        /// Get a file or folder path relative to the Unity project directory.
        /// </summary>
        /// <param name="path">File or directory path, either relative or absolute.</param>
        /// <returns>A new path relative to the current project root.</returns>
        public static string GetRelativePath(string path)
        {
            string full = Path.GetFullPath(path).Replace("\\", "/");
            string cur = Directory.GetCurrentDirectory().Replace("\\", "/");
            if (!cur.EndsWith("/"))
                cur += "/";
            return full.Replace(cur, "");
        }

        /// <summary>
        /// Check if a file or folder exists at path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        /// <summary>
        /// Load an internal asset relative to the ProBuilder directory.
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T LoadInternalAsset<T>(string path) where T : Object
        {
            string full = string.Format("{0}{1}", GetProBuilderInstallDirectory(), path);
            return Load<T>(full);
        }

        /// <summary>
        /// Fetch a default asset from path.  If not found, a new one is created.
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T LoadRequired<T>(string path) where T : ScriptableObject, IHasDefault
        {
            T asset = Load<T>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                asset.SetDefaultValues();

                UnityEditor.EditorUtility.SetDirty(asset);

                string folder = Path.GetDirectoryName(path);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// Write contents to a file path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        [System.Obsolete("Use WriteAllText")]
        public static void WriteFile(string path, string contents)
        {
            WriteAllText(path, contents);
        }

        /// <summary>
        /// Write contents to a file path, creating a new directory if necessary.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        public static void WriteAllText(string path, string contents)
        {
            string dir = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(dir))
            {
                Log.Error("Cannot write file to \"{0}\", invalid path.", path);
                return;
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, contents);
        }

        /// <summary>
        /// Save an image to the specified path.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="path"></param>
        /// <returns>True on success, false if operation failed.</returns>
        internal static bool SaveTexture(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();

            if (string.IsNullOrEmpty(path))
                return false;

            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            return true;
        }
    }
}
