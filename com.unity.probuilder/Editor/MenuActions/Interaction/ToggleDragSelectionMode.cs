using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ToggleDragSelectionMode : MenuAction
	{
		SelectionModifierBehavior SelectionModifierBehavior
		{
			get { return PreferencesInternal.GetEnum<SelectionModifierBehavior>(PreferenceKeys.pbDragSelectMode); }
			set { PreferencesInternal.SetInt(PreferenceKeys.pbDragSelectMode, (int) value); }
		}

		public override ToolbarGroup group { get { return ToolbarGroup.Selection; } }
		public override Texture2D icon {
			get {
				if(SelectionModifierBehavior == SelectionModifierBehavior.Add)
					return IconUtility.GetIcon("Toolbar/Selection_ShiftAdd", IconSkin.Pro);
				else if(SelectionModifierBehavior == SelectionModifierBehavior.Subtract)
					return IconUtility.GetIcon("Toolbar/Selection_ShiftSubtract", IconSkin.Pro);
				else
					return IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
			}
		}
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Set Drag Selection Mode",
@"When drag selecting elements, does the shift key

- [Add] Always add to the selection
- [Subtract] Always subtract from the selection
- [Difference] Invert the selection by the selected faces (Default)
");

		public override string menuTitle
		{
			get
			{
				return string.Format("Shift: {0}", SelectionModifierBehavior);
			}
		}

		public override ActionResult DoAction()
		{
			int mode = (int) SelectionModifierBehavior;
			int len = System.Enum.GetValues(typeof(SelectionModifierBehavior)).Length;
			SelectionModifierBehavior = (SelectionModifierBehavior) ((mode + 1) % len);
			ProBuilderEditor.instance.LoadPrefs();
			return new ActionResult(ActionResult.Status.Success, "Set Shift Drag Mode\n" + SelectionModifierBehavior);
		}

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry;
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry;
			}
		}
	}
}
