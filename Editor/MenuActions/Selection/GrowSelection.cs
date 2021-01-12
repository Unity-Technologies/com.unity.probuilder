using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class GrowSelection : MenuAction
    {
        Pref<bool> m_GrowSelectionWithAngle = new Pref<bool>("GrowSelection.useAngle", true);
        Pref<bool> m_GrowSelectionAngleIterative = new Pref<bool>("GrowSelection.iterativeGrow", false);
        Pref<float> m_GrowSelectionAngleValue = new Pref<float>("GrowSelection.angleValue", 15f);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_Grow", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Grow Selection",
                @"Adds adjacent elements to the current selection, optionally testing to see if they are within a specified angle.

Grow by angle is enabled by Option + Clicking the <b>Grow Selection</b> button.",
                keyCommandAlt, 'G'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && VerifyGrowSelection(); }
        }

        protected override MenuActionState optionsMenuState
        {
            get
            {
                if (enabled && ProBuilderEditor.selectMode == SelectMode.Face)
                    return MenuActionState.VisibleAndEnabled;

                return MenuActionState.Hidden;
            }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Grow Selection Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_GrowSelectionWithAngle.value = EditorGUILayout.Toggle("Restrict to Angle", m_GrowSelectionWithAngle.value);

            GUI.enabled = m_GrowSelectionWithAngle;

            m_GrowSelectionAngleValue.value = EditorGUILayout.FloatField("Max Angle", m_GrowSelectionAngleValue);

            GUI.enabled = m_GrowSelectionWithAngle;

            bool iterative = m_GrowSelectionWithAngle ? m_GrowSelectionAngleIterative : true;

            EditorGUI.BeginChangeCheck();
            iterative = EditorGUILayout.Toggle("Iterative", iterative);
            if (EditorGUI.EndChangeCheck())
            {
                m_GrowSelectionAngleIterative.value = iterative;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Grow Selection"))
                PerformAction();
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Grow Selection");

            int grown = 0;
            bool angleGrow = m_GrowSelectionWithAngle;
            bool iterative = m_GrowSelectionAngleIterative;
            float growSelectionAngle = m_GrowSelectionAngleValue;

            if (!angleGrow && !iterative)
                iterative = true;

            foreach (ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
            {
                int previousTriCount = pb.selectedVertexCount;

                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Vertex:
                        pb.SetSelectedEdges(ElementSelection.GetConnectedEdges(pb, pb.selectedIndexesInternal));
                        break;

                    case SelectMode.Edge:
                        pb.SetSelectedEdges(ElementSelection.GetConnectedEdges(pb, pb.selectedIndexesInternal));
                        break;

                    case SelectMode.TextureFace:
                    case SelectMode.Face:

                        Face[] selectedFaces = pb.GetSelectedFaces();

                        HashSet<Face> sel;

                        if (iterative)
                        {
                            sel = ElementSelection.GrowSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
                            sel.UnionWith(selectedFaces);
                        }
                        else
                        {
                            sel = ElementSelection.FloodSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
                        }

                        pb.SetSelectedFaces(sel.ToArray());

                        break;
                }

                grown += pb.selectedVertexCount - previousTriCount;
            }

            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();

            if (grown > 0)
                return new ActionResult(ActionResult.Status.Success, "Grow Selection");

            return new ActionResult(ActionResult.Status.Failure, "Nothing to Grow");
        }

        static bool VerifyGrowSelection()
        {
            int sel, max;

            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Face:
                    sel = MeshSelection.selectedFaceCount;
                    max = MeshSelection.totalFaceCount;
                    break;

                case SelectMode.Edge:
                    sel = MeshSelection.selectedEdgeCount;
                    max = MeshSelection.totalEdgeCount;
                    break;

                default:
                    sel = MeshSelection.selectedVertexCount;
                    max = MeshSelection.totalVertexCount;
                    break;
            }

            return sel > 0 && sel < max;
        }
    }
}
