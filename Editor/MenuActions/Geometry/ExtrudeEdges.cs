using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ExtrudeEdges : MenuAction
    {
        Pref<float> m_ExtrudeEdgeDistance = new Pref<float>("ExtrudeEdges.distance", .5f);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override string iconPath => "Toolbar/Edge_Extrude";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        protected internal override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Extrude Edges",
                @"Adds a new face extending from the currently selected edges. Edges must have an open side to be extruded.
                NB : Allow non-manifold actions should be authorized in ProBuilder preferences to enable this action.",
                keyCommandSuper, 'E'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedEdgeCount > 0 && ProBuilderEditor.s_AllowNonManifoldActions; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var toggle = new Toggle("As Group");
            toggle.tooltip = "Extrude as Group determines whether or not adjacent faces stay attached to one another when extruding.";
            toggle.SetValueWithoutNotify(VertexManipulationTool.s_ExtrudeEdgesAsGroup);
            toggle.RegisterCallback<ChangeEvent<bool>>(OnEdgesAsGroupChanged);
            root.Add(toggle);

            var floatField = new FloatField("Distance");
            floatField.isDelayed = PreviewActionManager.delayedPreview;
            floatField.tooltip = "Extrude Amount determines how far an edge will be moved along it's normal when extruding. This value can be negative.";
            floatField.SetValueWithoutNotify(m_ExtrudeEdgeDistance);
            floatField.RegisterCallback<ChangeEvent<float>>(OnExtrudeChanged);
            root.Add(floatField);

            return root;
        }

        void OnEdgesAsGroupChanged(ChangeEvent<bool> evt)
        {
            VertexManipulationTool.s_ExtrudeEdgesAsGroup.SetValue(evt.newValue);
            PreviewActionManager.UpdatePreview();
        }

        void OnExtrudeChanged(ChangeEvent<float> evt)
        {
            m_ExtrudeEdgeDistance.SetValue(evt.newValue);
            PreviewActionManager.UpdatePreview();
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
