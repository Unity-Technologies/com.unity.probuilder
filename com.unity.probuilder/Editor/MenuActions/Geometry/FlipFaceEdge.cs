using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class FlipFaceEdge : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Face_FlipTri", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Flip Face Edge",
			@"Reverses the direction of the middle edge in a quad.  Use this to fix ridges in quads with varied height corners."
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Face; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFlipEdges(MeshSelection.TopInternal());
		}
	}
}
