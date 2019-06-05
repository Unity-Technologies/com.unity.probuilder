using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenShapeEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "New Shape"; } }
        public override int toolbarPriority { get { return 0; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "New Shape Tool",
                "Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitives.",
                keyCommandSuper, keyCommandShift, 'K'
            );

        public override bool enabled
        {
            get { return true; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        public override ActionResult DoAction()
        {
            ShapeEditor.MenuCreateCube();
            return new ActionResult(ActionResult.Status.Success, "Create Shape");
        }

        protected override void DoAlternateAction()
        {
            ShapeEditor.MenuOpenShapeCreator();
        }
    }
}
