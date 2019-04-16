using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine.ProBuilder;
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
        public override Texture2D icon { get { return null; } }
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

        public override ActionResult DoAction()
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
            if (meshes == null || !meshes.Any())
                return "";

            string res = null;

            if (meshes.Count() < 2)
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
                for(int i = 0, c = meshes.Count; i < c; i++)
                {
                    var pb = meshes[i];
                    var assetPath = string.Format("{0}/{1}.asset", path, pb.name);


                    if(options.makePrefab)
                    {
                        res = ExportPrefab(assetPath, pb, options.replaceOriginal);
                    }
                    else
                    {
                        res = ExportMesh(assetPath, pb, options.replaceOriginal);

                        if (options.replaceOriginal)
                        {
                            pb.preserveMeshAssetOnDestroy = true;
                            Undo.DestroyObjectImmediate(pb);
                        }

                    }
                }
            }
            AssetDatabase.Refresh();

            return res;
        }

        static string ExportPrefab(ProBuilderMesh mesh, bool replace)
        {
            string path = UnityEditor.EditorUtility.SaveFilePanel("Export to Asset", "Assets", mesh.name, "prefab");

            if (string.IsNullOrEmpty(path))
                return null;

            return ExportPrefab(path, mesh, replace);
        }

        static string ExportMesh(ProBuilderMesh mesh, bool replace)
        {
            string path = UnityEditor.EditorUtility.SaveFilePanel("Export to Asset", "Assets", mesh.name, "asset");

            if (string.IsNullOrEmpty(path))
                return null;

            ExportMesh(path, mesh, replace);

            if (replace)
            {
                mesh.preserveMeshAssetOnDestroy = true;
                Undo.DestroyObjectImmediate(mesh);
            }
            else
            {
                mesh.mesh = null;
                EditorUtility.SynchronizeWithMeshFilter(mesh);
            }

            return path;
        }

        static string ExportPrefab(string path, ProBuilderMesh pb, bool replace)
        {
            string directory = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileNameWithoutExtension(path);
            string relativeDirectory = string.Format("Assets{0}", directory.Replace(Application.dataPath, ""));

            pb.ToMesh();
            pb.Refresh();
            pb.Optimize();

            string meshPath = string.Format("{0}/{1}.asset", relativeDirectory, name);

            Mesh meshAsset = pb.mesh;
            meshAsset.name = name;
            meshAsset = CreateOrReplaceAsset(meshAsset, meshPath);

            var go = replace ? pb.gameObject : Object.Instantiate(pb.gameObject);

            var component = go.GetComponent<ProBuilderMesh>();
            Undo.RecordObject(component, "Export ProBuilderMesh as Replacement");
            component.preserveMeshAssetOnDestroy = true;
            Undo.DestroyObjectImmediate(component);

            go.GetComponent<MeshFilter>().sharedMesh = meshAsset;
            var meshCollider = go.GetComponent<MeshCollider>();
            if (meshCollider)
            {
                meshCollider.sharedMesh = meshAsset;
            }
            string prefabPath = string.Format("{0}/{1}.prefab", relativeDirectory, name);

#if UNITY_2018_3_OR_NEWER
            if(replace)
                PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);
            else
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
#else
            PrefabUtility.CreatePrefab(prefabPath, go, ReplacePrefabOptions.Default);
#endif
            if (!replace)
            {
                pb.mesh = null;
                EditorUtility.SynchronizeWithMeshFilter(pb);
                Object.DestroyImmediate(go);
            }

            return meshPath;
        }

        static string ExportMesh(string path, ProBuilderMesh mesh, bool replace)
        {
            var existing = AssetDatabase.GetAssetPath(mesh.mesh);

            if (!string.IsNullOrEmpty(existing))
                return existing;

            string directory = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileNameWithoutExtension(path);
            string relativeDirectory = string.Format("Assets{0}", directory.Replace(Application.dataPath, ""));

            mesh.ToMesh();
            mesh.Refresh();
            mesh.Optimize();

            var meshAsset = mesh.mesh;
            meshAsset.name = name;

            string meshPath = string.Format("{0}/{1}.asset", relativeDirectory, name);
            meshAsset = CreateOrReplaceAsset(meshAsset, meshPath);

            if (replace)
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
            else
            {
                var tempPath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(asset, tempPath);
                FileUtil.ReplaceFile(tempPath, path);
                AssetDatabase.DeleteAsset(tempPath);
                return existingAsset;
            }
        }
    }
}
