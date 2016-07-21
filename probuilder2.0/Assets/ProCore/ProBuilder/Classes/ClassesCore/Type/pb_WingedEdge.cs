using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 *	A Winged-Edge data structure holds references to an edge, the previous and next edge in it's triangle, it's connected face, and the opposite edge (common).
	 *
	 *        /   (face)    /
	 *  prev /             / next
	 *      /    edge     /
	 *     /_ _ _ _ _ _ _/
	 *     |- - - - - - -| 
	 *     |  opposite   |
	 *     |             |
	 *     |             |
	 *     |             |
	 */
	public class pb_WingedEdge : IEquatable<pb_WingedEdge>
	{
		public pb_EdgeLookup edge;
		public pb_Face face;
		public pb_WingedEdge next;
		public pb_WingedEdge previous;
		public pb_WingedEdge opposite;

		public bool Equals(pb_WingedEdge b)
		{
			return b != null && edge.local.Equals(b.edge.local);
		}

		public override bool Equals(System.Object b)
		{
			pb_WingedEdge be = b as pb_WingedEdge;

			if(be != null && this.Equals(be))
				return true;

			pb_Edge e = b as pb_Edge;

			if(e != null && this.Equals(e))
				return true;

			return true;
		}

		public override int GetHashCode()
		{
			return edge.local.GetHashCode();
		}

		public override string ToString()
		{
			// return string.Format("Edge: {0}\nNext: {1}\nPrevious: {2}\nOpposite: {3}", 
			// 	edge.local.ToString(),
			// 	next.edge.local.ToString(),
			// 	previous.edge.local.ToString(),
			// 	opposite.edge.local.ToString());

			return string.Format("Common: {0}\nLocal: {1}\nOpposite: {2}\nFace: {3}",
				edge.common.ToString(),
				edge.local.ToString(),
				opposite == null ? "null" : opposite.edge.ToString(),
				face.ToString());
		}

		public pb_WingedEdge GetAdjacentEdgeWithCommonIndex(int common)
		{
			if(next.edge.common.Contains(common))
				return next;
			else if(previous.edge.common.Contains(common))
				return previous;

			return null;
		}

		/**
		 *	Returns a new set of edges where each edge's y matches the next edge x.
		 *	The first edge is used as a starting point.
		 */
		public static List<pb_Edge> SortEdgesByAdjacency(pb_Face face)
		{
			// grab perimeter edges
			List<pb_Edge> edges = new List<pb_Edge>(face.edges);

			for(int i = 1; i < edges.Count; i++)
			{
				int want = edges[i - 1].y;

				for(int n = i + 1; n < edges.Count; n++)
				{
					if(edges[n].x == want || edges[n].y == want)
					{
						pb_Edge swap = edges[n];
						edges[n] = edges[i];
						edges[i] = swap;
					}
				}
			}

			return edges;
		}

		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb)
		{
			return GetWingedEdges(pb, pb.faces);
		}

		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb, IList<pb_Face> faces)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			List<pb_WingedEdge> winged = new List<pb_WingedEdge>();
			Dictionary<pb_Edge, pb_WingedEdge> opposites = new Dictionary<pb_Edge, pb_WingedEdge>();
			int index = 0;

			for(int i = 0; i < faces.Count; i++)
			{
				pb_Face f = faces[i];
				List<pb_Edge> edges = SortEdgesByAdjacency(f);
				int edgeLength = edges.Count;

				for(int n = 0; n < edgeLength; n++)
				{
					pb_Edge e = edges[n];

					pb_WingedEdge w = new pb_WingedEdge();
					w.edge = new pb_EdgeLookup(lookup[e.x], lookup[e.y], e.x, e.y);
					w.face = f;

					if(n > 0)
					{
						w.previous = winged[index + n - 1];
						winged[index + n - 1].next = w;
					}

					if(n == edgeLength - 1)
					{
						w.next = winged[index];
						winged[index].previous = w;
					}

					pb_WingedEdge opp;

					if( opposites.TryGetValue(w.edge.common, out opp) )
					{
						opp.opposite = w;
						w.opposite = opp;
					}
					else
					{
						w.opposite = null;
						opposites.Add(w.edge.common, w );
					}

					winged.Add(w);
				}

				index += edgeLength;
			}

			return winged;
		}
	}
}
