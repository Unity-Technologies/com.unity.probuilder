using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class CenterPivot : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Pivot_CenterOnObject", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Center Pivot",
			@"Set the pivot point of this object to the center of it's bounds."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuCenterPivot(selection);
		}
	}
}
