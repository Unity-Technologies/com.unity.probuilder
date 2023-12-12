using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class FlipFaceEdge : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Face_FlipTri";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Flip Face Edge",
                @"Reverses the direction of the middle edge in a quad.  Use this to fix ridges in quads with varied height corners."
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
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Flip Face Edges");
            int success = 0;
            int attempts = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                foreach (Face face in pb.selectedFacesInternal)
                {
                    if (pb.FlipEdge(face))
                        success++;
                }

                attempts++;

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();

            if (success > 0)
                return new ActionResult(ActionResult.Status.Success, "Flipped " + success + " Edges");

            return new ActionResult(ActionResult.Status.Failure, string.Format("Flip Edges\n{0}", attempts > 0 ? "Faces Must Be Quads" : "No Faces Selected"));
        }
    }
}
