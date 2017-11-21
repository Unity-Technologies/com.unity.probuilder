using UnityEngine;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SelectFaceRing : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Ring_Face", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }
		public override int toolbarPriority { get { return 2; } }
		public override bool hasFileMenuEntry { get { return false; } }

		private static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Select Face Ring",
			"Selects a ring of connected faces.\n\n<b>Shortcut</b>: Control + Double Click on Face."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Face &&
			       	selection != null &&
			       	selection.Length > 0 &&
			       	selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Face;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuRingFaces(selection);
		}
	}
}
