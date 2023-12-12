using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ExportObj : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Export; } }
        public override string iconPath => string.Empty;
        public override Texture2D icon => null;
        public override TooltipContent tooltip { get { return _tooltip; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Export Obj",
                "Export a Wavefront OBJ file."
            );

        public override bool hidden
        {
            get { return true; }
        }

        public override bool enabled
        {
            get { return MeshSelection.selectedObjectCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            string res = ExportWithFileDialog(MeshSelection.topInternal);

            if (string.IsNullOrEmpty(res))
                return new ActionResult(ActionResult.Status.Canceled, "User Canceled");

            Export.PingExportedModel(res);

            return new ActionResult(ActionResult.Status.Success, "Export OBJ");
        }

        // Prompt user for a save file location and export meshes as Obj.
        public static string ExportWithFileDialog(IEnumerable<ProBuilderMesh> meshes, bool asGroup = true, bool allowQuads = true, ObjOptions options = null)
        {
            if (meshes == null || !meshes.Any())
                return null;

            IEnumerable<Model> models = allowQuads
                ? meshes.Select(x => new Model(x.gameObject.name, x))
                : meshes.Select(x => new Model(x.gameObject.name, x.mesh, x.GetComponent<MeshRenderer>().sharedMaterials, x.transform.localToWorldMatrix));

            string path = null, res = null;

            if (asGroup || models.Count() < 2)
            {
                ProBuilderMesh first = meshes.FirstOrDefault();
                string name = first != null ? first.name : "ProBuilderModel";
                path = UnityEditor.EditorUtility.SaveFilePanel("Export to Obj", "Assets", name, "obj");

                if (string.IsNullOrEmpty(path))
                    return null;

                res = DoExport(path, models, options);
            }
            else
            {
                path = UnityEditor.EditorUtility.SaveFolderPanel("Export to Obj", "Assets", "");

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    return null;

                foreach (Model model in models)
                    res = DoExport(string.Format("{0}/{1}.obj", path, model.name), new List<Model>() { model }, options);
            }

            return res;
        }

        internal static string DoExport(string path, IEnumerable<Model> models, ObjOptions options)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string directory = Path.GetDirectoryName(path);

            List<string> textures = null;
            string obj, mat;

            if (ObjExporter.Export(name, models, out obj, out mat, out textures, options))
            {
                try
                {
                    FileUtility.WriteAllText(string.Format("{0}/{1}.obj", directory, name), obj);
                    FileUtility.WriteAllText(string.Format("{0}/{1}.mtl", directory, name.Replace(" ", "_")), mat);
                    CopyTextures(textures, directory);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(string.Format("Failed writing obj to path: {0}\n{1}", string.Format("{0}/{1}.obj", path, name), e.ToString()));
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("No meshes selected.");
                return null;
            }

            return path;
        }

        // Copy files from their path to a destination directory.
        static void CopyTextures(List<string> textures, string destination)
        {
            foreach (string path in textures)
            {
                string dest = string.Format("{0}/{1}", destination, Path.GetFileName(path));

                if (!File.Exists(path))
                {
                    Log.Warning("OBJ Export: Could not find texture \"" + path + ",\" it will not be copied.");
                    continue;
                }

                if (!File.Exists(dest))
                    File.Copy(path, dest);
            }
        }
    }
}
