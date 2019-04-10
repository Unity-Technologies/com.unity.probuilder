using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// An edge connecting two vertices. May point to an index in the vertices or the sharedIndexes array (local / common in ProBuilder terminology).
    /// </summary>
    [System.Serializable]
    public struct Edge : System.IEquatable<Edge>
    {
        /// <value>
        /// An index corresponding to a mesh vertex array.
        /// </value>
        public int a;

        /// <value>
        /// An index corresponding to a mesh vertex array.
        /// </value>
        public int b;

        /// <value>
        /// An empty edge is defined as -1, -1.
        /// </value>
        public static readonly Edge Empty = new Edge(-1, -1);

        /// <summary>
        /// Create a new edge from two vertex indexes.
        /// </summary>
        /// <param name="a">An index corresponding to a mesh vertex array.</param>
        /// <param name="b">An index corresponding to a mesh vertex array.</param>
        public Edge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        /// <summary>
        /// Test if this edge points to valid vertex indexes.
        /// </summary>
        /// <returns>True if x and y are both greater than -1.</returns>
        public bool IsValid()
        {
            return a > -1 && b > -1 && a != b;
        }

        public override string ToString()
        {
            return "[" + a + ", " + b + "]";
        }

        public bool Equals(Edge other)
        {
            return (a == other.a && b == other.b) || (a == other.b && b == other.a);
        }

        public override bool Equals(object obj)
        {
            return obj is Edge && Equals((Edge)obj);
        }

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

        public static Edge operator+(Edge a, Edge b)
        {
            return new Edge(a.a + b.a, a.b + b.b);
        }

        public static Edge operator-(Edge a, Edge b)
        {
            return new Edge(a.a - b.a, a.b - b.b);
        }

        public static Edge operator+(Edge a, int b)
        {
            return new Edge(a.a + b, a.b + b);
        }

        public static Edge operator-(Edge a, int b)
        {
            return new Edge(a.a - b, a.b - b);
        }

        public static bool operator==(Edge a, Edge b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(Edge a, Edge b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Add two edges index values.
        /// </summary>
        /// <example>
        /// {0, 1} + {4, 5} = {5, 6}
        /// </example>
        /// <param name="a">Left edge parameter.</param>
        /// <param name="b">Right edge parameter.</param>
        /// <returns>The sum of a + b.</returns>
        public static Edge Add(Edge a, Edge b)
        {
            return a + b;
        }

        /// <summary>
        /// Subtract edge b from a.
        /// </summary>
        /// <example>
        /// Subtract( {7, 10}, {4, 5} ) = {3, 5}
        /// </example>
        /// <param name="a">The edge to subtract from.</param>
        /// <param name="b">The value to subtract.</param>
        /// <returns>The sum of a - b.</returns>
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
        /// Does this edge contain an index?
        /// </summary>
        /// <param name="index">The index to compare against x and y.</param>
        /// <returns>True if x or y is equal to a. False if not.</returns>
        public bool Contains(int index)
        {
            return (a == index || b == index);
        }

        /// <summary>
        /// Does this edge have any matching index to edge b?
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
