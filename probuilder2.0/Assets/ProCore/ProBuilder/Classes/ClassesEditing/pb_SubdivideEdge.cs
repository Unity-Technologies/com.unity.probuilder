using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Add additional vertices to an edge.
	 */
	public static class pb_SubdivideEdge
	{
		public static bool SubdivideEdge(this pb_Object pb, pb_Edge edge)
		{
			return SubdivideEdges(pb, new pb_Edge[] { edge });
		}

		public static bool SubdivideEdges(this pb_Object pb, IEnumerable<pb_Edge> edges)
		{


			return true;
		}
	}
}
