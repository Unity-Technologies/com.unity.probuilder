using UnityEngine.ProBuilder;
using UnityEngine;

using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewPolyShapeAction : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/CreatePolyShape"); } }
        public override TooltipContent tooltip { get { return _tooltip; } }
        public override string menuTitle { get { return "New Poly Shape"; } }
        public override int toolbarPriority { get { return 1; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "New Polygon Shape",
                "Creates a new shape by clicking around a perimeter and extruding."
            );

        public override bool hidden
        {
            get { return false; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            ToolManager.SetActiveTool(EditorToolManager.GetSingleton<DrawPolyShapeTool>());

            return new ActionResult(ActionResult.Status.Success,"Create Poly Shape");
        }
    }
}
