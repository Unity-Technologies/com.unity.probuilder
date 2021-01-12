using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SubdivideEdges : MenuAction
    {
        Pref<int> m_SubdivisionCount = new Pref<int>("SubdivideEdges.subdivisions", 1);
        Pref<int> m_SubdivisionUIMin = new Pref<int>("SubdivideEdges.subdivisionsUIMin", 1);
        Pref<int> m_SubdivisionUIMax = new Pref<int>("SubdivideEdges.subdivisionsUIMax", 32);
        Pref<bool> m_SubdivisionRangeExpanded = new Pref<bool>("SubdivideEdges.rangeExpanded", false);
        const int m_SubdivisionMin = 1;
        const int m_SubdivisionMax = 512;

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
            int minUIRange = m_SubdivisionUIMin.value;
            int maxUIRange = m_SubdivisionUIMax.value;
            bool expanded = m_SubdivisionRangeExpanded.value;
            m_SubdivisionCount.value = (int)UI.EditorGUIUtility.FreeSliderWithRange("Subdivisions", (int)m_SubdivisionCount.value, m_SubdivisionMin, m_SubdivisionMax, ref minUIRange, ref maxUIRange, ref expanded);
            m_SubdivisionUIMin.value = minUIRange;
            m_SubdivisionUIMax.value = maxUIRange;
            m_SubdivisionRangeExpanded.value = expanded;


            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Subdivide Edges"))
                PerformAction();
        }

        protected override ActionResult PerformActionImplementation()
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
