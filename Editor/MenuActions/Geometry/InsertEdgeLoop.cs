using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class InsertEdgeLoop : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Edge_InsertLoop", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return _tooltip; }
        }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Insert Edge Loop",
                @"Connects all edges in a ring around the object.",
                keyCommandAlt, 'U'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedEdgeCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            int success = 0;
            UndoUtility.RecordSelection("Insert Edge Loop");

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                Edge[] edges = pb.Connect(ElementSelection.GetEdgeRing(pb, pb.selectedEdges)).item2;

                if (edges != null)
                {
                    pb.SetSelectedEdges(edges);
                    pb.ToMesh();
                    pb.Refresh();
                    pb.Optimize();
                    success++;
                }
            }

            ProBuilderEditor.Refresh();

            if (success > 0)
                return new ActionResult(ActionResult.Status.Success, "Insert Edge Loop");

            return new ActionResult(ActionResult.Status.Success, "Insert Edge Loop");
        }
    }
}
