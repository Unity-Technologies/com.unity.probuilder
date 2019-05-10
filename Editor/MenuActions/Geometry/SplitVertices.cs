using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SplitVertices : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Vert_Split", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Split Vertices",
                @"Disconnects vertices that share the same position in space so that they may be moved independently of one another.",
                keyCommandAlt, 'X'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedVertexCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            int splitCount = 0;
            UndoUtility.RecordSelection("Split Vertices");

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                var coincident = mesh.selectedCoincidentVertices;
                splitCount += coincident.Count();
                mesh.SplitVertices(coincident);
            }

            ProBuilderEditor.Refresh();

            if (splitCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));
                
            return new ActionResult(ActionResult.Status.Failure, "Split Vertices\nInsuffient Vertices Selected");
        }
    }
}
