using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ShrinkSelection : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Shrink", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Shrink Selection",
			@"Removes elements on the edge of the current selection.",
			keyCommandAlt, keyCommandShift, 'G'
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MenuCommands.VerifyShrinkSelection(MeshSelection.TopInternal());
			}
		}

		public override bool hidden
		{
			get { return editLevel != EditLevel.Geometry; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuShrinkSelection(MeshSelection.TopInternal());
		}
	}
}
