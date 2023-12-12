using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class TriangulateFaces : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Face_Triangulate";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Triangulate Faces",
                "Break all selected faces down to triangles."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            ActionResult res = ActionResult.NoSelection;

            UndoUtility.RecordSelection("Triangulate Faces");

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                mesh.ToMesh();
                Face[] triangulatedFaces = mesh.ToTriangles(mesh.selectedFacesInternal);
                mesh.Refresh();
                mesh.Optimize();
                mesh.SetSelectedFaces(triangulatedFaces);
                res = new ActionResult(ActionResult.Status.Success, string.Format("Triangulated {0} {1}", triangulatedFaces.Length, triangulatedFaces.Length < 2 ? "Face" : "Faces"));
            }

            ProBuilderEditor.Refresh();

            return res;
        }
    }
}
