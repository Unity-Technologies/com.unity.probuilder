using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class PointToPointCut : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Face_Subdivide", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Point to Point Cut",
            @"Inserts vertices in the selected mesh to subdivide it.",
            keyCommandAlt, 'P'
        );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            if (MeshSelection.selectedObjectCount > 1)
                return new ActionResult(ActionResult.Status.Failure, "Only one ProBuilder mesh must be selected");

            ProBuilderMesh firstMesh = MeshSelection.activeMesh;
            CutShape shape = Undo.AddComponent<CutShape>(firstMesh.gameObject);
            shape.cutEditMode = CutShape.CutEditMode.Path;

            return new ActionResult(ActionResult.Status.Success,"Point ot Point Cut Edition");
        }
    }
}
