using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SmartSubdivide : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Smart Subdivide",
			"",
			CMD_ALT, 'S'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode != SelectMode.Vertex &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedEdgeCount > 0);
		}

		public override bool IsHidden()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			switch(pb_Editor.instance.selectionMode)
			{
				case SelectMode.Edge:
					return pb_MenuCommands.MenuSubdivideEdge(selection);

				default:
					return pb_MenuCommands.MenuSubdivideFace(selection);
			}
		}
	}
}
