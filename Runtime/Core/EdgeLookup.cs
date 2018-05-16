using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// An edge composed of both the local index and common index.
	/// <br />
	/// <br />
	/// This is useful when comparing vertex indices that are coincident. Coincident vertices are defined as vertices that are share the same coordinate space, but are separate values in the vertex array. ProBuilder tracks these coincident values in the @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" array. A "common" (also called "shared") index is the index of a vertex in the sharedIndexes array.
	/// </summary>
	/// <seealso cref="UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes"/>
	/// <seealso cref="UnityEngine.ProBuilder.Edge"/>
	public struct EdgeLookup : IEquatable<EdgeLookup>
	{
		/// <value>
		/// Local edges point to an index in the vertices array.
		/// </value>
		public Edge local { get; set; }

		/// <value>
		/// Commmon edges point to the vertex index in the sharedIndices array.
		/// </value>
		public Edge common { get; set; }

		/// <summary>
		/// Create an edge lookup from a common and local edge.
		/// </summary>
		/// <param name="common">An edge composed of common indexes (corresponds to @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes").</param>
		/// <param name="local">An edge composed of vertex indexes (corresponds to mesh vertex arrays).</param>
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
			common = new Edge(cx, cy);
			local = new Edge(x, y);
		}

		/// <summary>
		/// Compares each EdgeLookup common edge (does not take into account local edge differences).
		/// </summary>
		/// <param name="other">The EdgeLookup to compare against.</param>
		/// <returns>True if the common edges are equal, false if not.</returns>
		public bool Equals(EdgeLookup other)
		{
			return other.common.Equals(common);
		}

		/// <summary>
		/// Compares each EdgeLookup common edge (does not take into account local edge differences).
		/// </summary>
		/// <param name="obj">The EdgeLookup to compare against. False if obj is not an EdgeLookup type.</param>
		/// <returns>True if the common edges are equal, false if not.</returns>
		public override bool Equals(object obj)
		{
			return !ReferenceEquals(obj, null) && Equals((EdgeLookup)obj);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}

		/// <summary>
		/// Returns a string representation of the common edge property.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("Common: ({0}, {1}), local: ({2}, {3})", common.x, common.y, local.x, local.y);
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
		/// <param name="edges">A collection of local edges.</param>
		/// <param name="lookup">A shared index lookup dictionary (see pb_SharedIndices).</param>
		/// <returns>A HashSet of EdgeLookup edges. EdgeLookup values are compared by their common property only - local edges are not compared.</returns>
		public static HashSet<EdgeLookup> GetEdgeLookupHashSet(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
		{
            if (lookup == null || edges == null)
                return null;
			var hash = new HashSet<EdgeLookup>();
			foreach (var local in edges)
				hash.Add(new EdgeLookup(new Edge(lookup[local.x], lookup[local.y]), local));
			return hash;
		}
	}
}
