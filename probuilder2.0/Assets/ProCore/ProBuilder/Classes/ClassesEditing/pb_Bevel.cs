using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Functions for beveling edges.
	 */
	public static class pb_Bevel
	{

		public static List<pb_Edge> BevelEdge(pb_Object pb, pb_Edge edge, float amount)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			pb_Edge common = new pb_Edge(lookup[edge.x], lookup[edge.y]);

			List<pb_Edge> edges = pb.faces.SelectMany(x => x.edges.Where( y => 
				((lookup[y.x] == common.x && lookup[y.y] == common.y) ||
				(lookup[y.x] == common.y && lookup[y.y] == common.x)))).ToList();

			return edges;
 		}
	}
}
