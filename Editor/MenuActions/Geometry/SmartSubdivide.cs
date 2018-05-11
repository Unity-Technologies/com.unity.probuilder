using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SmartSubdivide : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return null; }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Smart Subdivide",
			"",
			keyCommandAlt, 'S'
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode != SelectMode.Vertex &&
				MeshSelection.Top().Any(x => x.selectedEdgeCount > 0);
		}

		public override bool IsHidden()
		{
			return true;
		}

		public override ActionResult DoAction()
		{
			switch (ProBuilderEditor.instance.selectionMode)
			{
				case SelectMode.Edge:
					return MenuCommands.MenuSubdivideEdge(MeshSelection.Top());

				default:
					return MenuCommands.MenuSubdivideFace(MeshSelection.Top());
			}
		}
	}
}
