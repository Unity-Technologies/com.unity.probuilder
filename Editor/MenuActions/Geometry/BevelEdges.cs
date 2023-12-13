using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class BevelEdges : MenuAction
    {
        const float k_MinBevelDistance = .0001f;
        Pref<float> m_BevelSize = new Pref<float>("BevelEdges.size", .2f);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override string iconPath => "Toolbar/Edge_Bevel";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        static readonly GUIContent gc_BevelDistance = EditorGUIUtility.TrTextContent("Distance", "The size of the bevel in meters. The value is clamped to the size of the smallest affected face.");

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Bevel Edges",
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

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var floatField = new FloatField(gc_BevelDistance.text);
            floatField.tooltip = gc_BevelDistance.tooltip;
            floatField.isDelayed = PreviewActionManager.delayedPreview;
            floatField.SetValueWithoutNotify(m_BevelSize.value);
            floatField.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                if (m_BevelSize.value != evt.newValue)
                {
                    if (evt.newValue < k_MinBevelDistance)
                    {
                        m_BevelSize.SetValue(k_MinBevelDistance);
                        floatField.SetValueWithoutNotify(m_BevelSize);
                    }
                    else
                        m_BevelSize.SetValue(evt.newValue);
                    PreviewActionManager.UpdatePreview();
                }
            });
            root.Add(floatField);

            return root;
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Bevel Edge Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Amount determines how much space the bevel occupies. The value is clamped to the size of the smallest affected face.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_BevelSize.value = EditorGUILayout.FloatField(gc_BevelDistance, m_BevelSize);

            if (m_BevelSize < k_MinBevelDistance)
                m_BevelSize.value = k_MinBevelDistance;

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Bevel Edges"))
                PerformAction();
        }

        protected override ActionResult PerformActionImplementation()
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
