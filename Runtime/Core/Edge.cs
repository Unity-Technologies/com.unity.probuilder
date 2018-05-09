using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// An edge connecting two vertices. May point to an index in the vertices or the sharedIndices array (local / common in ProBuilder terminology).
	/// </summary>
	[System.Serializable]
	public struct Edge : System.IEquatable<Edge>
	{
		/// <value>
		/// An index corresponding to a mesh vertex array.
		/// </value>
		public int x;

		/// <value>
		/// An index corresponding to a mesh vertex array.
		/// </value>
		public int y;

		/// <value>
		/// An empty edge is defined as -1, -1.
		/// </value>
		public static readonly Edge Empty = new Edge(-1, -1);

		/// <summary>
		/// Create a new edge from two vertex indices.
		/// </summary>
		/// <param name="x">An index corresponding to a mesh vertex array.</param>
		/// <param name="y">An index corresponding to a mesh vertex array.</param>
		public Edge(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		/// <summary>
		/// Test if this edge points to valid vertex indices.
		/// </summary>
		/// <returns>True if x and y are both greater than -1.</returns>
		public bool IsValid()
		{
			return x > -1 && y > -1 && x != y;
		}

		public override string ToString()
		{
			return "[" + x + ", " + y + "]";
		}

		public bool Equals(Edge other)
		{
			return (x == other.x && y == other.y) || (x == other.y && y == other.x);
		}

		public override bool Equals(System.Object obj)
		{
			return obj is Edge && Equals((Edge) obj);
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

		public static Edge operator +(Edge a, Edge b)
		{
			return new Edge(a.x + b.x, a.y + b.y);
		}

		public static Edge operator -(Edge a, Edge b)
		{
			return new Edge(a.x - b.x, a.y - b.y);
		}

		public static Edge operator +(Edge a, int b)
		{
			return new Edge(a.x + b, a.y + b);
		}

		public static Edge operator -(Edge a, int b)
		{
			return new Edge(a.x - b, a.y - b);
		}

		public static bool operator ==(Edge a, Edge b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Edge a, Edge b)
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
		/// Convert an edge to an array.
		/// </summary>
		/// <returns>A new array composed of x and y.</returns>
		public int[] ToArray()
		{
			return new int[2] { x, y };
		}

		/// <summary>
		/// Compares edges and takes shared triangles into account.
		/// </summary>
		/// <param name="b">The edge to compare against</param>
		/// <param name="lookup">A common vertex indices lookup dictionary. See pb_IntArray for more information.</param>
		/// <remarks>Generally you just pass pb.sharedIndices.ToDictionary() to lookup, but it's more effecient to do it once and reuse that dictionary if possible.</remarks>
		/// <returns>True if edges are perceptually equal (that is, they point to the same common indices).</returns>
		public bool Equals(Edge b, Dictionary<int, int> lookup)
		{
            if (lookup == null)
                return Equals(b);
			int x0 = lookup[x], y0 = lookup[y], x1 = lookup[b.x], y1 = lookup[b.y];
			return (x0 == x1 && y0 == y1) || (x0 == y1 && y0 == x1);
		}

		/// <summary>
		/// Does this edge contain an index?
		/// </summary>
		/// <param name="a">The index to compare against x and y.</param>
		/// <returns>True if x or y is equal to a. False if not.</returns>
		public bool Contains(int a)
		{
			return (x == a || y == a);
		}

		/// <summary>
		/// Does this edge have any matching index to edge b?
		/// </summary>
		/// <param name="b">The edge to compare against.</param>
		/// <returns>True if x or y matches either b.x or b.y.</returns>
		public bool Contains(Edge b)
		{
			return (x == b.x || y == b.x || x == b.y || y == b.x);
		}

		internal bool Contains(int a, IntArray[] sharedIndices)
		{
			// @todo optimize
			int ind = sharedIndices.IndexOf(a);
			return ( System.Array.IndexOf(sharedIndices[ind], x) > -1 || System.Array.IndexOf(sharedIndices[ind], y) > -1);
		}
	}
}
