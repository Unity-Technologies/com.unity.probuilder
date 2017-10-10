using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	[System.Serializable]
	public struct pb_Edge : System.IEquatable<pb_Edge>
	{
		public int x, y;

		public static readonly pb_Edge Empty = new pb_Edge(-1, -1);

		public pb_Edge(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public pb_Edge(pb_Edge edge)
		{
			x = edge.x;
			y = edge.y;
		}

		public bool IsValid()
		{
			return x > -1 && y > -1 && x != y;
		}

		public override string ToString()
		{
			return "[" + x + ", " + y + "]";
		}

		public bool Equals(pb_Edge edge)
		{
			return (x == edge.x && y == edge.y) || (x == edge.y && y == edge.x);
		}

		public override bool Equals(System.Object b)
		{
			return b is pb_Edge && this.Equals((pb_Edge) b);
		}

		public override int GetHashCode()
		{
			// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
			int hash = 27;

			unchecked
			{
				hash = hash * 29 + (x < y ? x : y);
				hash = hash * 29 + (x < y ? y : x);
			}

			return hash;
		}

		public static pb_Edge operator +(pb_Edge a, pb_Edge b)
		{
			return new pb_Edge(a.x + b.x, a.y + b.y);
		}

		public static pb_Edge operator -(pb_Edge a, pb_Edge b)
		{
			return new pb_Edge(a.x - b.x, a.y - b.y);
		}

		public static pb_Edge operator +(pb_Edge a, int b)
		{
			return new pb_Edge(a.x + b, a.y + b);
		}

		public static pb_Edge operator -(pb_Edge a, int b)
		{
			return new pb_Edge(a.x - b, a.y - b);
		}

		public static bool operator ==(pb_Edge a, pb_Edge b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(pb_Edge a, pb_Edge b)
		{
			return !(a == b);
		}

		public int[] ToArray()
		{
			return new int[2] {x, y};
		}

		/**
		 * \brief Compares edges and takes shared triangles into account.
		 * @param a First edge to compare.
		 * @param b Second edge to compare against.
		 * @param sharedIndices A pb_IntArray[] containing int[] of triangles that share a vertex.
		 * \returns True or false if edge a is equal to b.
		 */
		public bool Equals(pb_Edge b, Dictionary<int, int> lookup)
		{
			int x0 = lookup[x], y0 = lookup[y], x1 = lookup[b.x], y1 = lookup[b.y];
			return (x0 == x1 && y0 == y1) || (x0 == y1 && y0 == x1);
		}

		public bool Contains(int a)
		{
			return (x == a || y == a);
		}

		public bool Contains(pb_Edge b)
		{
			return (x == b.x || y == b.x || x == b.y || y == b.x);
		}

		public bool Contains(int a, pb_IntArray[] sharedIndices)
		{
			// @todo optimize
			int ind = sharedIndices.IndexOf(a);
			return ( System.Array.IndexOf(sharedIndices[ind], x) > -1 || System.Array.IndexOf(sharedIndices[ind], y) > -1);
		}

	#region static methods

		/**
		 *	Returns new edges where each edge is composed not of vertex indices, but rather the index in pb.sharedIndices of each
		 *	vertex.
		 */
		public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, Dictionary<int, int> sharedIndicesLookup)
		{
			pb_Edge[] uni = new pb_Edge[edges.Length];

			for(int i = 0; i < edges.Length; i++)
				uni[i] = new pb_Edge( sharedIndicesLookup[edges[i].x], sharedIndicesLookup[edges[i].y] );

			return uni;
		}

		public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, pb_IntArray[] sharedIndices)
		{
			return GetUniversalEdges(edges, sharedIndices.ToDictionary());
		}

		/**
		 * Converts a universal edge to local.  Does *not* guarantee that edges will be valid (indices belong to the same face and edge).
		 */
		public static pb_Edge GetLocalEdgeFast(pb_Edge edge, pb_IntArray[] sharedIndices)
		{
			return new pb_Edge(sharedIndices[edge.x][0], sharedIndices[edge.y][0]);
		}

		/**
		 * Given a local edge, this guarantees that both indices belong to the same face.
		 * Note that this will only return the first valid edge found - there will usually
		 * be multiple matches (well, 2 if your geometry is sane).
		 */
		public static bool ValidateEdge(pb_Object pb, pb_Edge edge, out pb_Tuple<pb_Face, pb_Edge> validEdge)
		{
			pb_Face[] faces = pb.faces;
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			pb_Edge universal = new pb_Edge(sharedIndices.IndexOf(edge.x), sharedIndices.IndexOf(edge.y));

			int dist_x = -1, dist_y = -1, shared_x = -1, shared_y = -1;
			for(int i = 0; i < faces.Length; i++)
			{
				if( faces[i].distinctIndices.ContainsMatch(sharedIndices[universal.x].array, out dist_x, out shared_x) &&
					faces[i].distinctIndices.ContainsMatch(sharedIndices[universal.y].array, out dist_y, out shared_y) )
				{
					int x = faces[i].distinctIndices[dist_x];
					int y = faces[i].distinctIndices[dist_y];

					validEdge = new pb_Tuple<pb_Face, pb_Edge>(faces[i], new pb_Edge(x, y));
					return true;
				}
			}

			validEdge = null;
			return false;
		}

		/**
		 *	Returns a new array of edges guaranteed to be distinct and valid to face.
		 */
		public static List<pb_Edge> ValidateEdges(pb_Object pb, pb_Edge[] edges)
		{
			pb_Face[] faces = pb.faces;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			HashSet<pb_EdgeLookup> edge_lookup = new HashSet<pb_EdgeLookup>(pb_EdgeLookup.GetEdgeLookup(edges, lookup));
			List<pb_Edge> valid = new List<pb_Edge>();
			bool superBreak = false;

			for(int i = 0; i < faces.Length && !superBreak; i++)
			{
				pb_Edge[] ea = faces[i].edges;

				for(int n = 0; n < ea.Length && !superBreak; n++)
				{
					pb_EdgeLookup le = new pb_EdgeLookup(lookup[ea[n].x], lookup[ea[n].y], ea[n].x, ea[n].y);

					if( edge_lookup.Contains(le) )
					{
						edge_lookup.Remove(le);
						superBreak = edge_lookup.Count < 1;
						valid.Add(le.local);
					}
				}
			}

			return valid;
		}

		/**
		 * Converts an array of universal edges to local.  Does *not* guarantee that edges will be valid.
		 */
		public static pb_Edge[] GetLocalEdges_Fast(pb_Edge[] edges, pb_IntArray[] sharedIndices)
		{
			pb_Edge[] local = new pb_Edge[edges.Length];
			for(int i = 0; i < local.Length; i++)
				local[i] = new pb_Edge(sharedIndices[edges[i].x][0], sharedIndices[edges[i].y][0]);
			return local;
		}

		/**
		 * Returns all Edges contained in these faces.
		 */
		public static pb_Edge[] AllEdges(pb_Face[] faces)
		{
			List<pb_Edge> edges = new List<pb_Edge>();
			foreach(pb_Face f in faces)
				edges.AddRange(f.edges);
			return edges.ToArray();
		}

		/**
		 *	Simple contains duplicate.  Does NOT account for shared indices
		 */
		public static bool ContainsDuplicateFast(pb_Edge[] edges, pb_Edge edge)
		{
			int c = 0;
			for(int i = 0; i < edges.Length; i++)
			{
				if(edges[i].Equals(edge))
					c++;
			}
			return c > 1;
		}

		public static Vector3[] VerticesWithEdges(pb_Edge[] edges, Vector3[] vertices)
		{
			Vector3[] v = new Vector3[edges.Length * 2];
			int n = 0;
			for(int i = 0; i < edges.Length; i++)
			{
				v[n++] = vertices[edges[i].x];
				v[n++] = vertices[edges[i].y];
			}
			return v;
		}

		/**
		 * Returns all edges in this array that are only referenced once.
		 */
		public static pb_Edge[] GetPerimeterEdges(pb_Edge[] edges)
		{
			int[] count = pbUtil.FilledArray<int>(0, edges.Length);

			for(int i = 0; i < edges.Length-1; i++)
			{
				for(int n = i + 1; n < edges.Length; n++)
				{
					if(edges[i].Equals(edges[n]))
					{
						count[i]++;
						count[n]++;
					}
				}
			}

			return edges.Where((val, index) => count[index] < 1).ToArray();
		}
	#endregion
	}

	public static class EdgeExtensions
	{
		/**
		 *	Checks for duplicates taking sharedIndices into account
		 */
		public static bool ContainsDuplicate(this List<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)// pb_IntArray[] sharedIndices)
		{
			int c = 0;

			for(int i = 0; i < edges.Count; i++)
			{
				if(edges[i].Equals(edge, lookup))
					if(++c > 1) return true;
			}

			return false;
		}

		/**
		 *	Fast contains - doens't account for shared indices
		 */
		public static bool Contains(this pb_Edge[] edges, pb_Edge edge)
		{
			for(int i = 0; i < edges.Length; i++)
			{
				if(edges[i].Equals(edge))
					return true;
			}

			return false;
		}

		/**
		 *	Fast contains - doens't account for shared indices
		 */
		public static bool Contains(this pb_Edge[] edges, int x, int y)
		{
			for(int i = 0; i < edges.Length; i++)
			{
				if( (x == edges[i].x && y == edges[i].y) || (x == edges[i].y && y == edges[i].x) )
					return true;
			}

			return false;
		}

		/**
		 *	Returns a set of edges where no edge overlaps another.
		 */
		public static IEnumerable<pb_Edge> DistinctCommon(this IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			IEnumerable<pb_EdgeLookup> lup = edges.Select(x => new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x));
			lup = lup.Distinct();
			return lup.Select(x => x.local);
		}

		public static int IndexOf(this IList<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)
		{
			for(int i = 0; i < edges.Count; i++)
			{
				if(edges[i].Equals(edge, lookup))
					return i;
			}

			return -1;
		}

		public static List<int> ToIntList(this List<pb_Edge> edges)
		{
			List<int> arr = new List<int>();
			foreach(pb_Edge edge in edges)
			{
				arr.Add( edge.x );
				arr.Add( edge.y );
			}
			return arr;
		}

		public static int[] AllTriangles(this pb_Edge[] edges)
		{
			int[] arr = new int[edges.Length*2];
			int n = 0;

			for(int i = 0; i < edges.Length; i++)
			{
				arr[n++] = edges[i].x;
				arr[n++] = edges[i].y;
			}
			return arr;
		}

		public static List<int> AllTriangles(this List<pb_Edge> edges)
		{
			List<int> arr = new List<int>();

			for(int i = 0; i < edges.Count; i++)
			{
				arr.Add(edges[i].x);
				arr.Add(edges[i].y);
			}
			return arr;
		}
	}
}
