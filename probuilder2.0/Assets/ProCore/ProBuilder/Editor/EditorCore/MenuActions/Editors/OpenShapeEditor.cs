using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class OpenShapeEditor : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_Shapes"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string MenuTitle { get { return "New Shape"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Shape Editor",
			"Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitves.",
			CMD_SUPER, CMD_SHIFT, 'K'
		);

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			pb_Geometry_Interface.MenuOpenShapeCreator();
			return new pb_ActionResult(Status.Success, "Open Shape Window");
		}
	}
}
