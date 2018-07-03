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

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.componentMode == ComponentMode.Edge &&
					MeshSelection.TopInternal().Sum(x => x.selectedEdgeCount) > 0;
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.componentMode != ComponentMode.Edge;
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuLoopSelection(MeshSelection.TopInternal());
		}
	}
}
