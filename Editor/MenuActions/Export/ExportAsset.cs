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
                        res = ExportMesh(assetPath, pb);

                        if (options.replaceOriginal)
                        {
                            pb.preserveMeshAssetOnDestroy = true;
                            Undo.DestroyObjectImmediate(pb);
                        }

                    }
                }
            }

            return res;
        }

        static string ExportPrefab(ProBuilderMesh mesh, bool replace)
        {
            string path = UnityEditor.EditorUtility.SaveFilePanel("Export to Asset", "Assets", mesh.name, "prefab");

            if (string.IsNullOrEmpty(path))
                return null;

            string directory = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string meshPath = string.Format("{0}/{1}.asset", directory, mesh.mesh.name).Replace("\\", "/");
            string prefabPath = string.Format("{0}/{1}.prefab", directory, name).Replace("\\", "/");

            if (File.Exists(meshPath))
                AssetDatabase.DeleteAsset(meshPath.Replace(Application.dataPath, "Assets"));

            if (File.Exists(prefabPath))
                AssetDatabase.DeleteAsset(prefabPath.Replace(Application.dataPath, "Assets"));

            return ExportPrefab(path, mesh, replace);
        }

        static string ExportMesh(ProBuilderMesh mesh, bool replace)
        {
            string path = UnityEditor.EditorUtility.SaveFilePanel("Export to Asset", "Assets", mesh.name, "asset");

            if (string.IsNullOrEmpty(path))
                return null;

            // If a file dialog was presented that means the user has already been asked to overwrite.
            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path.Replace(Application.dataPath, "Assets"));

            ExportMesh(path, mesh);

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

            string meshPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeDirectory, pb.mesh.name));

            AssetDatabase.CreateAsset(pb.mesh, meshPath);

            Mesh meshAsset = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));

            var go = replace ? pb.gameObject : Object.Instantiate(pb.gameObject);

            var component = go.GetComponent<ProBuilderMesh>();
            Undo.RecordObject(component, "Export ProBuilderMesh as Replacement");
            component.preserveMeshAssetOnDestroy = true;
            Undo.DestroyObjectImmediate(component);

            go.GetComponent<MeshFilter>().sharedMesh = meshAsset;
            string relativePrefabPath = string.Format("{0}/{1}.prefab", relativeDirectory, name);
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(relativePrefabPath);

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

        static string ExportMesh(string path, ProBuilderMesh mesh)
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
            mesh.mesh.name = name;

            string meshPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeDirectory, name));

            AssetDatabase.CreateAsset(mesh.mesh, meshPath);

            return meshPath;
        }
    }
}
