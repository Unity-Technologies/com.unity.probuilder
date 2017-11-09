using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class ShrinkSelection : pb_MenuAction
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
					pb_Menu_Commands.VerifyShrinkSelection(selection);
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuShrinkSelection(selection);
		}
	}
}
