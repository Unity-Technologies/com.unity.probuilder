using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleDragRectMode : MenuAction
    {
        RectSelectMode mode
        {
            get { return ProBuilderEditor.rectSelectMode; }
            set { ProBuilderEditor.rectSelectMode = value; }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => "Toolbar/DragSelect_Off";
        public override Texture2D icon => m_Icons[mode == RectSelectMode.Partial ? 0 : 1];

        internal Texture2D[] icons => m_Icons;
        Texture2D[] m_Icons;

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 0; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Set Drag Rect Mode",
                "Sets whether or not a mesh element (edge or face) needs to be completely encompassed by a drag to be selected.\n\nThe default value is Intersect, meaning if any part of the elemnent is touched by the drag rectangle it will be selected."
            );

        public override string menuTitle { get { return mode == RectSelectMode.Complete ? "Rect: Complete" : "Rect: Intersect"; } }

        public ToggleDragRectMode()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Toolbar/DragSelect_Off"),
                IconUtility.GetIcon("Toolbar/DragSelect_On")
            };
        }

        internal override bool IsMenuItemChecked()
        {
            return mode == RectSelectMode.Complete;
        }

        protected override ActionResult PerformActionImplementation()
        {
            mode = InternalUtility.NextEnumValue(mode);

            return new ActionResult(ActionResult.Status.Success,
                "Set Drag Select\n" + (mode == RectSelectMode.Complete ? "Complete" : "Intersect"));
        }

        public override bool enabled
        {
            get
            {
                return ProBuilderEditor.instance != null
                    && ProBuilderEditor.selectMode.ContainsFlag(SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace);
            }
        }
    }
}
