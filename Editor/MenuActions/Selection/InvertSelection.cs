using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class InvertSelection : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Invert", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Invert Selection",
			@"Selects the opposite of the current selection. Eg, all unselected elements will become selected, the current selection will be unselected.",
			CMD_SUPER, CMD_SHIFT, 'I'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && pb_Editor.instance.editLevel != EditLevel.Top && selection != null && selection.Length > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuInvertSelection(selection);
		}
	}
}


