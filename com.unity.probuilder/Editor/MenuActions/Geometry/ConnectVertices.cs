using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ConnectVertices : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Vert_Connect", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		protected override bool hasFileMenuEntry { get { return false; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Connect Vertices",
			@"Adds edges connecting all selected vertices.",
			keyCommandAlt, 'E'
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Vertex; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedSharedVertexCount > 1; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuConnectVertices(MeshSelection.TopInternal());
		}
	}
}
