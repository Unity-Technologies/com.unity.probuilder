using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenMaterialEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override string iconPath => "Toolbar/Panel_Materials";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Material Editor",
                "Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects."
            );

        public override bool enabled => true;

        protected override ActionResult PerformActionImplementation()
        {
            MaterialEditor.MenuOpenMaterialEditor();
            return new ActionResult(ActionResult.Status.Success, "Open Materials Window");
        }
    }
}
