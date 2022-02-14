using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;
#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    /// <summary>
    /// Represents the [Cut tool](../manual/cut-tool.html) button on the [ProBuilder toolbar](../manual/toolbar.html) in the Editor.
    /// </summary>
    public class CutToolToggle : MenuToolToggle
    {
        /// <inheritdoc/>
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        /// <inheritdoc/>
        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/CutTool", IconSkin.Pro); }
        }

        /// <inheritdoc/>
        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        /// <inheritdoc/>
        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        /// <inheritdoc/>
        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Cut Tool",
            @"Create a cut in a face to subdivide it.",
            keyCommandAlt, keyCommandShift, 'C'
        );

        /// <inheritdoc/>
        public override bool enabled
        {
            get => MeshSelection.selectedObjectCount > 0;
        }

        /// <inheritdoc/>
        protected override ActionResult PerformActionImplementation()
        {
            m_Tool = ScriptableObject.CreateInstance<CutTool>();
            ToolManager.SetActiveTool(m_Tool);

            Undo.RegisterCreatedObjectUndo(m_Tool, "Open Cut Tool");

            MenuAction.onPerformAction += ActionPerformed;
            ToolManager.activeToolChanging += LeaveTool;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            Selection.selectionChanged += OnSelectionChanged;

            //Give the focus back to scene view to handle key inputs directly
            SceneView.lastActiveSceneView.Focus();

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Starts");
        }

        internal override ActionResult EndActivation()
        {
            MenuAction.onPerformAction -= ActionPerformed;
            ToolManager.activeToolChanging -= LeaveTool;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            Selection.selectionChanged -= OnSelectionChanged;

            Object.DestroyImmediate(m_Tool);

            ProBuilderEditor.instance.Repaint();

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Ends");
        }

        void ActionPerformed(MenuAction newActionPerformed)
        {
            if(ToolManager.IsActiveTool(m_Tool) && newActionPerformed.GetType() != this.GetType())
                LeaveTool();
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            if(!obj.IsPositionMode())
                LeaveTool();
        }

        void OnSelectionChanged()
        {
            if(MeshSelection.activeMesh == null
            || MeshSelection.selectedObjectCount != 1)
                LeaveTool();
            else
                ((CutTool)m_Tool).UpdateTarget();
        }

        void LeaveTool()
        {
            ActionResult result = EndActivation();
            EditorUtility.ShowNotification(result.notification);
        }
    }
}
