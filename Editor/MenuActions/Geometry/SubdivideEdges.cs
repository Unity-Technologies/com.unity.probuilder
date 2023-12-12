using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;

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

        public override string iconPath => "Toolbar/Edge_Subdivide";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        protected internal override bool hasFileMenuEntry
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

        Vector2IntField m_RangeField;
        IntegerField m_SubdivCount;
        SliderInt m_Slider;

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            var tooltip = "How many vertices to insert on each selected edge. Vertices will be equally spaced between " +
                "one another and the boundaries of the edge.";

            var foldout = new Foldout();
            foldout.SetValueWithoutNotify(m_SubdivisionRangeExpanded.value);
            foldout.RegisterCallback<ChangeEvent<bool>>(OnFoldoutChanged);
            m_Slider = new SliderInt("Subdivisions", m_SubdivisionUIMin, m_SubdivisionUIMax);
            m_Slider.SetValueWithoutNotify(m_SubdivisionCount.value);
            m_Slider.tooltip = tooltip;
            m_Slider.RegisterCallback<ChangeEvent<int>>(OnSliderChanged);
            m_Slider.style.flexGrow = 1f;
            m_SubdivCount = new IntegerField();
            m_SubdivCount.isDelayed = PreviewActionManager.delayedPreview;
            m_SubdivCount.SetValueWithoutNotify(m_SubdivisionCount.value);
            m_SubdivCount.tooltip = tooltip;
            m_SubdivCount.RegisterCallback<ChangeEvent<int>>(OnCountChanged);
            m_SubdivCount.style.width = 40;
            line.Add(foldout);
            line.Add(m_Slider);
            line.Add(m_SubdivCount);
            root.Add(line);

            m_RangeField = new Vector2IntField("Range");
            m_RangeField.style.paddingLeft = new StyleLength(25);
            m_RangeField.SetValueWithoutNotify(new Vector2Int(m_SubdivisionUIMin.value, m_SubdivisionUIMax.value));
            m_RangeField.Q<Label>().style.flexGrow = 1;

            var label = m_RangeField.Q<IntegerField>("unity-x-input").Q<Label>();
            label.text = String.Empty;
            label.style.flexGrow = 0;
            label = m_RangeField.Q<IntegerField>("unity-y-input").Q<Label>();
            label.text = String.Empty;
            label.style.flexGrow = 0;

            var field = m_RangeField.Q<IntegerField>("unity-x-input");
            field.isDelayed = true;
            field.RegisterCallback<ChangeEvent<int>>(OnMinChanged);
            field.style.width = 40f;
            var child = field[0];
            child.style.display = DisplayStyle.None;
            child = m_RangeField.Q<IntegerField>("unity-x-input")[1];
            child.style.flexShrink = 0;

            field = m_RangeField.Q<IntegerField>("unity-y-input");
            field.isDelayed = true;
            field.RegisterCallback<ChangeEvent<int>>(OnMaxChanged);
            field.style.width = 40f;
            child = field[0];
            child.style.display = DisplayStyle.None;
            child = m_RangeField.Q<IntegerField>("unity-y-input")[1];
            child.style.flexShrink = 0;

            child = m_RangeField[1][2];
            child.style.display = DisplayStyle.None;

            m_RangeField.style.display = m_SubdivisionRangeExpanded.value ? DisplayStyle.Flex : DisplayStyle.None;
            root.Add(m_RangeField);

            return root;
        }

        void OnFoldoutChanged(ChangeEvent<bool> evt)
        {
            if(m_SubdivisionRangeExpanded.value == evt.newValue)
                return;

            m_SubdivisionRangeExpanded.SetValue(evt.newValue);
            m_RangeField.style.display = m_SubdivisionRangeExpanded.value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnSliderChanged(ChangeEvent<int> evt)
        {
            if(m_SubdivisionCount.value == evt.newValue)
                return;

            m_SubdivisionCount.SetValue(evt.newValue);
            m_SubdivCount.SetValueWithoutNotify(m_SubdivisionCount.value);
            PreviewActionManager.UpdatePreview();
        }

        void OnCountChanged(ChangeEvent<int> evt)
        {
            if(m_SubdivisionCount.value == evt.newValue)
                return;

            m_SubdivisionCount.SetValue(evt.newValue < 1 ? 1 : evt.newValue);
            if (m_SubdivisionCount.value < m_SubdivisionUIMin.value)
            {
                m_SubdivisionUIMin.SetValue(m_SubdivisionCount.value);
                m_Slider.lowValue = m_SubdivisionUIMin.value;
                m_RangeField.SetValueWithoutNotify(new Vector2Int(m_SubdivisionUIMin.value, m_SubdivisionUIMax.value));
            }
            if (m_SubdivisionCount.value > m_SubdivisionUIMax.value)
            {
                m_SubdivisionUIMax.SetValue(m_SubdivisionCount.value);
                m_Slider.highValue = m_SubdivisionUIMax.value;
                m_RangeField.SetValueWithoutNotify(new Vector2Int(m_SubdivisionUIMin.value, m_SubdivisionUIMax.value));
            }
            m_Slider.SetValueWithoutNotify(m_SubdivisionCount.value);
            PreviewActionManager.UpdatePreview();
        }

        void OnMinChanged(ChangeEvent<int> evt)
        {
            if(m_SubdivisionUIMin.value == evt.newValue)
                return;

            m_SubdivisionUIMin.SetValue(evt.newValue < 1 ? 1 : evt.newValue);
            m_Slider.lowValue = m_SubdivisionUIMin.value;
            m_RangeField.SetValueWithoutNotify(new Vector2Int(m_SubdivisionUIMin.value, m_SubdivisionUIMax.value));
            if (m_SubdivisionCount.value < m_SubdivisionUIMin.value)
            {
                m_SubdivisionCount.SetValue(m_SubdivisionUIMin.value);
                m_SubdivCount.SetValueWithoutNotify(m_SubdivisionCount.value);
                m_Slider.SetValueWithoutNotify(m_SubdivisionCount.value);
                PreviewActionManager.UpdatePreview();
            }
        }

        void OnMaxChanged(ChangeEvent<int> evt)
        {
            if(m_SubdivisionUIMax.value == evt.newValue)
                return;

            m_SubdivisionUIMax.SetValue(evt.newValue);
            m_Slider.highValue = m_SubdivisionUIMax.value;
            m_RangeField.SetValueWithoutNotify(new Vector2Int(m_SubdivisionUIMin.value, m_SubdivisionUIMax.value));
            if (m_SubdivisionCount.value > m_SubdivisionUIMax.value)
            {
                m_SubdivisionCount.SetValue(m_SubdivisionUIMax.value);
                m_SubdivCount.SetValueWithoutNotify(m_SubdivisionCount.value);
                m_Slider.SetValueWithoutNotify(m_SubdivisionCount.value);
                PreviewActionManager.UpdatePreview();
            }
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
