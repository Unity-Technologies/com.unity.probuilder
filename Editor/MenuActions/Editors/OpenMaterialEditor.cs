using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenMaterialEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Materials", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Material Editor",
                "Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects."
            );

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            MaterialEditor.MenuOpenMaterialEditor();
            return new ActionResult(ActionResult.Status.Success, "Open Materials Window");
        }
    }
}
