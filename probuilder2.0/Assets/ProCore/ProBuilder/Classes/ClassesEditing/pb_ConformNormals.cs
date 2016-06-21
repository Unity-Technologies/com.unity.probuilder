using UnityEngine;
using ProBuilder2.Common;
using System.Collections.Generic;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Methods for making sure adjacent face normals are consistent.
	 */
	public static class pb_ConformNormals
	{

		public static pb_ActionResult ConformNormals(this pb_Object pb, IList<pb_Face> faces)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			return pb_ActionResult.NoSelection;
		}

		public static pb_ActionResult ConformOppositeNormal(pb_WingedEdge source)
		{			
			if(source == null || source.opposite == null)
				return new pb_ActionResult(Status.Failure, "Source edge does not share an edge with another face.");

			pb_Edge cea = GetCommonEdgeInWindingOrder(source);
			pb_Edge ceb = GetCommonEdgeInWindingOrder(source.opposite);

			if( cea.x == ceb.x )
			{
				System.Array.Reverse(source.opposite.face.indices);

				return new pb_ActionResult(Status.Success, "Reversed target face winding order.");
			}

			return new pb_ActionResult(Status.NoChange, "Faces already unified.");
		}

		/**
		 *	Iterate a face and return a new common edge where the edge indices are true to the triangle winding order.
		 */
		private static pb_Edge GetCommonEdgeInWindingOrder(pb_WingedEdge wing)
		{
			int[] indices = wing.face.indices;
			int len = indices.Length;

			for(int i = 0; i < len; i += 3)
			{
				pb_Edge e = wing.edge.local;
				int a = indices[i], b = indices[i+1], c = indices[i+2];

				if(e.x == a && e.y == b)
					return new pb_Edge(wing.edge.common);
				else if(e.x == b && e.y == a)
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == b && e.y == c)
					return new pb_Edge(wing.edge.common);
				else if(e.x == c && e.y == b)
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == c && e.y == a)
					return new pb_Edge(wing.edge.common);
				else if(e.x == a && e.y == c)
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
			}
			return null;
		}
	}
}
