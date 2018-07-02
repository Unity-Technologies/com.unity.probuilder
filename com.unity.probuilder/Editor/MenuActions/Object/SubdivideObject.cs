using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SubdivideObject : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Object_Subdivide", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Subdivide Object",
			"Increase the number of edges and vertexes on this object by creating 4 new quads in every face."
		);

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null && MeshSelection.TopInternal().Length > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuSubdivide(MeshSelection.TopInternal());
		}
	}
}
