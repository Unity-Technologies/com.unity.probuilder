using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using System.Collections.Generic;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.MeshOperations;

namespace ProBuilder.Actions
{
	class SelectHole : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_SelectHole", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }
		public override bool isProOnly { get { return true; } }

		private static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Select Holes",
			"Selects holes on the mesh.\n\nUses the current element selection, or tests the whole mesh if no edges or vertices are selected."
		);

		public override bool IsEnabled()
		{
			if(pb_Editor.instance == null)
				return false;

			if(pb_Editor.instance.editLevel != EditLevel.Geometry)
				return false;

			if(pb_Editor.instance.selectionMode != SelectMode.Edge && pb_Editor.instance.selectionMode != SelectMode.Vertex)
				return false;

			if(selection == null || selection.Length < 1)
				return false;

			return true;
		}

		public override bool IsHidden()
		{
			if(pb_Editor.instance.editLevel != EditLevel.Geometry)
				return true;

			if(pb_Editor.instance.selectionMode != SelectMode.Edge && pb_Editor.instance.selectionMode != SelectMode.Vertex)
				return true;

			return false;
		}

		public override pb_ActionResult DoAction()
		{
			pb_Undo.RecordSelection(selection, "Select Hole");

			pb_ActionResult res = pb_ActionResult.NoSelection;

			foreach(pb_Object pb in selection)
			{
				bool selectAll = pb.SelectedTriangles == null || pb.SelectedTriangles.Length < 1;
				int[] indices = selectAll ? pb_Face.AllTriangles(pb.faces) : pb.SelectedTriangles;

				List<List<pb_Edge>> holes = pb_AppendPolygon.FindHoles(pb, indices);

				res = new pb_ActionResult(Status.Success, holes.Count > 0 ? string.Format("{0} holes found", holes.Count) : "No Holes in Selection");

				pb.SetSelectedEdges(holes.SelectMany(x => x));
			}

			pb_Editor.Refresh();

			return res;
		}
	}
}
