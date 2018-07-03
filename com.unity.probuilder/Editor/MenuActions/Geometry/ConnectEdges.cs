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

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.componentMode == ComponentMode.Edge &&
					MeshSelection.TopInternal().Any(x => x.selectedEdgeCount > 1);
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.componentMode != ComponentMode.Edge;
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuConnectEdges(MeshSelection.TopInternal());
		}
	}
}
