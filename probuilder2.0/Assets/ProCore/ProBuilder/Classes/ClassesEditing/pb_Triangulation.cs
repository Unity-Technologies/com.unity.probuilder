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
	 */
	public static class pb_Triangulation
	{
		/**
		 *	Given a set of points this method will format the points into a boundary contour and triangulate,
		 *	returning a set of indices that corresponds to the original ordering.
		 */
		public static List<int> SortAndTriangulate(IList<Vector2> points, bool convex = false)
		{
			IList<Vector2> sorted = pb_Math.Sort(points, SortMethod.CounterClockwise);

			Dictionary<int, int> map = new Dictionary<int, int>();

			for(int i = 0; i < sorted.Count; i++)
				map.Add(i, points.IndexOf(sorted[i]));

			List<int> indices = Triangulate(sorted, convex);

			for(int i = 0; i < indices.Count; i++)
				indices[i] = map[indices[i]];

			return indices;
		}

		/**
		 *	Given a set of points ordered counter-clockwise along a contour, return triangle indices.
		 *	Triangulation may optionally be set to convex, which will result in some a convex shape.
		 */
		public static List<int> Triangulate(IList<Vector2> points, bool convex = false)
		{
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
			// Ensures vertex indices are kept linear so that triangles match the points array.
			tm.Renumber(NodeNumbering.Linear);

			IEnumerable<Triangle> triangles = tm.Triangles;
			List<int> indices = new List<int>();

			foreach(Triangle t in triangles)
			{
				// Triangle.NET assumes right-handed coordinates; flip tris
				indices.Add( t.P2 );
				indices.Add( t.P1 );
				indices.Add( t.P0 );
			}

			return indices;
		}

		/**
		 * Re-triangulates a face with existing indices and vertices.  This function assumes that if convex is false
		 * the points are already sorted.  If points are not sorted to a contour, use pb_Math.SortCounterClockwise.
		 */
		public static void Triangulate(this pb_Object pb, pb_Face face, bool convex = false)
		{
			Vector2[] v2d = pb_Math.PlanarProject(pb.vertices, pb_Math.Normal(pb, face), face.indices);
			List<int> indices = Triangulate(v2d, convex);
			face.SetIndices(indices.ToArray());
		}
	}
}
