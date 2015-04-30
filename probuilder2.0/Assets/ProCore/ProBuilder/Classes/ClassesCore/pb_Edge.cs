using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Common {
[System.Serializable]
public class pb_Edge : System.IEquatable<pb_Edge>
{
	public int x, y;

	public pb_Edge() {}

	public pb_Edge(int _x, int _y)
	{
		x = _x;
		y = _y;
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
		return (this.x == edge.x && this.y == edge.y) || (this.x == edge.y && this.y == edge.x);
	}

	public override bool Equals(System.Object b)
	{
		return b is pb_Edge && (this.x == ((pb_Edge)b).x || this.x == ((pb_Edge)b).y) && (this.y == ((pb_Edge)b).x || this.y == ((pb_Edge)b).y);
	}

	public override int GetHashCode()
	{
		// return base.GetHashCode();
		int hashX;
		int hashY;

		if(x < y)
		{
			hashX = x.GetHashCode();
			hashY = y.GetHashCode();	
		}
		else
		{
			hashX = y.GetHashCode();
			hashY = x.GetHashCode();
		}

		//Calculate the hash code for the product. 
		return hashX ^ hashY;
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
	public bool Equals(pb_Edge b, pb_IntArray[] sharedIndices)
	{
		int index = -1;

		index = sharedIndices.IndexOf(x);
		int[] ax = (index > -1) ? sharedIndices[index].array : new int[1]{x};
		
		index = sharedIndices.IndexOf(y);
		int[] ay = (index > -1) ? sharedIndices[index].array : new int[1]{y};

		index = sharedIndices.IndexOf(b.x);
		int[] bx = (index > -1) ? sharedIndices[index].array : new int[1]{b.x};
		
		index = sharedIndices.IndexOf(b.y);
		int[] by = (index > -1) ? sharedIndices[index].array : new int[1]{b.y};

		if( (ax.ContainsMatch(bx) || ax.ContainsMatch(by)) && (ay.ContainsMatch(bx) || ay.ContainsMatch(by)) ) 
			return true;
		else
			return false;
	}

	public bool Equals(pb_Edge b, Dictionary<int, int> lookup)
	{
		int x0 = lookup[x], y0 = lookup[y], x1 = lookup[b.x], y1 = lookup[b.y];
		return (x0 == x1 && y0 == y1) || (x0 == y1 && y0 == x1);
	}

	public bool Contains(int a)
	{
		return (x == a || y == a);
	}

	public bool Contains(int a, pb_IntArray[] sharedIndices)
	{
		// @todo optimize
		int ind = sharedIndices.IndexOf(a);
		return ( System.Array.IndexOf(sharedIndices[ind], x) > -1 || System.Array.IndexOf(sharedIndices[ind], y) > -1);
	}

#region static methods

	struct pb_Range
	{
		public int min, max;
		public pb_Range (int min, int max)
		{
			this.min = min;
			this.max = max;
		}
		public bool Contains(int x)
		{
			return x >= min && x <= max;
		}

		public override string ToString()
		{
			return "(" + min + ", " + max +")";
		}
	}

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
		int len = edges.Length;
		int slen = sharedIndices.Length;

		pb_Range[] bounds = new pb_Range[sharedIndices.Length];
		for(int i = 0; i < slen; i++)
			bounds[i] = new pb_Range( pb_Math.Min(sharedIndices[i].array), pb_Math.Max(sharedIndices[i].array));

		pb_Edge[] uniEdges = new pb_Edge[len];
		for(int i = 0; i < len; i++)
		{
			int x = -1, y = -1;

			// X
			for(int t = 0; t < slen; t++)
			{
				if(!bounds[t].Contains(edges[i].x)) continue;

				for(int n = 0; n < sharedIndices[t].Length; n++)
				{
					if(sharedIndices[t][n] == edges[i].x)	
					{
						x = t;
						break;
					}
				}
				if(x > -1) break;
			}

			// Y
			for(int t = 0; t < slen; t++)
			{
				if(!bounds[t].Contains(edges[i].y)) continue;

				for(int n = 0; n < sharedIndices[t].Length; n++)
				{
					if(sharedIndices[t][n] == edges[i].y)	
					{
						y = t;
						break;
					}
				}
				if(y > -1) break;
			}

			uniEdges[i] = new pb_Edge(x, y);		
		}
		return uniEdges;
	}

	/**
	 * Returns a new pb_Edge containing the index of each element in the sharedIndices array.
	 */
	public static pb_Edge GetUniversalEdge(pb_Edge edge, pb_IntArray[] sharedIndices)
	{
		return new pb_Edge(sharedIndices.IndexOf(edge.x), sharedIndices.IndexOf(edge.y));
	}

	/**
	 * Converts a universal edge to a local edge, guaranteeing that the local edge is
	 * valid (indices point to the same face).
	 */
	public static pb_Edge GetLocalEdge(pb_Object pb, pb_Edge edge)
	{
		pb_Face[] faces = pb.faces;
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		int dist_x = -1, dist_y = -1, shared_x = -1, shared_y = -1;
		for(int i = 0; i < faces.Length; i++)
		{
			if( faces[i].distinctIndices.ContainsMatch(sharedIndices[edge.x].array, out dist_x, out shared_x) &&
				faces[i].distinctIndices.ContainsMatch(sharedIndices[edge.y].array, out dist_y, out shared_y) )
			{
				int x = faces[i].distinctIndices[dist_x];
				int y = faces[i].distinctIndices[dist_y];

				return new pb_Edge(x, y);
			}
		}

		return null;
	}

	/**
	 * Given a local edge, this guarantees that both indices belong to the same face.
	 * Note that this will only return the first valid edge found - there will usually
	 * be multiple matches (well, 2 if your geometry is sane).
	 */
	public static bool ValidateEdge(pb_Object pb, pb_Edge edge, out pb_Edge validEdge)
	{
		pb_Face[] faces = pb.faces;
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		pb_Edge universal = GetUniversalEdge(edge, sharedIndices);

		int dist_x = -1, dist_y = -1, shared_x = -1, shared_y = -1;
		for(int i = 0; i < faces.Length; i++)
		{
			if( faces[i].distinctIndices.ContainsMatch(sharedIndices[universal.x].array, out dist_x, out shared_x) &&
				faces[i].distinctIndices.ContainsMatch(sharedIndices[universal.y].array, out dist_y, out shared_y) )
			{
				int x = faces[i].distinctIndices[dist_x];
				int y = faces[i].distinctIndices[dist_y];

				validEdge = new pb_Edge(x, y);
				return true;
			}
		}

		validEdge = edge;
		return false;
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
			for(int n = i+1; n < edges.Length; n++)
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
	public static bool ContainsDuplicate(this List<pb_Edge> edges, pb_Edge edge, pb_IntArray[] sharedIndices)
	{
		int c = 0;

		for(int i = 0; i < edges.Count; i++)
		{
			if(edges[i].Equals(edge, sharedIndices))
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
	 * Slow IndexOf - takes sharedIndices into account when searching the List.
	 */
	public static int IndexOf(this IList<pb_Edge> edges, pb_Edge edge, pb_IntArray[] sharedIndices)
	{
		for(int i = 0; i < edges.Count; i++)
		{
			if(edges[i].Equals(edge, sharedIndices))
				return i;
		}

		return -1;	
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