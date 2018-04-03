using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class OpenShapeEditor : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Shape"; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"New Shape Tool",
			"Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitves.",
			CMD_SUPER, CMD_SHIFT, 'K'
		);

		public override bool IsEnabled()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			pb_ShapeEditor.MenuOpenShapeCreator();
			return new pb_ActionResult(Status.Success, "Open Shape Tool");
		}
	}
}
