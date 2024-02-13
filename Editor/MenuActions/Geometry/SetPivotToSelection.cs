using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SetPivotToSelection : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override string iconPath => "Toolbar/Pivot_CenterOnElements";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return _tooltip; } }
        public override string menuTitle { get { return "Set Pivot To Selection"; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Set Pivot to Center of Selection",
                @"Moves the pivot point of each mesh to the average of all selected elements positions.  This means the pivot point moves to where-ever the handle currently is."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        public override bool enabled
        {
            get
            {
                return base.enabled && (MeshSelection.selectedVertexCount > 0
                                        || MeshSelection.selectedEdgeCount > 0
                                        || MeshSelection.selectedFaceCount > 0);
            }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            Object[] objects = new Object[MeshSelection.selectedObjectCount * 2];

            for (int i = 0, c = MeshSelection.selectedObjectCount; i < c; i++)
            {
                objects[i] = MeshSelection.topInternal[i];
                objects[i + c] = MeshSelection.topInternal[i].transform;
            }

            UndoUtility.RegisterCompleteObjectUndo(objects, "Set Pivot");

            foreach (var mesh in MeshSelection.topInternal)
            {
                TransformUtility.UnparentChildren(mesh.transform);
                mesh.CenterPivot(mesh.selectedIndexesInternal);
                mesh.Optimize();
                TransformUtility.ReparentChildren(mesh.transform);
            }

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Set Pivot");
        }
    }
}
