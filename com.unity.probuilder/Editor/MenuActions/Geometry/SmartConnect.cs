using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SmartConnect : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Smart Connect",
			"",
			keyCommandAlt, 'E'
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.instance.componentMode != ComponentMode.Face &&
					MeshSelection.TopInternal().Any(x => x.selectedVertexCount > 1);
			}
		}

		public override bool hidden
		{
			get { return true; }
		}

		public override ActionResult DoAction()
		{
			switch (ProBuilderEditor.instance.componentMode)
			{
				case ComponentMode.Vertex:
					return MenuCommands.MenuConnectVertexes(MeshSelection.TopInternal());

				case ComponentMode.Edge:
				default:
					return MenuCommands.MenuConnectEdges(MeshSelection.TopInternal());

				// default:
				// 	return pb_Menu_Commands.MenuSubdivideFace(selection);
			}
		}
	}
}
