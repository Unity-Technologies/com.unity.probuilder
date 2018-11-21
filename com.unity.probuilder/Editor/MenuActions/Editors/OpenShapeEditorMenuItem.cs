using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenShapeEditorMenuItem : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return null; } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "New Shape"; } }
        public override int toolbarPriority { get { return 0; } }
        protected override bool hasFileMenuEntry { get { return true; } }

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

        public override ActionResult DoAction()
        {
            ShapeEditor.MenuOpenShapeCreator();
            return new ActionResult(ActionResult.Status.Success, "Open Shape Editor");
        }
    }
}
