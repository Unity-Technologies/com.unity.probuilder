using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class ToggleDragRectMode : pb_MenuAction
	{
		pb_RectSelectMode mode
		{
			get
			{
				return (pb_RectSelectMode) pb_PreferencesInternal.GetInt(pb_Constant.pbRectSelectMode,
					(int) pb_RectSelectMode.Partial);
			}

			set { pb_PreferencesInternal.SetInt(pb_Constant.pbRectSelectMode, (int) value); }
		}

		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }

		public override Texture2D icon
		{
			get
			{
				return mode == pb_RectSelectMode.Complete
					? pb_IconUtility.GetIcon("Toolbar/Selection_Rect_Complete")
					: pb_IconUtility.GetIcon("Toolbar/Selection_Rect_Intersect", IconSkin.Pro);
			}
		}
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Drag Rect Mode",
			"Sets whether or not a mesh element (edge or face) needs to be completely encompassed by a drag to be selected.\n\nThe default value is Intersect, meaning if any part of the elemnent is touched by the drag rectangle it will be selected."
		);

		public override string menuTitle { get { return mode == pb_RectSelectMode.Complete ? "Rect: Complete" : "Rect: Intersect"; } }

		public override pb_ActionResult DoAction()
		{
			mode = pb_Util.NextEnumValue(mode);
			return new pb_ActionResult(Status.Success,
				"Set Drag Select\n" + (mode == pb_RectSelectMode.Complete ? "Complete" : "Intersect"));
		}

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null &&
			       pb_Editor.instance.editLevel == EditLevel.Geometry &&
			       pb_Editor.instance.selectionMode != SelectMode.Vertex;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
			       	pb_Editor.instance.selectionMode == SelectMode.Vertex;
		}
	}
}
