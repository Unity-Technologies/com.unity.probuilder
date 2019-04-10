using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ConnectEdges : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Connect", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Connect Edges",
                "Inserts a new edge connecting the center points of all selected edges.  See also \"Subdivide.\"",
                keyCommandAlt, 'E'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedEdgeCountObjectMax > 1; }
        }

        public override ActionResult DoAction()
        {
            ActionResult res = ActionResult.NoSelection;

            UndoUtility.RecordSelection("Connect Edges");

            foreach (var mesh in MeshSelection.topInternal)
            {
                Edge[] connections;
                Face[] faces;

                res = ConnectElements.Connect(mesh, mesh.selectedEdges, out faces, out connections, true, true);

                if (connections != null)
                {
                    if (connections.Length != 0)
                        mesh.SetSelectedEdges(connections);
                    mesh.Refresh();
                    mesh.Optimize();
                }
            }

            ProBuilderEditor.Refresh();
            return res;
        }
    }
}
