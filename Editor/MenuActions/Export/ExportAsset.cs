using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder.Actions
{
    [Serializable]
    struct ExportAssetOptions
    {
        public bool makePrefab;
        public bool replaceOriginal;

        public static readonly ExportAssetOptions defaults = new ExportAssetOptions()
        {
            makePrefab = false,
            replaceOriginal = false
        };
    }

    sealed class ExportAsset : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Export; } }
        public override string iconPath => string.Empty;
        public override Texture2D icon => null;
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        internal static Pref<ExportAssetOptions> s_ExportAssetOptions =new Pref<ExportAssetOptions>("export.assetOptions", ExportAssetOptions.defaults);

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Export Asset",
                "Export a Unity mesh asset file."
            );

        public override bool enabled
        {
            get { return MeshSelection.selectedObjectCount > 0; }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            var opt = s_ExportAssetOptions.value;
            var res = ExportWithFileDialog(MeshSelection.topInternal, opt);
            Export.PingExportedModel(res);
            return new ActionResult(ActionResult.Status.Success, opt.makePrefab ? "Make Prefab" : "Make Asset");
        }

        /// <summary>
        /// Export meshes to a Unity asset.
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        public static string ExportWithFileDialog(IList<ProBuilderMesh> meshes, ExportAssetOptions options)
        {
            if (meshes == null || meshes.Count == 0)
                return "";

            string res = null;

            if (meshes.Count < 2)
            {
                ProBuilderMesh first = meshes.FirstOrDefault();

                if (first == null)
                    return null;

                res = options.makePrefab
                    ? ExportPrefab(first, options.replaceOriginal)
                    : ExportMesh(first, options.replaceOriginal);
            }
            else
            {
                string path = UnityEditor.EditorUtility.SaveFolderPanel("Export to Asset", "Assets", "");

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    return null;

                meshes = meshes.ToList();
                for (int i = 0, c = meshes.Count; i < c; i++)
                {
                    var pb = meshes[i];
                    var assetPath = string.Format("{0}/{1}.asset", path, pb.name);

                    if (options.makePrefab)
                    {
                        res = ExportPrefab(assetPath, pb, options.replaceOriginal, true);
                    }
                    else
                    {
                        res = ExportMesh(assetPath, pb, options.replaceOriginal, true);

                        if (options.replaceOriginal)
                            StripProBuilderScripts.DestroyProBuilderMeshAndDependencies(pb.gameObject, pb, true, true);
                    }
                }
            }

            ProBuilderEditor.Refresh();
            AssetDatabase.Refresh();

            return res;
        }

        static string AssetPathFromFullPath(string path, string extension)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string directory = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileNameWithoutExtension(path);
            string relativeDirectory = string.Format("Assets{0}", directory.Replace(Application.dataPath, ""));
            return !string.IsNullOrEmpty(extension)
                ? string.Format("{0}/{1}.{2}", relativeDirectory, name, extension)
                : string.Format("{0}/{1}", relativeDirectory, name);
        }

        static string AskForPath(string fileName, string extension)
        {
            return UnityEditor.EditorUtility.SaveFilePanel("Export to Asset", "Assets", fileName, extension);
        }

        static string AskForAssetPath(string fileName, string extension)
        {
            return AssetPathFromFullPath(AskForPath(fileName, extension), extension);
        }

        static string PickDifferentAssetPath(string previousAssetPath)
        {
            var extension = Path.GetExtension(previousAssetPath);
            if (extension != null && extension.StartsWith("."))
            {
                extension = extension.Substring(1);
            }
            var newPath = AskForAssetPath(
                Path.GetFileNameWithoutExtension(previousAssetPath),
                extension
            );
            return !string.IsNullOrEmpty(newPath)
                ? newPath
                : AssetDatabase.GenerateUniqueAssetPath(previousAssetPath);
        }

        static string ExportPrefab(ProBuilderMesh mesh, bool replaceOriginal)
        {
            string path = AskForPath(mesh.name, "prefab");

            if (string.IsNullOrEmpty(path))
                return null;

            return ExportPrefab(path, mesh, replaceOriginal, false);
        }

        static string ExportMesh(ProBuilderMesh mesh, bool replaceOriginal)
        {
            string path = AskForPath(mesh.name, "asset");

            if (string.IsNullOrEmpty(path))
                return null;

            ExportMesh(path, mesh, replaceOriginal, false);

            if (replaceOriginal)
            {
                StripProBuilderScripts.DestroyProBuilderMeshAndDependencies(mesh.gameObject, mesh, true, true);
            }
            else
            {
                mesh.mesh = null;
                EditorUtility.SynchronizeWithMeshFilter(mesh);
            }

            return path;
        }

        static string ExportPrefab(string path, ProBuilderMesh pb, bool replaceOriginal, bool batchExport)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string basePath = AssetPathFromFullPath(path, null);

            pb.ToMesh();
            pb.Refresh();
            pb.Optimize();

            string meshPath = string.Format("{0}.asset", basePath);
            string prefabPath = string.Format("{0}.prefab", basePath);

            if (batchExport)
            {
                // Never overwrite during batch export.
                meshPath = AssetDatabase.GenerateUniqueAssetPath(meshPath);
                prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            }
            else
            {
                var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (existingMesh)
                {
                    // Overwriting mesh.
                    var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (existingPrefab)
                    {
                        // Overwriting prefab as well.
                        var meshFilter = existingPrefab.GetComponent<MeshFilter>();
                        if (!meshFilter || meshFilter.sharedMesh != existingMesh)
                        {
                            // Prefab and mesh being overwritten are not related, pick different path.
                            meshPath = PickDifferentAssetPath(meshPath);
                        }
                        // Else allow overwrite both as they are related.
                    }
                    else
                    {
                        // Unrelated mesh is being overriden, pick different path.
                        meshPath = PickDifferentAssetPath(meshPath);
                    }
                }
            }

            Mesh meshAsset = pb.mesh;
            meshAsset.name = name;
            meshAsset = CreateOrReplaceAsset(meshAsset, meshPath);

            var go = replaceOriginal ? pb.gameObject : Object.Instantiate(pb.gameObject);

            var component = go.GetComponent<ProBuilderMesh>();
            Undo.RecordObject(component, "Export ProBuilderMesh as Replacement");
            StripProBuilderScripts.DestroyProBuilderMeshAndDependencies(go, component, true, true);

            go.GetComponent<MeshFilter>().sharedMesh = meshAsset;
            var meshCollider = go.GetComponent<MeshCollider>();
            if (meshCollider)
                meshCollider.sharedMesh = meshAsset;

            if (replaceOriginal)
                PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);
            else
            {
                // if we're about to overwrite the prefab asset of the source mesh, first disconnect it so that we're not
                // overwriting the instance
                if (PrefabUtility.IsPartOfAnyPrefab(pb)
                    && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(pb) == prefabPath)
                    PrefabUtility.UnpackPrefabInstance(pb.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }

            if (!replaceOriginal)
            {
                pb.mesh = null;
                EditorUtility.SynchronizeWithMeshFilter(pb);
                Object.DestroyImmediate(go);
            }

            return meshPath;
        }

        static string ExportMesh(string path, ProBuilderMesh mesh, bool replaceOriginal, bool batchExport)
        {
            var existing = AssetDatabase.GetAssetPath(mesh.mesh);

            if (!string.IsNullOrEmpty(existing))
                return existing;

            string name = Path.GetFileNameWithoutExtension(path);
            string meshPath = AssetPathFromFullPath(path, "asset");
            if (batchExport)
            {
                // Never replace assets during batch export.
                meshPath = AssetDatabase.GenerateUniqueAssetPath(meshPath);
            }

            mesh.ToMesh();
            mesh.Refresh();
            mesh.Optimize();

            var meshAsset = mesh.mesh;
            meshAsset.name = name;

            meshAsset = CreateOrReplaceAsset(meshAsset, meshPath);

            if (replaceOriginal)
            {
                mesh.GetComponent<MeshFilter>().sharedMesh = meshAsset;
                var meshCollider = mesh.GetComponent<MeshCollider>();
                if (meshCollider)
                {
                    meshCollider.sharedMesh = meshAsset;
                }
            }

            return meshPath;
        }

        static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                return asset;
            }

            var tempPath = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(asset, tempPath);
            FileUtil.ReplaceFile(tempPath, path);
            AssetDatabase.DeleteAsset(tempPath);
            return existingAsset;
        }
    }
}
