using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Represents an edge composed of both the local index and the common index.
    ///
    /// Use this to compare vertex indices that are coincident.
    /// [Coincident](../manual/gloss.html#coincident) vertices share the same coordinate space, but are separate values
    /// in the vertex array. ProBuilder tracks these coincident values in the <see cref="ProBuilderMesh.sharedVertices" />
    /// array. A "common" (also called "shared") index is the index of a vertex in the sharedVertices array.
    /// </summary>
    /// <seealso cref="ProBuilderMesh.sharedVertices">UnityEngine.ProBuilder.ProBuilderMesh.sharedVertices</seealso>
    /// <seealso cref="Edge">UnityEngine.ProBuilder.Edge</seealso>
    public struct EdgeLookup : IEquatable<EdgeLookup>
    {
        Edge m_Local;
        Edge m_Common;

        /// <summary>
        /// Gets or sets the local edges.
        ///
        /// Local edges point to an index in the <see cref="ProBuilderMesh.GetVertices">vertices</see> array.
        /// </summary>
        public Edge local
        {
            get { return m_Local; }
            set { m_Local = value; }
        }

        /// <summary>
        /// Gets or sets the common edges.
        ///
        /// Commmon edges point to the vertex index in the sharedVertices array.
        /// </summary>
        public Edge common
        {
            get { return m_Common; }
            set { m_Common = value; }
        }

        /// <summary>
        /// Creates an edge lookup from common and local Edge instances.
        /// </summary>
        /// <param name="common">An edge composed of common indexes (corresponds to <see cref="ProBuilderMesh.sharedVertices" />).</param>
        /// <param name="local">An edge composed of vertex indexes (corresponds to <see cref="ProBuilderMesh.GetVertices">mesh vertex arrays</see>).</param>
        public EdgeLookup(Edge common, Edge local)
        {
            m_Common = common;
            m_Local = local;
        }

        /// <summary>
        /// Creates an edge lookup from two set of vertices that represent the common and local edges.
        /// </summary>
        /// <param name="cx">Common edge x.</param>
        /// <param name="cy">Common edge y.</param>
        /// <param name="x">Local edge x.</param>
        /// <param name="y">Local edge y.</param>
        public EdgeLookup(int cx, int cy, int x, int y)
        {
            m_Common = new Edge(cx, cy);
            m_Local = new Edge(x, y);
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
        /// <returns>True if the common edges are equal; false if not.</returns>
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(obj, null) && Equals((EdgeLookup)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return common.GetHashCode();
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="a">The first EdgeLookup instance.</param>
        /// <param name="b">The second EdgeLookup instance.</param>
        /// <returns>True if the objects are equal; false if not.</returns>
        public static bool operator==(EdgeLookup a, EdgeLookup b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Returns true if the two objects are not equal.
        /// </summary>
        /// <param name="a">The first EdgeLookup instance.</param>
        /// <param name="b">The second EdgeLookup instance.</param>
        /// <returns>True if the objects are not equal; false if they are equal.</returns>
        public static bool operator!=(EdgeLookup a, EdgeLookup b)
        {
            return !Equals(a, b);
        }

        /// <summary>
        /// Returns a string representation of the common edge property.
        /// </summary>
        /// <returns>String formatted as: "Common: (`common.a`, `common.b`), local: (`local.a`, `local.b`)"</returns>
        public override string ToString()
        {
            return string.Format("Common: ({0}, {1}), local: ({2}, {3})", common.a, common.b, local.a, local.b);
        }

        /// <summary>
        /// Creates a list of EdgeLookup edges from a set of local edges and a sharedVertices dictionary.
        /// </summary>
        /// <param name="edges">A collection of local edges.</param>
        /// <param name="lookup">A shared index lookup dictionary (see <see cref="ProBuilderMesh.sharedVertices" />).</param>
        /// <returns>A set of EdgeLookup edges that you can iterate over.</returns>
        public static IEnumerable<EdgeLookup> GetEdgeLookup(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
        {
            return edges.Select(x => new EdgeLookup(new Edge(lookup[x.a], lookup[x.b]), x));
        }

        /// <summary>
        /// Creates a hashset of edge lookup values from a collection of local edges and a shared indexes lookup.
        /// </summary>
        /// <param name="edges">A collection of local edges.</param>
        /// <param name="lookup">A shared index lookup dictionary (see <see cref="ProBuilderMesh.sharedVertices" />).</param>
        /// <returns>A HashSet of EdgeLookup edges. EdgeLookup values are compared by their common property only: local edges are not compared.</returns>
        public static HashSet<EdgeLookup> GetEdgeLookupHashSet(IEnumerable<Edge> edges, Dictionary<int, int> lookup)
        {
            if (lookup == null || edges == null)
                return null;
            var hash = new HashSet<EdgeLookup>();
            foreach (var local in edges)
                hash.Add(new EdgeLookup(new Edge(lookup[local.a], lookup[local.b]), local));
            return hash;
        }
    }
}
