using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// A Winged-Edge data structure holds references to an edge, the previous and next edge in it's triangle, it's connected face, and the opposite edge (common).
	///
	///        /   (face)    /
	///  prev /             / next
	///      /    edge     /
	///     /_ _ _ _ _ _ _/
	///     |- - - - - - -|
	///     |  opposite   |
	///     |             |
	///     |             |
	///     |             |
	/// </summary>
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

			if(b is pb_Edge && this.edge.local.Equals((pb_Edge) b))
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

		/**
		 * How many edges are in this sequence.
		 */
		public int Count()
		{
			pb_WingedEdge current = this;
			int count = 0;

			do
			{
				count++;
				current = current.next;
			} while(current != null && current != this);

			return count;
		}

		public override string ToString()
		{
			return string.Format("Common: {0}\nLocal: {1}\nOpposite: {2}\nFace: {3}",
				edge.common.ToString(),
				edge.local.ToString(),
				opposite == null ? "null" : opposite.edge.ToString(),
				face.ToString());
		}

		/**
		 * Given two adjacent triangle wings create a single quad (int[4]).
		 */
		public static int[] MakeQuad(pb_WingedEdge left, pb_WingedEdge right)
		{
			// Both faces must be triangles in order to be considered a quad when combined
			if(left.Count() != 3 || right.Count() != 3)
				return null;

			pb_EdgeLookup[] all = new pb_EdgeLookup[6]
			{
				left.edge,
				left.next.edge,
				left.next.next.edge,
				right.edge,
				right.next.edge,
				right.next.next.edge
			};

			int[] dup = new int[6];
			int matches = 0;

			for(int i = 0; i < 3; i++)
			{
				for(int n = 3; n < 6; n++)
				{
					if(all[i].Equals(all[n]))
					{
						matches++;
						dup[i] = 1;
						dup[n] = 1;
						break;
					}
				}
			}

			// Edges are either not adjacent, or share more than one edge
			if(matches != 1)
				return null;

			int qi = 0;

			pb_EdgeLookup[] edges = new pb_EdgeLookup[4];

			for(int i = 0; i < 6; i++)
				if(dup[i] < 1)
					edges[qi++] = all[i];

			int[] quad = new int[4] { edges[0].local.x, edges[0].local.y, -1, -1 };

			int c1 = edges[0].common.y, c2 = -1;

			if(edges[1].common.x == c1)
			{
				quad[2] = edges[1].local.y;
				c2 = edges[1].common.y;
			}
			else if(edges[2].common.x == c1)
			{
				quad[2] = edges[2].local.y;
				c2 = edges[2].common.y;
			}
			else if(edges[3].common.x == c1)
			{
				quad[2] = edges[3].local.y;
				c2 = edges[3].common.y;
			}

			if(edges[1].common.x == c2)
				quad[3] = edges[1].local.y;
			else if(edges[2].common.x == c2)
				quad[3] = edges[2].local.y;
			else if(edges[3].common.x == c2)
				quad[3] = edges[3].local.y;

			if (quad[2] == -1 || quad[3] == -1)
				return null;

			return quad;
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

			return SortEdgesByAdjacency(edges);
		}

		/**
		 * Sort edges list by adjacency.
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
		 */
		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb, IEnumerable<pb_Face> faces, bool oneWingPerFace = false, Dictionary<int, int> sharedIndexLookup = null)
		{
			Dictionary<int, int> lookup = sharedIndexLookup == null ? pb.sharedIndices.ToDictionary() : sharedIndexLookup;
			IEnumerable<pb_Face> distinct = faces.Distinct();

			List<pb_WingedEdge> winged = new List<pb_WingedEdge>();
			Dictionary<pb_Edge, pb_WingedEdge> opposites = new Dictionary<pb_Edge, pb_WingedEdge>();
			int index = 0;

			foreach(pb_Face f in distinct)
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
