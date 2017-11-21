using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class FlipObjectNormals : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Object_FlipNormals", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Flip Normals"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Flip Object Normals",
			@"Reverse the direction of all faces on the selected objects."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuFlipObjectNormals(selection);
		}
	}
}
