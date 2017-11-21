using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class MergeObjects : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Object_Merge", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Merge Objects",
			@"Merges all selected ProBuilder objects to a single mesh."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 1;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuMergeObjects(selection);
		}
	}
}
