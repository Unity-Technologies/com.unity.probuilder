using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SelectEdgeRing : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Ring", IconSkin.Pro); }
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
			"Select Edge Ring",
			"Selects a ring of edges.  Ringed edges are opposite the selected edge.\n\n<b>Shortcut</b>: Shift + Double-Click on Edge",
			keyCommandAlt, 'R'
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
			var selection = MeshSelection.topInternal;

			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Select Edge Ring");

			bool success = false;

			foreach(ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				Edge[] edges = ElementSelection.GetEdgeRing(pb, pb.selectedEdges).ToArray();

				if(edges.Length > pb.selectedEdgeCount)
					success = true;

				pb.SetSelectedEdges( edges );
			}

			ProBuilderEditor.Refresh();

			SceneView.RepaintAll();

			if(success)
				return new ActionResult(ActionResult.Status.Success, "Select Edge Ring");

			return new ActionResult(ActionResult.Status.Failure, "Nothing to Ring");
		}
	}
}
