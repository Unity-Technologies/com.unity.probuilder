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
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Edge_InsertLoop", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Insert Edge Loop",
			@"Connects all edges in a ring around the object.",
			keyCommandAlt, 'U'
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Edge; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedEdgeCount > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuInsertEdgeLoop(MeshSelection.TopInternal());
		}
	}
}
