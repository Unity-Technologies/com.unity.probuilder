using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class GenerateUV2 : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Object_GenerateUV2", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Lightmap UVs",
                @"Generate Lightmap UVs for any meshes in the open scenes that are missing them."
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
            var res = Lightmapping.RebuildMissingLightmapUVs(Object.FindObjectsOfType<ProBuilderMesh>(), true);

            if (res < 1)
                return new ActionResult(ActionResult.Status.Success, "No Missing Lightmap UVs Found");

            return new ActionResult(ActionResult.Status.Success, "Generate Lightmap UVs\n" +
                (res > 1 ? string.Format("for {0} objects", res) : "for 1 object"));
        }

        protected override void DoAlternateAction()
        {
            EditorWindow.GetWindow<LightmapUVEditor>(true, "Lightmap UV Editor", true).position = LightmapUVEditor.desiredPosition;
        }
    }
}
