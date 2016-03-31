using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

using TMesh = TriangleNet.Mesh;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Wrapper around Triangle.NET triangulation methods.
	 *	https://github.com/zon/triangle
	 */
	public static class pb_Triangulation
	{
		/**
		 *	Given a set of points this method will format the points into a boundary contour and triangulate,
		 *	returning a set of indices that corresponds to the original ordering.
		 */
		public static bool SortAndTriangulate(IList<Vector2> points, out List<int> indices, bool convex = false)
		{
			IList<Vector2> sorted = pb_Math.Sort(points, SortMethod.CounterClockwise);

			Dictionary<int, int> map = new Dictionary<int, int>();

			for(int i = 0; i < sorted.Count; i++)
				map.Add(i, points.IndexOf(sorted[i]));

			if(!Triangulate(sorted, out indices, convex))
				return false;

			for(int i = 0; i < indices.Count; i++)
				indices[i] = map[indices[i]];

			return true;
		}

		/**
		 *	Given a set of points ordered counter-clockwise along a contour, return triangle indices.
		 *	Triangulation may optionally be set to convex, which will result in some a convex shape.
		 */
		public static bool Triangulate(IList<Vector2> points, out List<int> indices, bool convex = false)
		{
			indices = new List<int>();

			int vertexCount = points.Count;
			InputGeometry input = new InputGeometry(vertexCount);

			for(int i = 0; i < vertexCount; i++)
			{
				input.AddPoint(points[i].x, points[i].y, 2);
				input.AddSegment(i, (i + 1) % vertexCount, 2);
			}

			Behavior b = new Behavior();
			b.Convex = convex;
			b.ConformingDelaunay = false;
			b.NoBisect = 2;			// prevent all splitting
			b.NoHoles = true;
			b.Jettison = false;		// don't jettison unused vertices

			TMesh tm = new TMesh(b);
			tm.Triangulate(input);

			if(tm.Vertices.Count != points.Count)
			{
				Debug.LogWarning("Triangulation has inserted additional vertices.");
				return false;
			}

			// Ensures vertex indices are kept linear so that triangles match the points array.
			tm.Renumber(NodeNumbering.Linear);

			IEnumerable<Triangle> triangles = tm.Triangles;

			foreach(Triangle t in triangles)
			{
				// Triangle.NET assumes right-handed coordinates; flip tris
				indices.Add( t.P2 );
				indices.Add( t.P1 );
				indices.Add( t.P0 );
			}

			return true;
		}
	}
}
