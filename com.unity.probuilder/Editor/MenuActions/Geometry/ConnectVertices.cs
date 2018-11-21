using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ConnectVertices : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Vert_Connect", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return _tooltip; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Connect Vertices",
                @"Adds edges connecting all selected vertices.",
                keyCommandAlt, 'E'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedSharedVertexCount > 1; }
        }

        public override ActionResult DoAction()
        {
            ActionResult res = ActionResult.NoSelection;

            UndoUtility.RecordSelection("Connect Vertices");

            foreach (var mesh in MeshSelection.topInternal)
            {
                mesh.ToMesh();
                int[] splits = mesh.Connect(mesh.selectedIndexesInternal);

                if (splits != null)
                {
                    mesh.Refresh();
                    mesh.Optimize();
                    mesh.SetSelectedVertices(splits);
                    res = new ActionResult(ActionResult.Status.Success, "Connect Edges");
                }
                else
                {
                    res = new ActionResult(ActionResult.Status.Failure, "Failed Connecting Edges");
                }
            }
            ProBuilderEditor.Refresh();

            return res;
        }
    }
}
