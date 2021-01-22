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
			"Remove Edges",
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
		protected override ActionResult PerformActionImplementation()
		{
			var selection = MeshSelection.top.ToArray();
			Undo.RecordObjects(selection, "Removing Edges");

			List<Face> edgeFaces = new List<Face>();

			Dictionary<Face, int> faceToMergeGroup = new Dictionary<Face, int>();
			HashSet<int> mergeGroupIDs = new HashSet<int>();
			List<List<Face>> mergeGroups = new List<List<Face>>();

			foreach (ProBuilderMesh pbMesh in selection)
			{
				if(pbMesh.selectedEdgeCount > 0)
				{
					var selectedEdges = pbMesh.selectedEdges;
					faceToMergeGroup.Clear();

					foreach(var edge in selectedEdges)
					{
						edgeFaces.Clear();
						mergeGroupIDs.Clear();

						//Retrieving impacted faces from edge
						ElementSelection.GetNeighborFaces(pbMesh, edge, edgeFaces);

						//Chacking all edges status
						foreach(var face in edgeFaces)
						{
							if(faceToMergeGroup.ContainsKey(face))
								mergeGroupIDs.Add(faceToMergeGroup[face]);
							else
								faceToMergeGroup.Add(face, -1);
						}

						//These faces haven't been seen before
						if(mergeGroupIDs.Count == 0)
						{
							foreach(var face in edgeFaces)
								faceToMergeGroup[face] = mergeGroups.Count;
							mergeGroups.Add(new List<Face>(edgeFaces));
						}
						//If only a face should already be merge, add other faces to the same merge group
						else if(mergeGroupIDs.Count == 1)
						{
							foreach(var face in edgeFaces)
							{
								if(faceToMergeGroup[face] == -1)
								{
									int index = mergeGroupIDs.First();
									faceToMergeGroup[face] = index;
									mergeGroups[index].Add(face);
								}
							}
						}
						//If more than a face already belongs to a merge group, merge these groups together
						else
						{
							//Group the different mergeGroups together
							List<Face> facesToMerge = new List<Face>();
							foreach(var groupID in mergeGroupIDs)
							{
								facesToMerge.AddRange(mergeGroups[groupID]);
								mergeGroups[groupID] = null;
							}

							foreach(var face in edgeFaces)
								if(!facesToMerge.Contains(face))
									facesToMerge.Add(face);

							//Remove unnecessary groups
							mergeGroups.RemoveAll(group => group == null);
							//Add newly created one
							mergeGroups.Add(facesToMerge);

							//Update groups references
							for(int i = 0; i < mergeGroups.Count; i++)
							{
								foreach(var face in mergeGroups[i])
									faceToMergeGroup[face] = i;
							}
						}
					}

					foreach(var mergeGroup in mergeGroups)
						MergeElements.Merge(pbMesh, mergeGroup);

					pbMesh.ToMesh();
					pbMesh.Refresh();
					pbMesh.Optimize();
				}
			}

			MeshSelection.ClearElementSelection();

			// Rebuild the pb_Editor caches
			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Edges Removed");
		}


	}
}
