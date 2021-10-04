using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Represents an edge connecting two vertices.
    ///
    /// This might point to an index in the <see cref="ProBuilderMesh.GetVertices">vertices</see> (local) or the <see cref="ProBuilderMesh.sharedVertices" /> (common) array. The ProBuilder terminology "local" and "common" refers to whether this is an index from the list of all vertices in the ProBuilderMesh or an index from the list of only shared vertices.
    /// </summary>
    /// <seealso cref="ProBuilderMesh.sharedVertices">UnityEngine.ProBuilder.ProBuilderMesh.sharedVertices</seealso>
    /// <seealso cref="EdgeLookup">UnityEngine.ProBuilder.EdgeLookup</seealso>
    [System.Serializable]
    public struct Edge : System.IEquatable<Edge>
    {
        /// <summary>
        /// Stores an index that corresponds to a mesh vertex array.
        /// </summary>
        public int a;

        /// <summary>
        /// Stores an index that corresponds to a mesh vertex array.
        /// </summary>
        public int b;

        /// <summary>
        /// Creates an empty edge defined as `(-1, -1)`.
        /// </summary>
        public static readonly Edge Empty = new Edge(-1, -1);

        /// <summary>
        /// Creates a new edge from two vertex indexes.
        /// </summary>
        /// <param name="a">An index corresponding to a mesh vertex array.</param>
        /// <param name="b">An index corresponding to a mesh vertex array.</param>
        public Edge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        /// <summary>
        /// Tests whether this edge points to valid vertex indexes.
        /// </summary>
        /// <returns>True if x and y are both greater than -1.</returns>
        public bool IsValid()
        {
            return a > -1 && b > -1 && a != b;
        }

        /// <summary>
        /// Returns a string representation of the edge.
        /// </summary>
        /// <returns>String formatted as `[a, b]`.</returns>
        public override string ToString()
        {
            return "[" + a + ", " + b + "]";
        }

        /// <summary>
        /// Tests whether this Edge is equal to another Edge object.
        /// </summary>
        /// <param name="other">The Edge to compare against.</param>
        /// <returns>True if the edges are equal, false if not.</returns>
        public bool Equals(Edge other)
        {
            return (a == other.a && b == other.b) || (a == other.b && b == other.a);
        }

        /// <summary>
        /// Tests whether this object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True if the edges are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            return obj is Edge && Equals((Edge)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
            int hash = 27;

            unchecked
            {
                hash = hash * 29 + (a < b ? a : b);
                hash = hash * 29 + (a < b ? b : a);
            }

            return hash;
        }

        /// <summary>
        /// Creates a new Edge by adding the two left indices together and the two right indices together from both Edge objects.
        /// </summary>
        /// <param name="a">Left edge.</param>
        /// <param name="b">Right edge.</param>
        /// <returns>A new edge where `{x, y} = {(a.a + b.a), (a.b + b.b)}`.</returns>
        public static Edge operator+(Edge a, Edge b)
        {
            return new Edge(a.a + b.a, a.b + b.b);
        }

        /// <summary>
        /// Creates a new Edge by subtracting the two left indices together and the two right indices together from both Edge objects.
        /// </summary>
        /// <param name="a">Left edge.</param>
        /// <param name="b">Right edge.</param>
        /// <returns>A new edge where `{x, y} = {(a.a - b.a), (a.b - b.b)}`.</returns>
        public static Edge operator-(Edge a, Edge b)
        {
            return new Edge(a.a - b.a, a.b - b.b);
        }

        /// <summary>
        /// Creates a new Edge by adding an integer to both indices on an Edge object.
        /// </summary>
        /// <param name="a">The Edge to add to.</param>
        /// <param name="b">The value to add.</param>
        /// <returns>A new edge where `{x, y} = {(a.a + b), (a.b + b)}`.</returns>
        public static Edge operator+(Edge a, int b)
        {
            return new Edge(a.a + b, a.b + b);
        }

        /// <summary>
        /// Creates a new Edge by subtracting an integer from both indices on an Edge object.
        /// </summary>
        /// <param name="a">The Edge to subtract from.</param>
        /// <param name="b">The value to subtract.</param>
        /// <returns>A new edge where `{x, y} = {(a.a - b), (a.b - b)}`.</returns>
        public static Edge operator-(Edge a, int b)
        {
            return new Edge(a.a - b, a.b - b);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="a">The first Edge instance.</param>
        /// <param name="b">The second Edge instance.</param>
        /// <returns>True if the objects are equal; false if not.</returns>
        public static bool operator==(Edge a, Edge b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns true if the two objects are not equal.
        /// </summary>
        /// <param name="a">The first Edge instance.</param>
        /// <param name="b">The second Edge instance.</param>
        /// <returns>True if the objects are not equal; false if they are equal.</returns>
        public static bool operator!=(Edge a, Edge b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Adds two edges index values.
        /// </summary>
        /// <example>
        /// {0, 1} + {4, 5} = {5, 6}
        /// </example>
        /// <param name="a">Left edge parameter.</param>
        /// <param name="b">Right edge parameter.</param>
        /// <returns>The sum of `a + b`.</returns>
        public static Edge Add(Edge a, Edge b)
        {
            return a + b;
        }

        /// <summary>
        /// Subtracts edge b from a.
        /// </summary>
        /// <example>
        /// Subtract( {7, 10}, {4, 5} ) = {3, 5}
        /// </example>
        /// <param name="a">The edge to subtract from.</param>
        /// <param name="b">The value to subtract.</param>
        /// <returns>The difference of `a - b`.</returns>
        public static Edge Subtract(Edge a, Edge b)
        {
            return a - b;
        }

        /// <summary>
        /// Compares edges and takes shared triangles into account.
        /// </summary>
        /// <param name="other">The edge to compare against.</param>
        /// <param name="lookup">A common vertex indexes lookup dictionary. See pb_IntArray for more information.</param>
        /// <remarks>Generally you just pass ProBuilderMesh.sharedIndexes.ToDictionary() to lookup, but it's more efficient to do it once and reuse that dictionary if possible.</remarks>
        /// <returns>True if edges are perceptually equal (that is, they point to the same common indexes).</returns>
        public bool Equals(Edge other, Dictionary<int, int> lookup)
        {
            if (lookup == null)
                return Equals(other);
            int x0 = lookup[a], y0 = lookup[b], x1 = lookup[other.a], y1 = lookup[other.b];
            return (x0 == x1 && y0 == y1) || (x0 == y1 && y0 == x1);
        }

        /// <summary>
        /// Tests whether this edge contains an index.
        /// </summary>
        /// <param name="index">The index to compare against x and y.</param>
        /// <returns>True if x or y is equal to a. False if not.</returns>
        public bool Contains(int index)
        {
            return (a == index || b == index);
        }

        /// <summary>
        /// Tests whether this edge has any matching index to the other edge `b`.
        /// </summary>
        /// <param name="other">The edge to compare against.</param>
        /// <returns>True if x or y matches either b.x or b.y.</returns>
        public bool Contains(Edge other)
        {
            return (a == other.a || b == other.a || a == other.b || b == other.a);
        }

        internal bool Contains(int index, Dictionary<int, int> lookup)
        {
            var common = lookup[index];
            return lookup[a] == common || lookup[b] == common;
        }

        internal static void GetIndices(IEnumerable<Edge> edges, List<int> indices)
        {
            indices.Clear();

            foreach (var edge in edges)
            {
                indices.Add(edge.a);
                indices.Add(edge.b);
            }
        }
    }
}
