using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	class ConnectEdges : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Connect", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }
		public override bool hasFileMenuEntry { get { return false; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Connect Edges",
			"Inserts a new edge connecting the center points of all selected edges.  See also \"Subdivide.\"",
			CMD_ALT, 'E'
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.instance.selectionMode == SelectMode.Edge &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedEdgeCount > 1);
		}

		public override bool IsHidden()
		{
			return 	ProBuilderEditor.instance == null ||
					ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.instance.selectionMode != SelectMode.Edge;

		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuConnectEdges(selection);
		}
	}
}
