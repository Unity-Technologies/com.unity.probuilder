using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class DeleteVertices : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Vertex_Delete";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Delete Vertices",
                @"Delete all selected vertices and connected faces.",
                keyCommandDelete
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedVertexCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Delete Vertices");

            int count = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                if (pb.selectedVertexCount == pb.vertexCount)
                {
                    Debug.LogWarning("Attempting to delete all vertices on this mesh... operation aborted.");
                    continue;
                }

                Undo.RecordObject(pb.renderer, "Update Renderer");

                var removed = pb.DeleteVerticesAndConnectedFaces(pb.selectedCoincidentVertices);
                count += removed != null ? removed.Length : 0;

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            MeshSelection.ClearElementSelection();
            ProBuilderEditor.Refresh();

            if (count > 0)
                return new ActionResult(ActionResult.Status.Success, "Delete " + count + " Vertices");

            return new ActionResult(ActionResult.Status.Failure, "No Vertices Selected");
        }
    }
}
