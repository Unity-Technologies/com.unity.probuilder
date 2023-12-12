using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;


namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectEdgeLoop : MenuAction
    {
        Pref<bool> m_SelectIterative = new Pref<bool>("SelectEdgeLoop.selectIterative", false);
        GUIContent gc_selectIterative = new GUIContent("Iterative Selection", "Optionally restrict the selection to neighbors edges on the loop.");

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => "Toolbar/Selection_Loop_Edge";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        protected internal override bool hasFileMenuEntry
        {
            get { return false; }
        }

        private static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Edge Loop",
                "Selects a loop of connected edges.\n\n<b>Shortcut</b>: Double-Click on Edge",
                keyCommandAlt, 'L'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        protected override MenuActionState optionsMenuState
        {
            get {
                if (enabled && ProBuilderEditor.selectMode == SelectMode.Edge)
                    return MenuActionState.VisibleAndEnabled;

                return MenuActionState.Hidden;
            }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedEdgeCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Select Edge Loop");

            bool foundLoop = false;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                Edge[] loop;
                bool success = false;

                if (m_SelectIterative)
                    success = ElementSelection.GetEdgeLoopIterative(pb, pb.selectedEdges, out loop);
                else
                    success = ElementSelection.GetEdgeLoop(pb, pb.selectedEdges, out loop);

                if (success)
                {
                    if (loop.Length > pb.selectedEdgeCount)
                        foundLoop = true;

                    pb.SetSelectedEdges(loop);
                }
            }

            ProBuilderEditor.Refresh();

            SceneView.RepaintAll();

            if (foundLoop)
                return new ActionResult(ActionResult.Status.Success, "Select Edge Loop");
            else
                return new ActionResult(ActionResult.Status.Failure, "Nothing to Loop");
        }

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var toggle = new Toggle(gc_selectIterative.text);
            toggle.tooltip = gc_selectIterative.tooltip;
            toggle.SetValueWithoutNotify(m_SelectIterative);
            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                m_SelectIterative.SetValue(evt.newValue);
                PreviewActionManager.UpdatePreview();
            });
            root.Add(toggle);

            return root;
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Select Loop Edge Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            m_SelectIterative.value = EditorGUILayout.Toggle(gc_selectIterative, m_SelectIterative);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select Edge Loop"))
            {
                PerformAction();
                SceneView.RepaintAll();
            }
        }
    }
}
