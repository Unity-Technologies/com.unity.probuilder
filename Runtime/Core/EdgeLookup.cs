using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// An edge composed of both the vertex index and common index. Comparisons between pb_EdgeLookup objects are passed to the common edge.
	/// </summary>
	public class EdgeLookup : IEquatable<EdgeLookup>
	{
		/// <summary>
		/// Local edges point to an index in the vertices array.
		/// </summary>
		public Edge local;

		/// <summary>
		/// Commmon edges point to the vertex index in the sharedIndices array.
		/// </summary>
		public Edge common;

		/// <summary>
		/// Create an edge lookup from a common and local edge.
		/// </summary>
		/// <param name="common"></param>
		/// <param name="local"></param>
		public EdgeLookup(Edge common, Edge local)
		{
			this.common = common;
			this.local = local;
		}

		/// <summary>
		/// Create an edge lookup from common and local edges.
		/// </summary>
		/// <param name="cx">Common edge x.</param>
		/// <param name="cy">Common edge y.</param>
		/// <param name="x">Local edge x.</param>
		/// <param name="y">Local edge y.</param>
		public EdgeLookup(int cx, int cy, int x, int y)
		{
			this.common = new Edge(cx, cy);
			this.local = new Edge(x, y);
		}

		/// <summary>
		/// Compares each EdgeLookup common edge (does not take into account local edge differences).
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public bool Equals(EdgeLookup b)
		{
			return common.Equals(ReferenceEquals(b, null) ? Edge.Empty : b.common);
		}

		public override bool Equals(object b)
		{
			EdgeLookup be = b as EdgeLookup;
			return be != null && common.Equals(be.common);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", common.x, common.y);
			// return string.Format("common ({0}, {1})\nlocal({2}, {3})", common.x, common.y, local.x, local.y);
			// return string.Format("c({0}, {1})  l({2}, {3})  > {4}", common.x, common.y, local.x, local.y, GetHashCode());
		}

		/// <summary>
		/// Create a list of EdgeLookup edges from a set of local edges and a sharedIndices dictionary.
		/// </summary>
		/// <param name="edges">A collection of local edges.</param>
		/// <param name="lookup">A shared index lookup dictionary (see pb_SharedIndices).</param>
		/// <returns>A set of EdgeLookup edges.</returns>
		public static IEnumerable<EdgeLookup> GetEdgeLookup(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
		{
			return edges.Select(x => new EdgeLookup(new Edge(lookup[x.x], lookup[x.y]), x));
		}

		/// <summary>
		/// Create a hashset of edge lookup values from a collection of local edges and a shared indices lookup.
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="lookup"></param>
		/// <returns></returns>
		public static HashSet<EdgeLookup> GetEdgeLookupHashSet(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
		{
			var hash = new HashSet<EdgeLookup>();
			foreach (var local in edges)
				hash.Add(new EdgeLookup(new Edge(lookup[local.x], lookup[local.y]), local));
			return hash;
		}
	}
}
