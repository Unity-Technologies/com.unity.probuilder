using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleDragSelectionMode : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get
            {
                //Returning a default value for icon mode
                return IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
            }
        }

        public override TooltipContent tooltip => _tooltip;

        public override int toolbarPriority => 0;

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Set Drag Selection Mode",
                @"Obsolete, when drag selecting elements:

                - Use the Shift key to add elements to the selection
                - Use the Ctrl/Cmd key to subtract elements from the selection"
            );

        public override bool enabled => false;

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override string menuTitle => "Shift: Obsolete";

        protected override ActionResult PerformActionImplementation()
        {
            return new ActionResult(ActionResult.Status.NoChange, "Set Shift Drag Mode is obsolete");
        }
    }
}
