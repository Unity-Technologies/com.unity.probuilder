using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class ConnectVertices : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Vert_Connect", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }
		public override bool hasFileMenuEntry { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Connect Vertices",
			@"Adds edges connecting all selected vertices.",
			CMD_ALT, 'E'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Vertex &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedTriangleCount > 1);
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Vertex;

		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuConnectVertices(selection);
		}
	}
}
