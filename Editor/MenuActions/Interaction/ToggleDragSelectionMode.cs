using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleDragSelectionMode : MenuAction
    {
#if !UNITY_2023_2_OR_NEWER
        SelectionModifierBehavior modifier
        {
            get { return ProBuilderEditor.selectionModifierBehavior; }
            set { ProBuilderEditor.selectionModifierBehavior = value; }
        }
#endif

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get
            {
#if !UNITY_2023_2_OR_NEWER
                if (modifier == SelectionModifierBehavior.Add)
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftAdd", IconSkin.Pro);
                else if (modifier == SelectionModifierBehavior.Subtract)
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftSubtract", IconSkin.Pro);
                else
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
#else
                //Returning a default value for icon mode
                return IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
#endif
            }
        }

        public override TooltipContent tooltip
        {
            get { return _tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 0; }
        }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Set Drag Selection Mode",
#if UNITY_2023_2_OR_NEWER
                @"Obsolete, when drag selecting elements:

                - Use the Shift key to add elements to the selection
                - Use the Ctrl/Cmd key to subtract elements from the selection"
#else
                @"When drag selecting elements, does the shift key

                - [Add] Always add to the selection
                - [Subtract] Always subtract from the selection
                - [Difference] Invert the selection by the selected faces (Default)"
#endif
            );


#if UNITY_2023_2_OR_NEWER
        public override bool enabled => false;
#endif

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override string menuTitle
        {
#if !UNITY_2023_2_OR_NEWER
            get { return string.Format("Shift: {0}", modifier); }
#else
                get { return "Shift: Obsolete"; }
#endif
        }

        protected override ActionResult PerformActionImplementation()
        {
#if !UNITY_2023_2_OR_NEWER
            int mode = (int)modifier;
            int len = System.Enum.GetValues(typeof(SelectionModifierBehavior)).Length;
            modifier = (SelectionModifierBehavior)((mode + 1) % len);
            return new ActionResult(ActionResult.Status.Success, "Set Shift Drag Mode\n" + modifier);
#else
            return new ActionResult(ActionResult.Status.NoChange, "Set Shift Drag Mode is obsolete");
#endif
        }
    }
}
