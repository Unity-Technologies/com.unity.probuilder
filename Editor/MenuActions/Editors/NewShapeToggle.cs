using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using EditorToolManager = UnityEditor.EditorTools.EditorToolContext;
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewShapeToggle : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "New Shape"; } }
        public override int toolbarPriority { get { return 0; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "New Shape",
                "Opens the Shape window.\n\nThis tool allows you to interactively create new 3d primitives.",
                keyCommandSuper, keyCommandShift, 'K'
            );

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            ProBuilderEditor.selectMode = SelectMode.Object;
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
