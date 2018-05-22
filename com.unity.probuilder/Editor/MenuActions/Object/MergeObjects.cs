using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class MergeObjects : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Object_Merge", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Merge Objects",
			@"Merges all selected ProBuilder objects to a single mesh."
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null && MeshSelection.Top().Length > 1;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuMergeObjects(MeshSelection.Top());
		}
	}
}
