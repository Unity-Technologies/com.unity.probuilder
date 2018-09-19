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
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Shrink Selection",
			@"Removes elements on the edge of the current selection.",
			keyCommandAlt, keyCommandShift, 'G'
		);

		protected override SelectMode validSelectModes
		{
			get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.Texture; }
		}

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null
					&& ProBuilderEditor.selectMode.HasFlag(validSelectModes)
					&& MenuCommands.VerifyShrinkSelection(MeshSelection.TopInternal());
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuShrinkSelection(MeshSelection.TopInternal());
		}
	}
}
