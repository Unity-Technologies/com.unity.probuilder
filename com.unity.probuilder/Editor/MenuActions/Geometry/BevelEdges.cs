using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class BevelEdges : MenuAction
    {
        Pref<float> m_BevelSize = new Pref<float>("BevelEdges.size", .2f);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Bevel", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Bevel",
                @"Smooth the selected edges by adding a slanted face connecting the two adjacent faces."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Face; }
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
            GUILayout.Label("Bevel Edge Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Amount determines how much space the bevel takes up.  Bigger value means more bevel action.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_BevelSize.value = UI.EditorGUIUtility.FreeSlider("Distance", m_BevelSize, .001f, .99f);

            if (m_BevelSize < .001f)
                m_BevelSize.value = .001f;

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Bevel Edges"))
                DoAction();
        }

        public override ActionResult DoAction()
        {
            ActionResult res = ActionResult.NoSelection;

            UndoUtility.RecordSelection("Bevel Edges");

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                pb.ToMesh();

                List<Face> faces = Bevel.BevelEdges(pb, pb.selectedEdges, m_BevelSize);
                res = faces != null ? new ActionResult(ActionResult.Status.Success, "Bevel Edges") : new ActionResult(ActionResult.Status.Failure, "Failed Bevel Edges");

                if (res)
                    pb.SetSelectedFaces(faces);

                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();

            return res;
        }
    }
}
