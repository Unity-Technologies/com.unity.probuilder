using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_ConnectEdges
	{
		/**
		 *	Inserts new edges connecting edges that share a face.
		 */
		public static bool ConnectEdges(this pb_Object pb, IList<pb_Edge> edges, int divisions = 1)
		{
			// Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			// IEnumerable<pb_EdgeLookup> edge_lookups = edges.Select(x => new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x));
			// List<pb_WingedEdge> wings = pb_WingedEdge.GenerateWingedEdges(pb);
					

			return true;
		}
	}
}
