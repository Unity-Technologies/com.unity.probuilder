using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class InvertSelection : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Invert", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Invert Selection",
			@"Selects the opposite of the current selection. Eg, all unselected elements will become selected, the current selection will be unselected.",
			keyCommandSuper, keyCommandShift, 'I'
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null
				&& ProBuilderEditor.instance.editLevel != EditLevel.Top
				&& MeshSelection.Top().Length > 0;
		}

		public override bool IsHidden()
		{
			return editLevel != EditLevel.Geometry;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuInvertSelection(MeshSelection.Top());
		}
	}
}
