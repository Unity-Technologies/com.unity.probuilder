using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleDragSelectionMode : MenuAction
    {
        SelectionModifierBehavior modifier
        {
            get { return ProBuilderEditor.selectionModifierBehavior; }
            set { ProBuilderEditor.selectionModifierBehavior = value; }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get
            {
                if (modifier == SelectionModifierBehavior.Add)
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftAdd", IconSkin.Pro);
                else if (modifier == SelectionModifierBehavior.Subtract)
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftSubtract", IconSkin.Pro);
                else
                    return IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
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
                @"When drag selecting elements, does the shift key

- [Add] Always add to the selection
- [Subtract] Always subtract from the selection
- [Difference] Invert the selection by the selected faces (Default)
");

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override string menuTitle
        {
            get { return string.Format("Shift: {0}", modifier); }
        }

        public override ActionResult DoAction()
        {
            int mode = (int)modifier;
            int len = System.Enum.GetValues(typeof(SelectionModifierBehavior)).Length;
            modifier = (SelectionModifierBehavior)((mode + 1) % len);
            return new ActionResult(ActionResult.Status.Success, "Set Shift Drag Mode\n" + modifier);
        }
    }
}
