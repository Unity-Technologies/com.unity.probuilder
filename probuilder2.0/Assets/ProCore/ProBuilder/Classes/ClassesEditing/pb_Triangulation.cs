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
	}
}
