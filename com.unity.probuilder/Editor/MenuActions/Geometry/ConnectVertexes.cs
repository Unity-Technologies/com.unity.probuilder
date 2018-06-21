using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ConnectVertexes : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Vert_Connect", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		protected override bool hasFileMenuEntry { get { return false; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Connect Vertexes",
			@"Adds edges connecting all selected vertexes.",
			keyCommandAlt, 'E'
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.componentMode == ComponentMode.Vertex &&
				MeshSelection.TopInternal().Any(x => x.selectedVertexCount > 1);
		}

		public override bool IsHidden()
		{
			return 	ProBuilderEditor.instance == null ||
					ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.instance.componentMode != ComponentMode.Vertex;

		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuConnectVertexes(MeshSelection.TopInternal());
		}
	}
}
