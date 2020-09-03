// This script demonstrates how to create a new action that can be accessed from the ProBuilder toolbar.
// A new menu item is registered under "Geometry" actions called "Remove edges soft".

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
	/// <summary>
	/// This is the actual action that will be executed.
	/// </summary>
	[ProBuilderMenuAction]
	public class SoftDeleteEdgesAction : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return k_Tooltip; } }

		/// <summary>
		/// What to show in the hover tooltip window.
		/// TooltipContent is similar to GUIContent, with the exception that it also includes an optional params[]
		/// char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
		/// </summary>
		static readonly TooltipContent k_Tooltip = new TooltipContent
		(
			"Remove edges soft",
			"Remove selected edges and merge faces."
		);

		/// <summary>
		/// Determines if the action should be enabled or grayed out. Here when at least one edge is selected.
		/// </summary>
		/// <returns></returns>
		public override bool enabled
		{
			get { return MeshSelection.selectedEdgeCount > 0; }
		}

		/// <summary>
		/// This action is applicable in Edge selection modes.
		/// </summary>
		public override SelectMode validSelectModes
		{
			get { return SelectMode.Edge; }
		}

		/// <summary>
		/// Return a pb_ActionResult indicating the success/failure of action.
		/// </summary>
		/// <returns></returns>
		public override ActionResult DoAction()
		{
			var selection = MeshSelection.top.ToArray();
			Undo.RecordObjects(selection, "Removing edges");
			
			List<SimpleTuple<Vector3, Vector3>> edgesBounds;
			foreach (ProBuilderMesh pbMesh in selection)
			{
				if(pbMesh.selectedEdgeCount > 0)
				{
					var selectedEdges = pbMesh.selectedEdges;
					
					edgesBounds = selectedEdges.Select(edge => new SimpleTuple<Vector3,Vector3>(pbMesh.positions[edge.a], pbMesh.positions[edge.b])).ToList();
					
					for(int i= 0; i < edgesBounds.Count; i++)
					{	
						List<Face> facesToMerge = pbMesh.faces.Where(face => face.edges.ToList().Exists(e => IsEdgeEquivalent(pbMesh, e, edgesBounds[i]))).ToList();
						Face f = MergeElements.Merge(pbMesh, facesToMerge);
						
						pbMesh.ToMesh();
						pbMesh.Refresh();
						pbMesh.Optimize();
					}
					
					edgesBounds.Clear();		
				}
			}
			
			MeshSelection.ClearElementSelection();

			// Rebuild the pb_Editor caches
			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Edges removed");
		}
		
		bool IsEdgeEquivalent(ProBuilderMesh mesh, Edge edge1, SimpleTuple<Vector3, Vector3> edge2)
		{
			Vector3 edge1A = mesh.positions[edge1.a];
			Vector3 edge1B = mesh.positions[edge1.b];
			return ( edge1A == edge2.item1 && edge1B == edge2.item2 ) || ( edge1A == edge2.item2 && edge1B == edge2.item1 );
		}
		
	}
}
