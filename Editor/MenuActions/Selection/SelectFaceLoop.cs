using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SelectFaceLoop : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Loop_Face", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		public override int toolbarPriority
		{
			get { return 1; }
		}

		protected override bool hasFileMenuEntry
		{
			get { return false; }
		}

		private static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Select Face Loop",
			"Selects a loop of connected faces.\n\n<b>Shortcut</b>: Shift + Double Click on Face."
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode == SelectMode.Face &&
				MeshSelection.Top().Sum(x => x.selectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return ProBuilderEditor.instance == null ||
				ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
				ProBuilderEditor.instance.selectionMode != SelectMode.Face;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuLoopFaces(MeshSelection.Top());
		}
	}
}
