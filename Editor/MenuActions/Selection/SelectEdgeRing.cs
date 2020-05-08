using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectEdgeRing : MenuAction
    {
        Pref<bool> m_SelectIterative = new Pref<bool>("SelectEdgeRing.selectIterative", false);
        GUIContent gc_selectIterative = new GUIContent("Iterative Selection", "Optionally restrict the selection to neighbors edges on the ring.");

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_Ring", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 2; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        protected override MenuActionState optionsMenuState
        {
            get {
                if (enabled && ProBuilderEditor.selectMode == SelectMode.Edge)
                    return MenuActionState.VisibleAndEnabled;

                return MenuActionState.Hidden;
            }
        }

        private static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Edge Ring",
                "Selects a ring of edges.  Ringed edges are opposite the selected edge.\n\n<b>Shortcut</b>: Shift + Double-Click on Edge",
                keyCommandAlt, 'R'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedEdgeCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Select Edge Ring");

            bool success = false;

            foreach (var mesh in MeshSelection.topInternal)
            {
                Edge[] edges;

                if (m_SelectIterative)
                {
                    edges = ElementSelection.GetEdgeRingIterative(mesh, mesh.selectedEdges).ToArray();
                }
                else
                {
                    edges = ElementSelection.GetEdgeRing(mesh, mesh.selectedEdges).ToArray();
                }
               
                if (edges.Length > mesh.selectedEdgeCount)
                    success = true;

                mesh.SetSelectedEdges(edges);
            }

            ProBuilderEditor.Refresh();

            SceneView.RepaintAll();

            if (success)
                return new ActionResult(ActionResult.Status.Success, "Select Edge Ring");

            return new ActionResult(ActionResult.Status.Failure, "Nothing to Ring");
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Select Ring Edge Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            m_SelectIterative.value = EditorGUILayout.Toggle(gc_selectIterative, m_SelectIterative);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select Edge Ring"))
            {
                DoAction();
                SceneView.RepaintAll();
            }
        }
    }
}
