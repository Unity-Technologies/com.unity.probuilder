using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    // Menu item entry
    sealed class SelectLoop : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => string.Empty;
        public override Texture2D icon => null;

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 2; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select Loop",
                "",
                keyCommandAlt, 'L'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get
            {
                if (ProBuilderEditor.selectMode == SelectMode.Edge)
                    return EditorToolbarLoader.GetInstance<SelectEdgeLoop>().enabled;
                else if (ProBuilderEditor.selectMode == SelectMode.Face)
                    return EditorToolbarLoader.GetInstance<SelectFaceLoop>().enabled;
                else
                    return false;
            }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (ProBuilderEditor.selectMode == SelectMode.Edge)
                return EditorToolbarLoader.GetInstance<SelectEdgeLoop>().PerformAction();
            else if (ProBuilderEditor.selectMode == SelectMode.Face)
                return EditorToolbarLoader.GetInstance<SelectFaceLoop>().PerformAction();
            return ActionResult.NoSelection;
        }

        internal override string GetMenuItemOverride()
        {
            return @"                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<SelectEdgeLoop>(), true));
                        break;
                    case SelectMode.Face:
                        EditorToolbarLoader.GetInstance<SelectFaceLoop>().PerformAction();
                        break;
                }";
        }
    }
}
