using UnityEngine.ProBuilder;
using UnityEngine;
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewShapeAction : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/AddShape"); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "New Shape"; } }
        public override int toolbarPriority { get { return 0; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "New Shape",
                "Opens the Shape window.\n\nThis tool allows you to interactively create new 3d primitives.",
                keyCommandSuper, keyCommandShift, 'K'
            );

        protected override ActionResult PerformActionImplementation()
        {
            MeshSelection.SetSelection((GameObject)null);

            if(EditorToolManager.activeTool is DrawShapeTool active)
                Object.DestroyImmediate(active);
            else
                ToolManager.SetActiveTool<DrawShapeTool>();

            SceneView.RepaintAll();

            return new ActionResult(ActionResult.Status.Success,"Draw Shape Tool");
        }
    }
}
