using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenVertexColorEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_VertColors", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Vertex Colors"; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Vertex Colors Editor",
                "Opens either the Vertex Color Palette or the Vertex Color Painter.\n\nThe Palette is useful for applying colors to selected faces with hard edges, where the Painter is good for brush strokes and soft edges.\n\nTo select which editor this button opens, Option + Click."
            );

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }

        public override ActionResult DoAction()
        {
            VertexColorPalette.MenuOpenWindow();
            return new ActionResult(ActionResult.Status.Success, "Open Vertex Color Window");
        }
    }
}
