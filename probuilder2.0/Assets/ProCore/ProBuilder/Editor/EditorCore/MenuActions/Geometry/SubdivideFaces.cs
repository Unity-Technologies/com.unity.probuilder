using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SubdivideFaces : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Face_Subdivide", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }
		public override bool hasFileMenuEntry { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Subdivide Faces",
			@"Inserts a new vertex at the center of each selected face and creates a new edge from the center of each perimeter edge to the center vertex.",
			CMD_ALT, 'S'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 0);
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Face;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuSubdivideFace(selection);
		}
	}
}

