using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class SelectEdgeLoop : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Loop"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select Edge Loop",
			"Selects a loop of connected edges.\n\n<b>Shortcut</b>: Double-Click on Edge",
			CMD_ALT, 'L'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Edge &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedEdgeCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Edge;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuLoopSelection(selection);
		}
	}
}
