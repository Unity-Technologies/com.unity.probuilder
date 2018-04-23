using ProBuilder.Core;
using UnityEditor.ProBuilder;
using UnityEngine;

namespace ProBuilder.Actions
{
	class OpenSmoothingEditor : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Smoothing", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return m_Tooltip; } }
		public override string menuTitle { get { return "Smoothing"; } }
		public override bool isProOnly { get { return true; } }
		public override int toolbarPriority { get { return 2; } }
		static readonly TooltipContent m_Tooltip = new TooltipContent
		(
			"Smoothing Groups Editor",
@"Opens the Smoothing Group Editor.

Smoothing groups average the vertex normals with neighboring planes. This allows lighting to behave in a more realistic manner when dealing with edges that are intended to be smooth.

ProBuilder decides which edges should be smoothed by checking for neighboring faces that are in the same group. It also checks for Hard groups, which hardens edges of neighboring faces."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			SmoothGroupEditor.MenuOpenSmoothGroupEditor();
			return new pb_ActionResult(Status.Success, "Open Smoothing Groups Editor");
		}
	}
}
