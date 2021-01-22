using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ExtrudeEdges : MenuAction
    {
        Pref<float> m_ExtrudeEdgeDistance = new Pref<float>("ExtrudeEdges.distance", .5f);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Extrude", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Extrude Edges",
                @"Adds a new face extending from the currently selected edges.  Edges must have an open side to be extruded.",
                keyCommandSuper, 'E'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedEdgeCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Extrude Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Extrude Amount determines how far an edge will be moved along it's normal when extruding.  This value can be negative.\n\nExtrude as Group determines whether or not adjacent faces stay attached to one another when extruding.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            VertexManipulationTool.s_ExtrudeEdgesAsGroup.value = EditorGUILayout.Toggle("As Group", VertexManipulationTool.s_ExtrudeEdgesAsGroup);

            m_ExtrudeEdgeDistance.value = EditorGUILayout.FloatField("Distance", m_ExtrudeEdgeDistance);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Extrude Edges"))
                PerformAction();
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Extrude");

            int extrudedFaceCount = 0;
            bool success = false;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                pb.ToMesh();
                pb.Refresh(RefreshMask.Normals);

                if (pb.selectedEdgeCount < 1)
                    continue;

                extrudedFaceCount += pb.selectedEdgeCount;

                Edge[] newEdges = pb.Extrude(pb.selectedEdges,
                        m_ExtrudeEdgeDistance,
                        VertexManipulationTool.s_ExtrudeEdgesAsGroup,
                        ProBuilderEditor.s_AllowNonManifoldActions);

                success |= newEdges != null;

                if (success)
                    pb.SetSelectedEdges(newEdges);
                else
                    extrudedFaceCount -= pb.selectedEdgeCount;

                pb.Rebuild();
            }

            ProBuilderEditor.Refresh();

            if (extrudedFaceCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Extrude");

            return new ActionResult(ActionResult.Status.Canceled, "Extrude\nEmpty Selection");
        }
    }
}
