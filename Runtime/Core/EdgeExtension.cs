using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Utilities for working with pb_Edge.
	/// </summary>
	static class EdgeExtension
	{
		/// <summary>
		/// Returns new edges where each edge is composed not of vertex indices, but rather the index in pb.sharedIndices of each vertex.
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="sharedIndicesLookup"></param>
		/// <returns></returns>
		public static Edge[] GetUniversalEdges(IList<Edge> edges, Dictionary<int, int> sharedIndicesLookup)
		{
			int ec = edges.Count;
			Edge[] uni = new Edge[ec];
			for(var i = 0; i < ec; i++)
				uni[i] = new Edge( sharedIndicesLookup[edges[i].x], sharedIndicesLookup[edges[i].y] );

			return uni;
		}

		/// <summary>
		/// Returns new edges where each edge is composed not of vertex indices, but rather the index in pb.sharedIndices of each vertex.
		/// </summary>
		/// <remarks>For performance reasons, where possible you should favor using the overload that accepts a shared indices dictionary.</remarks>
		/// <param name="edges"></param>
		/// <param name="sharedIndices"></param>
		/// <returns></returns>
		public static Edge[] GetUniversalEdges(IList<Edge> edges, IList<IntArray> sharedIndices)
		{
			return GetUniversalEdges(edges, sharedIndices.ToDictionary());
		}

		/// <summary>
		/// Converts a universal edge to local.  Does *not* guarantee that edges will be valid (indices belong to the same face and edge).
		/// </summary>
		/// <param name="edge"></param>
		/// <param name="sharedIndices"></param>
		/// <returns></returns>
		internal static Edge GetLocalEdgeFast(Edge edge, IntArray[] sharedIndices)
		{
			return new Edge(sharedIndices[edge.x][0], sharedIndices[edge.y][0]);
		}

		/// <summary>
		/// Given a local edge, this guarantees that both indices belong to the same face.
		/// Note that this will only return the first valid edge found - there will usually
		/// be multiple matches (well, 2 if your geometry is sane).
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edge"></param>
		/// <param name="validEdge"></param>
		/// <returns></returns>
		public static bool ValidateEdge(ProBuilderMesh pb, Edge edge, out SimpleTuple<Face, Edge> validEdge)
		{
			Face[] faces = pb.facesInternal;
			IntArray[] sharedIndices = pb.sharedIndicesInternal;

			Edge universal = new Edge(sharedIndices.IndexOf(edge.x), sharedIndices.IndexOf(edge.y));

			int dist_x = -1, dist_y = -1, shared_x = -1, shared_y = -1;
			for(int i = 0; i < faces.Length; i++)
			{
				if( faces[i].distinctIndices.ContainsMatch(sharedIndices[universal.x].array, out dist_x, out shared_x) &&
					faces[i].distinctIndices.ContainsMatch(sharedIndices[universal.y].array, out dist_y, out shared_y) )
				{
					int x = faces[i].distinctIndices[dist_x];
					int y = faces[i].distinctIndices[dist_y];

					validEdge = new SimpleTuple<Face, Edge>(faces[i], new Edge(x, y));
					return true;
				}
			}

			validEdge = null;
			return false;
		}

		/// <summary>
		/// Returns all Edges contained in these faces.
		/// </summary>
		/// <param name="faces"></param>
		/// <returns></returns>
		internal static Edge[] AllEdges(Face[] faces)
		{
			List<Edge> edges = new List<Edge>();
			foreach(Face f in faces)
				edges.AddRange(f.edgesInternal);
			return edges.ToArray();
		}

		/// <summary>
		/// Fast contains. Doesn't account for shared indices
		/// </summary>
		internal static bool Contains(this Edge[] edges, Edge edge)
		{
			for(int i = 0; i < edges.Length; i++)
			{
				if(edges[i].Equals(edge))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Fast contains. Doesn't account for shared indices
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		internal static bool Contains(this Edge[] edges, int x, int y)
		{
			for(int i = 0; i < edges.Length; i++)
			{
				if( (x == edges[i].x && y == edges[i].y) || (x == edges[i].y && y == edges[i].x) )
					return true;
			}

			return false;
		}

		internal static int IndexOf(this IList<Edge> edges, Edge edge, Dictionary<int, int> lookup)
		{
			for(int i = 0; i < edges.Count; i++)
			{
				if(edges[i].Equals(edge, lookup))
					return i;
			}

			return -1;
		}

		internal static int[] AllTriangles(this Edge[] edges)
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

		internal static List<int> AllTriangles(this List<Edge> edges)
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
