using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class InsertEdgeLoop : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Edge_InsertLoop", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Insert Edge Loop",
			@"Connects all edges in a ring around the object.",
			CMD_ALT, 'U'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Edge &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedEdgeCount > 0);
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Edge;

		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuInsertEdgeLoop(selection);
		}
	}
}
