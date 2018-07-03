using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SelectFaceRing : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Ring_Face", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		public override int toolbarPriority
		{
			get { return 2; }
		}

		protected override bool hasFileMenuEntry
		{
			get { return false; }
		}

		private static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Select Face Ring",
			"Selects a ring of connected faces.\n\n<b>Shortcut</b>: Control + Double Click on Face."
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.componentMode == ComponentMode.Face &&
					MeshSelection.TopInternal().Sum(x => x.selectedFaceCount) > 0;
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.componentMode != ComponentMode.Face;
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuRingFaces(MeshSelection.TopInternal());
		}
	}
}
