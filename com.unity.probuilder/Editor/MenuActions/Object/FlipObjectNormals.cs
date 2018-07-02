using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed 	class FlipObjectNormals : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Object_FlipNormals", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		public override string menuTitle
		{
			get { return "Flip Normals"; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Flip Object Normals",
			@"Reverse the direction of all faces on the selected objects."
		);

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null && MeshSelection.TopInternal().Length > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFlipObjectNormals(MeshSelection.TopInternal());
		}
	}
}
