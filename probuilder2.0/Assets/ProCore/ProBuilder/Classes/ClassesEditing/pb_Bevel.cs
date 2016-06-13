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

		public static List<pb_Edge> BevelEdge(pb_Object pb, IList<pb_Edge> edges, float amount)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			List<pb_EdgeLookup> 
			List<pb_WingedEdge> wings = pb_WingedEdge.GenerateWingedEdges(pb);

			return null;
 		}
	}
}
