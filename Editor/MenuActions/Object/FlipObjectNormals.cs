using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed  class FlipObjectNormals : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }
        public override string iconPath => "Toolbar/Object_FlipNormals";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override string menuTitle
        {
            get { return "Flip Normals"; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Flip Object Normals",
                @"Reverse the direction of all faces on the selected objects."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Flip Object Normals");

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                foreach (var face in pb.facesInternal)
                    face.Reverse();
                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();
            return new ActionResult(ActionResult.Status.Success, "Flip Object Normals");
        }
    }
}
