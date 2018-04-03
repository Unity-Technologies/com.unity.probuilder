using System;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	/// <summary>
	/// An edge composed of both the vertex index and common index. Comparisons between pb_EdgeLookup objects are passed to the common edge.
	/// </summary>
	public class pb_EdgeLookup : IEquatable<pb_EdgeLookup>
	{
		/// <summary>
		/// Local edges point to an index in the vertices array.
		/// </summary>
		public pb_Edge local;

		/// <summary>
		/// Commmon edges point to the vertex index in the sharedIndices array.
		/// </summary>
		public pb_Edge common;

		/// <summary>
		/// Create an edge lookup from a common and local edge.
		/// </summary>
		/// <param name="common"></param>
		/// <param name="local"></param>
		public pb_EdgeLookup(pb_Edge common, pb_Edge local)
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
		public pb_EdgeLookup(int cx, int cy, int x, int y)
		{
			this.common = new pb_Edge(cx, cy);
			this.local = new pb_Edge(x, y);
		}

		/// <summary>
		/// Compares each EdgeLookup common edge (does not take into account local edge differences).
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public bool Equals(pb_EdgeLookup b)
		{
			return common.Equals(ReferenceEquals(b, null) ? pb_Edge.Empty : b.common);
		}

		public override bool Equals(object b)
		{
			pb_EdgeLookup be = b as pb_EdgeLookup;
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
		public static IEnumerable<pb_EdgeLookup> GetEdgeLookup(IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			return edges.Select(x => new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x));
		}

		/// <summary>
		/// Create a hashset of edge lookup values from a collection of local edges and a shared indices lookup.
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="lookup"></param>
		/// <returns></returns>
		public static HashSet<pb_EdgeLookup> GetEdgeLookupHashSet(IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			var hash = new HashSet<pb_EdgeLookup>();
			foreach (var local in edges)
				hash.Add(new pb_EdgeLookup(new pb_Edge(lookup[local.x], lookup[local.y]), local));
			return hash;
		}
	}
}
