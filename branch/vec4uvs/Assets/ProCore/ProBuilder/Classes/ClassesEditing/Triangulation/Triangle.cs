using System;
using System.Collections.Generic;
using System.Text;

namespace ProBuilder2.Triangulator.Geometry
{
	/// <summary>
	/// Triangle made from three point indexes
	/// </summary>
	public struct Triangle
	{
		/// <summary>
		/// First vertex index in triangle
		/// </summary>
		public int p1;
		/// <summary>
		/// Second vertex index in triangle
		/// </summary>
		public int p2;
		/// <summary>
		/// Third vertex index in triangle
		/// </summary>
		public int p3;
		/// <summary>
		/// Initializes a new instance of a triangle
		/// </summary>
		/// <param name="point1">Vertex 1</param>
		/// <param name="point2">Vertex 2</param>
		/// <param name="point3">Vertex 3</param>
		public Triangle(int point1, int point2, int point3)
		{
			p1 = point1; p2 = point2; p3 = point3;
		}

		public int[] ToArray()
		{
			return new int[3]
			{
				p1,
				p2,
				p3
			};
		}

		public override string ToString()
		{
			return "(" + p1 + ", " + p2 + ", " + p3 + ")";
		}
	}

	public static class TriangleExtension
	{
		public static int[] ToIntArray(this List<Geometry.Triangle> t)
		{
			int[] n = new int[t.Count*3];
			for(int i = 0; i < t.Count; i++)
			{
				int u = i*3;
				n[u+0] = t[i].p1;
				n[u+1] = t[i].p2;
				n[u+2] = t[i].p3;
			}
			return n;
		}
	}
}
