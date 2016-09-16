using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	public class pb_WingedEdge : IEquatable<pb_WingedEdge>, IEnumerable
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

		IEnumerator IEnumerable.GetEnumerator()
		{
		   return (IEnumerator) GetEnumerator();
		}

		public pb_WingedEdgeEnumerator GetEnumerator()
		{
		    return new pb_WingedEdgeEnumerator(this);
		}

		public override string ToString()
		{
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

		public static List<pb_Edge> SortEdgesByAdjacency(pb_Face face)
		{
			// grab perimeter edges
			List<pb_Edge> edges = new List<pb_Edge>(face.edges);

			return SortEdgesByAdjacency(edges);
		}

		/**
		 *	Returns a new set of edges where each edge's y matches the next edge x.
		 *	The first edge is used as a starting point.
		 */
		public static List<pb_Edge> SortEdgesByAdjacency(List<pb_Edge> edges)
		{
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

		/**
		 *	Returns a dictionary where each key is a common index with a list of each winged edge touching it.
		 */
		public static Dictionary<int, List<pb_WingedEdge>> GetSpokes(List<pb_WingedEdge> wings)
		{
			Dictionary<int, List<pb_WingedEdge>> spokes = new Dictionary<int, List<pb_WingedEdge>>();
			List<pb_WingedEdge> l = null;

			for(int i = 0; i < wings.Count; i++)
			{
				if(spokes.TryGetValue(wings[i].edge.common.x, out l))
					l.Add(wings[i]);
				else
					spokes.Add(wings[i].edge.common.x, new List<pb_WingedEdge>() { wings[i] });

				if(spokes.TryGetValue(wings[i].edge.common.y, out l))
					l.Add(wings[i]);
				else
					spokes.Add(wings[i].edge.common.y, new List<pb_WingedEdge>() { wings[i] });
			}

			return spokes;
		}

		/**
		 *	Given a set of winged edges and list of common indices, attempt to create a complete path of indices where each
		 *	is connected by edge.  May be clockwise or counter-clockwise ordered, or null if no path is found.
		 */
		public static List<int> SortCommonIndicesByAdjacency(List<pb_WingedEdge> wings, HashSet<int> common)
		{
			List<pb_Edge> matches = wings.Where(x => common.Contains(x.edge.common.x) && common.Contains(x.edge.common.y)).Select(y => y.edge.common).ToList();

			// if edge count != index count there isn't a full perimeter
			if(matches.Count != common.Count)
				return null;

			return SortEdgesByAdjacency(matches).Select(x => x.x).ToList();
		}

		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb, bool oneWingPerFace = false)
		{
			return GetWingedEdges(pb, pb.faces, oneWingPerFace);
		}

		/**
		 *	Generate a Winged Edge data structure.
		 * 	If `oneWingPerFace` is true the returned list will contain a single winged edge per-face (but still point to all edges).
		 *	
		 *	Faces must be distinct!  Duplicate faces will result in incorrect wings.
		 */
		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb, IEnumerable<pb_Face> faces, bool oneWingPerFace = false)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			List<pb_WingedEdge> winged = new List<pb_WingedEdge>();
			Dictionary<pb_Edge, pb_WingedEdge> opposites = new Dictionary<pb_Edge, pb_WingedEdge>();
			int index = 0;

			foreach(pb_Face f in faces)
			{
				List<pb_Edge> edges = SortEdgesByAdjacency(f);
				int edgeLength = edges.Count;
				pb_WingedEdge first = null, prev = null;

				for(int n = 0; n < edgeLength; n++)
				{
					pb_Edge e = edges[n];

					pb_WingedEdge w = new pb_WingedEdge();
					w.edge = new pb_EdgeLookup(lookup[e.x], lookup[e.y], e.x, e.y);
					w.face = f;
					if(n < 1) first = w;

					if(n > 0)
					{
						w.previous = prev;
						prev.next = w;
					}

					if(n == edgeLength - 1)
					{
						w.next = first;
						first.previous = w;
					}

					prev = w;

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

					if(!oneWingPerFace || n < 1)
						winged.Add(w);
				}

				index += edgeLength;
			}

			return winged;
		}
	}
}
