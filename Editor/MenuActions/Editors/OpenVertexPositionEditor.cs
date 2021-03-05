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
        public override Texture2D icon { get { return null; } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Vertex Editor"; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Vertex Position Editor",
                "Opens the vertex positions editor window."
            );

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            VertexPositionEditor.MenuOpenVertexEditor();
            return new ActionResult(ActionResult.Status.Success, "Open Vertex Editor Window");
        }
    }
}
