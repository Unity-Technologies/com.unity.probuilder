using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class BridgeEdges : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Edge_Bridge", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Bridge Edges",
			@"Add a new face connecting two edges.",
			keyCommandAlt, 'B'
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Edge; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.topInternal.Any(x => x.selectedEdgeCount == 2); }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuBridgeEdges(MeshSelection.topInternal);
		}
	}
}
