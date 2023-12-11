using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenSmoothingEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override string iconPath => "Toolbar/Panel_Smoothing";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Smoothing"; } }
        public override int toolbarPriority { get { return 2; } }
        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Smoothing Groups Editor",
                @"Opens the Smoothing Group Editor.

Smoothing groups average the vertex normals with neighboring planes. This allows lighting to behave in a more realistic manner when dealing with edges that are intended to be smooth.

ProBuilder decides which edges should be smoothed by checking for neighboring faces that are in the same group. It also checks for Hard groups, which hardens edges of neighboring faces."
            );

        public override bool enabled => true;

        protected override ActionResult PerformActionImplementation()
        {
            SmoothGroupEditor.MenuOpenSmoothGroupEditor();
            return new ActionResult(ActionResult.Status.Success, "Open Smoothing Groups Editor");
        }
    }
}
