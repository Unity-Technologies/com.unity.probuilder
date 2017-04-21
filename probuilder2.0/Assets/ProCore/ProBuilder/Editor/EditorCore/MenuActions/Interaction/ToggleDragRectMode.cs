using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	public class ToggleDragRectMode : pb_MenuAction
	{
		bool isComplete { get { return pb_Preferences_Internal.GetBool(pb_Constant.pbDragSelectWholeElement); } }

		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return isComplete ? pb_IconUtility.GetIcon("Toolbar/Selection_Rect_Complete") : pb_IconUtility.GetIcon("Toolbar/Selection_Rect_Intersect"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 3; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Drag Rect Mode", "Sets whether or not a mesh element (vertex, edge, or face) needs to be completely encompassed by a drag to be selected.\n\nThe default value is Intersect, meaning if any part of the elemnent is touched by the drag rectangle it will be selected."
		);

		public override string menuTitle { get { return isComplete ? "Rect: Complete" : "Rect: Intersect"; } }

		public override pb_ActionResult DoAction()
		{
			pb_Preferences_Internal.SetBool(pb_Constant.pbDragSelectWholeElement, !isComplete);
			return new pb_ActionResult(Status.Success, "Set Drag Select\n" + (isComplete ? "Complete" : "Intersect") );
		}

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Face;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Face;
		}
	}
}
