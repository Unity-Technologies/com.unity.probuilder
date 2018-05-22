using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class FlipFaceEdge : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_FlipTri", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Flip Face Edge",
			@"Reverses the direction of the middle edge in a quad.  Use this to fix ridges in quads with varied height corners."
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				MeshSelection.Top().Any(x => x.selectedFaceCount > 0);
		}

		public override bool IsHidden()
		{
			return ProBuilderEditor.instance == null ||
				ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
				ProBuilderEditor.instance.selectionMode != SelectMode.Face;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFlipEdges(MeshSelection.Top());
		}
	}
}

