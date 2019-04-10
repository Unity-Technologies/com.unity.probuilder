using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ShrinkSelection : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_Shrink", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Shrink Selection",
                @"Removes elements on the edge of the current selection.",
                keyCommandAlt, keyCommandShift, 'G'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && VerifyShrinkSelection(); }
        }

        public override ActionResult DoAction()
        {
            var selection = MeshSelection.topInternal;
            var selectionCount = MeshSelection.selectedObjectCount;

            UndoUtility.RecordSelection("Shrink Selection");

            // find perimeter edges
            int rc = 0;
            for (int i = 0; i < selectionCount; i++)
            {
                ProBuilderMesh mesh = selection[i];

                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                    {
                        int[] perimeter = ElementSelection.GetPerimeterEdges(mesh, mesh.selectedEdges);
                        mesh.SetSelectedEdges(mesh.selectedEdges.RemoveAt(perimeter));
                        rc += perimeter != null ? perimeter.Length : 0;
                        break;
                    }

                    case SelectMode.TextureFace:
                    case SelectMode.Face:
                    {
                        Face[] perimeter = ElementSelection.GetPerimeterFaces(mesh, mesh.selectedFacesInternal).ToArray();
                        mesh.SetSelectedFaces(mesh.selectedFacesInternal.Except(perimeter).ToArray());
                        rc += perimeter.Length;
                        break;
                    }

                    case SelectMode.Vertex:
                    {
                        var universalEdges = mesh.GetSharedVertexHandleEdges(mesh.facesInternal.SelectMany(x => x.edges)).ToArray();
                        int[] perimeter = ElementSelection.GetPerimeterVertices(mesh, mesh.selectedIndexesInternal, universalEdges);
                        mesh.SetSelectedVertices(mesh.selectedIndexesInternal.RemoveAt(perimeter));
                        rc += perimeter != null ? perimeter.Length : 0;
                        break;
                    }
                }
            }

            ProBuilderEditor.Refresh();

            if (rc > 0)
                return new ActionResult(ActionResult.Status.Success, "Shrink Selection");

            return new ActionResult(ActionResult.Status.Canceled, "Nothing to Shrink");
        }

        static bool VerifyShrinkSelection()
        {
            int sel, max;

            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Face:
                    sel = MeshSelection.selectedFaceCount;
                    max = MeshSelection.totalFaceCount;
                    break;

                case SelectMode.Edge:
                    sel = MeshSelection.selectedEdgeCount;
                    max = MeshSelection.totalEdgeCount;
                    break;

                default:
                    sel = MeshSelection.selectedVertexCount;
                    max = MeshSelection.totalVertexCount;
                    break;
            }

            return sel > 1 && sel < max;
        }
    }
}
