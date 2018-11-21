using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SubdivideObject : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Object_Subdivide", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Subdivide Object",
                "Increase the number of edges and vertices on this object by creating 4 new quads in every face."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Subdivide Selection");

            int success = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                pb.ToMesh();

                if (pb.Subdivide())
                    success++;

                pb.Refresh();
                pb.Optimize();

                pb.SetSelectedVertices(new int[0]);
            }

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Subdivide " + success + " Objects");
        }
    }
}
