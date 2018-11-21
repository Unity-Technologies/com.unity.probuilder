using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Threading;
using UnityEngine.ProBuilder.Stl;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.EditorUtility;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ExportStlAscii : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Export; } }
        public override Texture2D icon { get { return null; } }
        public override TooltipContent tooltip { get { return _tooltip; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Export Stl",
                @"Export an Stl model file."
            );

        public override bool hidden
        {
            get { return true; }
        }

        public override bool enabled
        {
            get { return Selection.gameObjects != null && Selection.gameObjects.Length > 0; }
        }

        public override ActionResult DoAction()
        {
            var res = ExportWithFileDialog(Selection.gameObjects, FileType.Ascii);

            if (string.IsNullOrEmpty(res))
                return new ActionResult(ActionResult.Status.Canceled, "User Canceled");

            Export.PingExportedModel(res);

            return new ActionResult(ActionResult.Status.Success, "Export STL");
        }

        public static string ExportWithFileDialog(GameObject[] gameObjects, FileType type)
        {
            GameObject first = gameObjects.FirstOrDefault(x => x.GetComponent<ProBuilderMesh>() != null);

            string name = first != null ? first.name : "Mesh";
            string path = UnityEditor.EditorUtility.SaveFilePanel("Save Mesh to STL", "", name, "stl");

            var res = false;
            var currentCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                // pb_Stl is an external lib
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                res = pb_Stl_Exporter.Export(path, gameObjects, type);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }


            if (res)
            {
                string full = path.Replace("\\", "/");

                // if the file was saved in project, ping it
                if (full.Contains(Application.dataPath))
                {
                    string relative = full.Replace(Application.dataPath, "Assets");
                    Object o = AssetDatabase.LoadAssetAtPath(relative, typeof(Object));
                    if (o != null)
                        EditorGUIUtility.PingObject(o);
                    AssetDatabase.Refresh();
                }
                return path;
            }

            return null;
        }
    }
}
