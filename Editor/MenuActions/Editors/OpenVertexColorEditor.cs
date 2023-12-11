using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenVertexColorEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override string iconPath => "Toolbar/Panel_VertColors";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Vertex Colors"; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Vertex Colors Editor",
                "Opens the Vertex Color Palette.\n\nApply using Face mode for hard-edged colors.\nApply using Edge or Vertex mode for soft, blended colors."
            );

        public override bool enabled => true;

        protected override ActionResult PerformActionImplementation()
        {
            VertexColorPalette.MenuOpenWindow();
            return new ActionResult(ActionResult.Status.Success, "Open Vertex Color Window");
        }
    }
}
