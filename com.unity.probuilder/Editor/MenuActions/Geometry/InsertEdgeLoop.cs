using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class InsertEdgeLoop : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_InsertLoop", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Insert Edge Loop",
			@"Connects all edges in a ring around the object.",
			keyCommandAlt, 'U'
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.componentMode == ComponentMode.Edge &&
					MeshSelection.TopInternal().Any(x => x.selectedEdgeCount > 0);
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
			return MenuCommands.MenuInsertEdgeLoop(MeshSelection.TopInternal());
		}
	}
}
