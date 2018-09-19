using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ConnectEdges : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Connect", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		protected override bool hasFileMenuEntry { get { return false; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Connect Edges",
			"Inserts a new edge connecting the center points of all selected edges.  See also \"Subdivide.\"",
			keyCommandAlt, 'E'
		);

		protected override SelectMode validSelectModes
		{
			get { return SelectMode.Edge; }
		}

		public override bool enabled
		{
			get { return base.enabled && ProBuilderEditor.instance.selectedEdgeCount > 1; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuConnectEdges(MeshSelection.TopInternal());
		}
	}
}
