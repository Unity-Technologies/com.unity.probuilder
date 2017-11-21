using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;

namespace ProBuilder.Actions
{
	class OpenSmoothingEditor : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_Smoothing", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }
		public override string menuTitle { get { return "Smoothing"; } }
		public override bool isProOnly { get { return true; } }
		public override int toolbarPriority { get { return 2; } }
		static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Smoothing Groups Editor",
@"Opens the Smoothing Group Editor.

Smoothing groups average the vertex normals with neighboring planes. This allows lighting to behave in a more realistic manner when dealing with edges that are intended to be smooth.

ProBuilder decides which edges should be smoothed by checking for neighboring faces that are in the same group. It also checks for Hard groups, which hardens edges of neighboring faces."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			pb_SmoothGroupEditor.MenuOpenSmoothGroupEditor();
			return new pb_ActionResult(Status.Success, "Open Smoothing Groups Editor");
		}
	}
}
