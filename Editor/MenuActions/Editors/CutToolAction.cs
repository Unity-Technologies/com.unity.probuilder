using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder.Actions
{
    /// <summary>
    /// Represents the [Cut tool](../manual/cut-tool.html) button on the [ProBuilder toolbar](../manual/toolbar.html) in the Editor.
    /// </summary>
    public class CutToolAction : MenuAction
    {
        /// <inheritdoc/>
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Tool; }
        }

        /// <inheritdoc/>
        public override string iconPath => "Toolbar/CutTool";

        /// <inheritdoc/>
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

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
        protected internal override bool hasFileMenuEntry
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
            get => MeshSelection.selectedObjectCount == 1;
        }

        /// <inheritdoc/>
        protected override ActionResult PerformActionImplementation()
        {
            if(ToolManager.activeToolType == typeof(CutTool))
                ToolManager.RestorePreviousTool();
            else
                ToolManager.SetActiveTool<CutTool>();

            //Give the focus back to scene view to handle key inputs directly
            SceneView.lastActiveSceneView.Focus();
            return new ActionResult(ActionResult.Status.Success,"Cut Tool Starts");
        }

    }
}
