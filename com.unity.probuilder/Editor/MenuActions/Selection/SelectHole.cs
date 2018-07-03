using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SelectHole : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_SelectHole", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		private static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Select Holes",
			"Selects holes on the mesh.\n\nUses the current element selection, or tests the whole mesh if no edges or vertexes are selected."
		);

		public override bool enabled
		{
			get
			{
				if (ProBuilderEditor.instance == null)
					return false;

				if (ProBuilderEditor.editLevel != EditLevel.Geometry)
					return false;

				if (ProBuilderEditor.componentMode != ComponentMode.Edge && ProBuilderEditor.componentMode != ComponentMode.Vertex)
					return false;

				if (MeshSelection.TopInternal().Length < 1)
					return false;

				return true;
			}
		}

		public override bool hidden
		{
			get
			{
				if (ProBuilderEditor.editLevel != EditLevel.Geometry)
					return true;

				if (ProBuilderEditor.componentMode != ComponentMode.Edge && ProBuilderEditor.componentMode != ComponentMode.Vertex)
					return true;

				return false;
			}
		}

		public override ActionResult DoAction()
		{
			UndoUtility.RecordSelection(MeshSelection.TopInternal(), "Select Hole");

			ActionResult res = ActionResult.NoSelection;

			foreach (ProBuilderMesh pb in MeshSelection.TopInternal())
			{
				bool selectAll = pb.selectedIndexesInternal == null || pb.selectedIndexesInternal.Length < 1;
				IEnumerable<int> indexes = selectAll ? pb.facesInternal.SelectMany(x => x.indexes) : pb.selectedIndexesInternal;

				List<List<Edge>> holes = ElementSelection.FindHoles(pb, indexes);

				res = new ActionResult(ActionResult.Status.Success, holes.Count > 0 ? string.Format("{0} holes found", holes.Count) : "No Holes in Selection");

				pb.SetSelectedEdges(holes.SelectMany(x => x));
			}

			ProBuilderEditor.Refresh();

			return res;
		}
	}
}
