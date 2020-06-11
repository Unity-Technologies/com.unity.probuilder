using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class VertexInsertionSingle : MenuAction
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
            get { return SelectMode.Vertex; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Single Vertex Insertion",
            @"Inserts a vertex in a face at a desire position and creates new edges accordingly.",
            keyCommandAlt, keyCommandShift, 'V'
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
                return new ActionResult(ActionResult.Status.Failure, "Only one ProBuilder object must be selected");

            ProBuilderMesh firstObj = MeshSelection.activeMesh;
            SubdivideOnVertex voFace = Undo.AddComponent<SubdivideOnVertex>(firstObj.gameObject);
            voFace.vertexEditMode = SubdivideOnVertex.VertexEditMode.Edit;

            return new ActionResult(ActionResult.Status.Success,"Single Vertex On Face Insertion");
        }
    }
}
