using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenVertexPositionEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override string iconPath => string.Empty;
        public override Texture2D icon => null;
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Vertex Editor"; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Vertex Position Editor",
                "Opens the vertex positions editor window."
            );

        public override bool enabled => true;
        public override bool hidden => true;

        protected override ActionResult PerformActionImplementation()
        {
            VertexPositionEditor.MenuOpenVertexEditor();
            return new ActionResult(ActionResult.Status.Success, "Open Vertex Editor Window");
        }
    }
}
