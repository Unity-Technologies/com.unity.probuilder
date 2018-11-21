using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.ProBuilder.AssetIdRemapUtility
{
    static class PackageImporter
    {
        const string k_PackageManagerEditorCore = "e98d45d69e2c4936a7382af00fd45e58";
        const string k_AssetStoreEditorCore = "4df21bd079886d84699ca7be1316c7a7";
        const string k_ProBuilder2CoreGUID = "0472bdc8d6d15384d98f22ee34302f9c";
        const string k_ProBuilder3CoreGUID = "4f0627da958b4bb78c260446066f065f";

        static readonly string[] k_AssetStoreInstallGuids = new string[]
        {
            "0472bdc8d6d15384d98f22ee34302f9c", // ProBuilderCore
            "b617d7797480df7499f141d87e13ebc5", // ProBuilderMeshOps
            "4df21bd079886d84699ca7be1316c7a7"  // ProBuilderEditor
        };

        static readonly string[] k_PackageManagerInstallGuids = new string[]
        {
            "4f0627da958b4bb78c260446066f065f", // Core
            "9b27d8419276465b80eb88c8799432a1", // Mesh Ops
            "e98d45d69e2c4936a7382af00fd45e58", // Editor
        };

        internal static string EditorCorePackageManager { get { return k_PackageManagerEditorCore; } }
        internal static string EditorCoreAssetStore { get { return k_AssetStoreEditorCore; } }

        internal static void SetEditorDllEnabled(string guid, bool isEnabled)
        {
            string dllPath = AssetDatabase.GUIDToAssetPath(guid);

            var importer = AssetImporter.GetAtPath(dllPath) as PluginImporter;

            if (importer != null)
            {
                importer.SetCompatibleWithAnyPlatform(false);
                importer.SetCompatibleWithEditor(isEnabled);
            }
        }

        internal static bool IsEditorPluginEnabled(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as PluginImporter;
            if (!importer)
                return false;
            return importer.GetCompatibleWithEditor() && !importer.GetCompatibleWithAnyPlatform();
        }

        internal static void Reimport(string guid)
        {
            string dllPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!string.IsNullOrEmpty(dllPath))
                AssetDatabase.ImportAsset(dllPath);
        }

        static bool AreAnyAssetsAreLoaded(string[] guids)
        {
            foreach (var id in guids)
            {
                if (AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(id)) != null)
                    return true;
            }

            return false;
        }

        static bool FileContainsString(string path, string search)
        {
            using (var sr = new StreamReader(path))
            {
                while (sr.Peek() > -1)
                {
                    if (sr.ReadLine().Contains(search))
                        return true;
                }
            }

            return false;
        }

        internal static bool DoesProjectContainDeprecatedGUIDs()
        {
            foreach (var file in Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories))
                if (FileContainsString(file, k_ProBuilder2CoreGUID) || FileContainsString(file, k_ProBuilder3CoreGUID))
                    return true;

            foreach (var file in Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories))
                if (FileContainsString(file, k_ProBuilder2CoreGUID) || FileContainsString(file, k_ProBuilder3CoreGUID))
                    return true;

            return false;
        }

        /// <summary>
        /// Check if any pre-4.0 ProBuilder package is present in the project
        /// </summary>
        /// <returns></returns>
        internal static bool IsPreProBuilder4InProject()
        {
            return AreAnyAssetsAreLoaded(k_AssetStoreInstallGuids)
                || AreAnyAssetsAreLoaded(k_PackageManagerInstallGuids);
        }

        internal static bool IsProBuilder4OrGreaterLoaded()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(x => x.ToString().Contains("Unity.ProBuilder"));
        }
    }
}
