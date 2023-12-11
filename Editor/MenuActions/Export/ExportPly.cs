using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Stl;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ExportPly : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Export; } }
        public override string iconPath => string.Empty;
        public override Texture2D icon => null;
        public override TooltipContent tooltip { get { return _tooltip; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Export Ply",
                "Export a Stanford PLY file."
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

            return new ActionResult(ActionResult.Status.Success, "Export PLY");
        }

        /**
         *  Prompt user for a save file location and export meshes as Obj.
         */
        public static string ExportWithFileDialog(IEnumerable<ProBuilderMesh> meshes, bool asGroup = true, PlyOptions options = null)
        {
            if (meshes == null || meshes.Count() < 1)
                return null;

            string path = null, res = null;

            if (asGroup || meshes.Count() < 2)
            {
                ProBuilderMesh first = meshes.FirstOrDefault();
                string name = first != null ? first.name : "ProBuilderModel";
                path = UnityEditor.EditorUtility.SaveFilePanel("Export as PLY", "Assets", name, "ply");

                if (string.IsNullOrEmpty(path))
                    return null;

                res = DoExport(path, meshes, options);
            }
            else
            {
                path = UnityEditor.EditorUtility.SaveFolderPanel("Export to PLY", "Assets", "");

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    return null;

                foreach (ProBuilderMesh model in meshes)
                    res = DoExport(string.Format("{0}/{1}.ply", path, model.name), new List<ProBuilderMesh>() { model }, options);
            }

            return res;
        }

        static string DoExport(string path, IEnumerable<ProBuilderMesh> models, PlyOptions options)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string directory = Path.GetDirectoryName(path);

            string ply;

            if (PlyExporter.Export(models, out ply, options))
            {
                try
                {
                    FileUtility.WriteAllText(string.Format("{0}/{1}.ply", directory, name), ply);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(string.Format("Failed writing PLY to path: {0}\n{1}", string.Format("{0}/{1}.ply", path, name), e.ToString()));
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
    }
}
