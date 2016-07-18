using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Utility class for connecting vertices.
	 */
	public static class pb_ConnectVertices
	{
		public static pb_ActionResult Connect(this pb_Object pb, IList<int> indices, out pb_Edge[] connectingEdges)
		{
			connectingEdges = null;
			return pb_ActionResult.NoSelection;
		}
	}
}
