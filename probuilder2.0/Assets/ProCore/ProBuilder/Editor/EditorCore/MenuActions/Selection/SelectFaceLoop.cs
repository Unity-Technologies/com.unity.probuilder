using UnityEngine;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class SelectFaceLoop : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Select Face Loop",
			"Selects a loop of connected faces.\n\n<b>Shortcut</b>: Shift + Double Click on Face."
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
			// not in toolbar
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuLoopFaces(selection);
		}
	}
}
