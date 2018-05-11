using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SelectEdgeLoop : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Loop", IconSkin.Pro); }
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
			"Select Edge Loop",
			"Selects a loop of connected edges.\n\n<b>Shortcut</b>: Double-Click on Edge",
			keyCommandAlt, 'L'
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode == SelectMode.Edge &&
				MeshSelection.Top().Sum(x => x.selectedEdgeCount) > 0;
		}

		public override bool IsHidden()
		{
			return ProBuilderEditor.instance == null ||
				ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
				ProBuilderEditor.instance.selectionMode != SelectMode.Edge;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuLoopSelection(MeshSelection.Top());
		}
	}
}
