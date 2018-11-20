using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SubdivideEdges : MenuAction
    {
        Pref<int> m_SubdivisionCount = new Pref<int>("SubdivideEdges.subdivisions", 1);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Edge_Subdivide", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Subdivide Edges",
                "Appends evenly spaced new vertices to the selected edges.",
                keyCommandAlt, 'S'
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
            GUILayout.Label("Subdivide Edge Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox("How many vertices to insert on each selected edge.\n\nVertices will be equally spaced between one another and the boundaries of the edge.", MessageType.Info);

            m_SubdivisionCount.value = (int)UI.EditorGUIUtility.FreeSlider("Subdivisions", m_SubdivisionCount, 1, 32);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Subdivide Edges"))
                DoAction();
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            int subdivisions = m_SubdivisionCount;

            UndoUtility.RecordSelection("Subdivide Edges");

            ActionResult result = ActionResult.NoSelection;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                List<Edge> newEdgeSelection = AppendElements.AppendVerticesToEdge(pb, pb.selectedEdges, subdivisions);

                if (newEdgeSelection != null)
                {
                    pb.SetSelectedEdges(newEdgeSelection);
                    pb.ToMesh();
                    pb.Refresh();
                    pb.Optimize();
                    result = new ActionResult(ActionResult.Status.Success, "Subdivide Edge");
                }
                else
                {
                    result = new ActionResult(ActionResult.Status.Failure, "Failed Subdivide Edge");
                }
            }

            ProBuilderEditor.Refresh();

            return result;
        }
    }
}
