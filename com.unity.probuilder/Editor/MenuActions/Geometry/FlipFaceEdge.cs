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

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MeshSelection.TopInternal().Any(x => x.selectedFaceCount > 0);
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.componentMode != ComponentMode.Face;
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFlipEdges(MeshSelection.TopInternal());
		}
	}
}

