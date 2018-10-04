using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class FreezeTransform : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Pivot_Reset", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Freeze Transform",
			@"Set the pivot point to world coordinates (0,0,0) and clear all Transform values while keeping the mesh in place."
		);

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFreezeTransforms(MeshSelection.topInternal);
		}
	}
}
