using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class ToggleDragSelectionMode : pb_MenuAction
	{
		DragSelectMode dragSelectMode
		{
			get { return pb_PreferencesInternal.GetEnum<DragSelectMode>(pb_Constant.pbDragSelectMode); }
			set { pb_PreferencesInternal.SetInt(pb_Constant.pbDragSelectMode, (int) value); }
		}

		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon {
			get {
				if(dragSelectMode == DragSelectMode.Add)
					return pb_IconUtility.GetIcon("Toolbar/Selection_ShiftAdd", IconSkin.Pro);
				else if(dragSelectMode == DragSelectMode.Subtract)
					return pb_IconUtility.GetIcon("Toolbar/Selection_ShiftSubtract", IconSkin.Pro);
				else
					return pb_IconUtility.GetIcon("Toolbar/Selection_ShiftDifference", IconSkin.Pro);
			}
		}
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
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
				return string.Format("Shift: {0}", dragSelectMode);
			}
		}

		public override pb_ActionResult DoAction()
		{
			int mode = (int) dragSelectMode;
			dragSelectMode = (DragSelectMode) ((mode + 1) % 3);
			pb_Editor.instance.LoadPrefs();
			return new pb_ActionResult(Status.Success, "Set Shift Drag Mode\n" + dragSelectMode);
		}

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry;
		}
	}
}
