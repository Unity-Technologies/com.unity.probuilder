using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SmartConnect : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Smart Connect",
			"",
			CMD_ALT, 'E'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode != SelectMode.Face &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedTriangleCount > 1);
		}

		public override bool IsHidden()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			switch(pb_Editor.instance.selectionMode)
			{
				case SelectMode.Vertex:
					return pb_MenuCommands.MenuConnectVertices(selection);

				case SelectMode.Edge:
				default:
					return pb_MenuCommands.MenuConnectEdges(selection);

				// default:
				// 	return pb_Menu_Commands.MenuSubdivideFace(selection);
			}
		}
	}
}
