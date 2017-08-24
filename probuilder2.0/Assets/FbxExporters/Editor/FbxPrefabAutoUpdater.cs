using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FbxExporters
{
    /// <summary>
    /// This class handles updating prefabs that are linked to an FBX source file.
    ///
    /// Whenever the Unity asset database imports (or reimports) assets, this
    /// class receives the <code>OnPostprocessAllAssets</code> event.
    ///
    /// If any FBX assets were (re-)imported, this class finds prefab assets
    /// that are linked to those FBX files, and makes them sync to the new FBX.
    ///
    /// The FbxPrefab component handles the sync process. This class is limited to
    /// discovering which prefabs need to be updated automatically.
    ///
    /// All functions in this class are static: there is no reason to make an
    /// instance.
    /// </summary>
    public /*static*/ class FbxPrefabAutoUpdater : UnityEditor.AssetPostprocessor
    {
        public static string FindFbxPrefabAssetPath()
        {
            // Find guids that are scripts that look like FbxPrefab.
            // That catches FbxPrefabTest too, so we have to make sure.
            var allGuids = AssetDatabase.FindAssets("FbxPrefab t:MonoScript");
            foreach(var guid in allGuids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("/FbxPrefab.cs")) {
                    return path;
                }
            }
            Debug.LogError("FbxPrefab.cs not found; are you trying to uninstall FbxExporters?");
            return "";
        }

        public static bool IsFbxAsset(string assetPath) {
            return assetPath.EndsWith(".fbx");
        }

        public static bool IsPrefabAsset(string assetPath) {
            return assetPath.EndsWith(".prefab");
        }

        /// <summary>
        /// Return false if the prefab definitely does not have an
        /// FbxPrefab component that points to one of the Fbx assets
        /// that were imported.
        ///
        /// May return a false positive. This is a cheap check.
        /// </summary>
        public static bool MayHaveFbxPrefabToFbxAsset(string prefabPath,
                string fbxPrefabScriptPath, HashSet<string> fbxImported) {
            var depPaths = AssetDatabase.GetDependencies(prefabPath, recursive: false);
            bool dependsOnFbxPrefab = false;
            bool dependsOnImportedFbx = false;
            foreach(var dep in depPaths) {
                if (dep == fbxPrefabScriptPath) {
                    if (dependsOnImportedFbx) { return true; }
                    dependsOnFbxPrefab = true;
                } else if (fbxImported.Contains(dep)) {
                    if (dependsOnFbxPrefab) { return true; }
                    dependsOnImportedFbx = true;
                }
            }
            // Either none or only one of the conditions was true, which
            // means this prefab certainly doesn't match.
            return false;
        }

        static void OnPostprocessAllAssets(string [] imported, string [] deleted, string [] moved, string [] movedFrom)
        {
            //Debug.Log("Postprocessing...");

            // Did we import an fbx file at all?
            // Optimize to not allocate in the common case of 'no'
            HashSet<string> fbxImported = null;
            foreach(var fbxModel in imported) {
                if (IsFbxAsset(fbxModel)) {
                    if (fbxImported == null) { fbxImported = new HashSet<string>(); }
                    fbxImported.Add(fbxModel);
                    //Debug.Log("Tracking fbx asset " + fbxModel);
                } else {
                    //Debug.Log("Not an fbx asset " + fbxModel);
                }
            }
            if (fbxImported == null) {
                //Debug.Log("No fbx imported");
                return;
            }

            //
            // Iterate over all the prefabs that have an FbxPrefab component that
            // points to an FBX file that got (re)-imported.
            //
            // There's no one-line query to get those, so we search for a much
            // larger set and whittle it down, hopefully without needing to
            // load the asset into memory if it's not necessary.
            //
            var fbxPrefabScriptPath = FindFbxPrefabAssetPath();
            var allObjectGuids = AssetDatabase.FindAssets("t:GameObject");
            foreach(var guid in allObjectGuids) {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsPrefabAsset(prefabPath)) {
                    //Debug.Log("Not a prefab: " + prefabPath);
                    continue;
                }
                if (!MayHaveFbxPrefabToFbxAsset(prefabPath, fbxPrefabScriptPath, fbxImported)) {
                    //Debug.Log("No dependence: " + prefabPath);
                    continue;
                }
                //Debug.Log("Considering updating prefab " + prefabPath);

                // We're now guaranteed that this is a prefab, and it depends
                // on the FbxPrefab script, and it depends on an Fbx file that
                // was imported.
                //
                // To be sure it has an FbxPrefab component that points to an
                // Fbx file, we need to load the asset (which we need to do to
                // update the prefab anyway).
                var prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
                if (!prefab) {
                    //Debug.LogWarning("FbxPrefab reimport: failed to update prefab " + prefabPath);
                    continue;
                }
                foreach(var fbxPrefabComponent in prefab.GetComponentsInChildren<FbxPrefab>()) {
                    if (!fbxPrefabComponent.WantsAutoUpdate()) {
                        //Debug.Log("Not auto-updating " + prefabPath);
                        continue;
                    }
                    var fbxAssetPath = fbxPrefabComponent.GetFbxAssetPath();
                    if (!fbxImported.Contains(fbxAssetPath)) {
                        //Debug.Log("False-positive dependence: " + prefabPath + " via " + fbxAssetPath);
                        continue;
                    }
                    //Debug.Log("Updating " + prefabPath + "...");
                    fbxPrefabComponent.SyncPrefab();
                }
            }
        }
    }
}
