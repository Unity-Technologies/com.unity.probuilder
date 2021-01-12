using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenLightmapUVEditor : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return null; } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Lightmap UV Editor"; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Lightmap UV Editor",
                ""
            );

        public override bool enabled
        {
            get { return true; }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            EditorWindow.GetWindow<LightmapUVEditor>(true, "Lightmap UV Editor", true).position = LightmapUVEditor.desiredPosition;
            return new ActionResult(ActionResult.Status.Success, "Open Lightmap UV Editor Window");
        }
    }
}
