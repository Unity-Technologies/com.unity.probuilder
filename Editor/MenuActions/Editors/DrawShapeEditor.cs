using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class DrawShapeEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Draw Shape"; } }
        public override int toolbarPriority { get { return 0; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Draw Shape Tool",
            "Opens the Shape Drawing Tool.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitives.",
            keyCommandSuper, keyCommandShift, 'K'
        );

        public override bool enabled
        {
            get { return true; }
        }

        public override ActionResult DoAction()
        {
            EditorTools.EditorTools.SetActiveTool<DrawShapeTool>();
            return new ActionResult(ActionResult.Status.Success, "Open Shape Tool");
        }
    }
}
