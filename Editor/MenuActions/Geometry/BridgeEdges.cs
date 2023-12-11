using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class BridgeEdges : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Edge_Bridge";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Bridge Edges",
                @"Add a new face connecting two edges.",
                keyCommandAlt, 'B'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.topInternal.Any(x => x.selectedEdgeCount == 2); }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Bridge Edges");

            bool success = false;

            foreach (var mesh in MeshSelection.topInternal)
            {
                if (mesh.selectedEdgeCount == 2)
                {
                    if (mesh.Bridge(mesh.selectedEdges[0], mesh.selectedEdges[1], ProBuilderEditor.s_AllowNonManifoldActions) != null)
                    {
                        success = true;
                        mesh.ToMesh();
                        mesh.Refresh();
                        mesh.Optimize();
                    }
                }
            }

            if (success)
            {
                ProBuilderEditor.Refresh();
                return new ActionResult(ActionResult.Status.Success, "Bridge Edges");
            }
            else
            {
                Debug.LogWarning("Failed Bridge Edges.  Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
                return new ActionResult(ActionResult.Status.Failure, "Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
            }
        }
    }
}
