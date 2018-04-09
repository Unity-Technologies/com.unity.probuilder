using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class ShrinkSelection : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Shrink", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Shrink Selection",
			@"Removes elements on the edge of the current selection.",
			CMD_ALT, CMD_SHIFT, 'G'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_MenuCommands.VerifyShrinkSelection(selection);
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuShrinkSelection(selection);
		}
	}
}
