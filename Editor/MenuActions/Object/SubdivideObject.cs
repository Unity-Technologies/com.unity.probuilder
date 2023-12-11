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
        public override string iconPath => "Toolbar/Object_Subdivide";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

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

        protected override ActionResult PerformActionImplementation()
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
                else
                    Debug.LogError($"Subidivision of [{pb.name}] failed, complex concave objects are not supported");

                pb.Refresh();
                pb.Optimize();

                pb.SetSelectedVertices(new int[0]);
            }

            ProBuilderEditor.Refresh();
            return new ActionResult(ActionResult.Status.Success, "Subdivide " + success + " Objects");
        }
    }
}
