using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using Poly2Tri;

namespace ProBuilder2.MeshOperations
{
	/// <summary>
	/// Wrapper around Triangle.NET triangulation methods. https://github.com/zon/triangle
	/// </summary>
	static class pb_Triangulation
	{
		/**
		 *	Given a set of points this method will format the points into a boundary contour and triangulate,
		 *	returning a set of indices that corresponds to the original ordering.
		 */
		public static bool SortAndTriangulate(IList<Vector2> points, out List<int> indices, bool convex = false)
		{
			IList<Vector2> sorted = pb_Projection.Sort(points, SortMethod.CounterClockwise);

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
		 *	Attempts to triangulate a set of vertices.
		 *	If unordered is specified as false vertices will not be re-ordered before triangulation.
		 */
		public static bool TriangulateVertices(IList<pb_Vertex> vertices, out List<int> triangles, bool unordered = true, bool convex = false)
		{
			Vector3[] facePoints = new Vector3[vertices.Count];

			for(int i = 0; i < vertices.Count; ++i)
				facePoints[i] = vertices[i].position;

			return TriangulateVertices(facePoints, out triangles, unordered, convex);
		}

		public static bool TriangulateVertices(Vector3[] vertices, out List<int> triangles, bool unordered = true, bool convex = false)
		{
			triangles = null;
			int vertexCount = vertices == null ? 0 : vertices.Length;

			if(vertexCount < 3)
				return false;

			if(vertexCount == 3)
			{
				triangles = new List<int>() { 0, 1, 2 };
				return true;
			}

			Vector3 normal = pb_Projection.FindBestPlane(vertices).normal;
			Vector2[] points2d = pb_Projection.PlanarProject(vertices, normal);

			if(unordered)
				return pb_Triangulation.SortAndTriangulate(points2d, out triangles, convex);
			else
				return Triangulate(points2d, out triangles, convex);
		}

		/**
		 *	Given a set of points ordered counter-clockwise along a contour, return triangle indices.
		 *	Triangulation may optionally be set to convex, which will result in some a convex shape.
		 */
		public static bool Triangulate(IList<Vector2> points, out List<int> indices, bool convex = false)
		{
			indices = new List<int>();

			int index = 0;

			Triangulatable soup = convex ?
				(Triangulatable) new PointSet(points.Select(x => new TriangulationPoint(x.x, x.y, index++)).ToList()) :
				(Triangulatable) new Polygon(points.Select(x => new PolygonPoint(x.x, x.y, index++)));

			P2T.Triangulate(TriangulationAlgorithm.DTSweep, soup);

			foreach(DelaunayTriangle d in soup.Triangles)
			{
				if(d.Points[0].Index < 0 || d.Points[1].Index < 0 || d.Points[2].Index < 0)
				{
					pb_Log.Warning("Triangulation failed - additional vertices were inserted.");
					return false;
				}

				indices.Add( d.Points[0].Index );
				indices.Add( d.Points[1].Index );
				indices.Add( d.Points[2].Index );
			}

			WindingOrder originalWinding = pb_TriangleOps.GetWindingOrder(points);

			// // if the re-triangulated first tri doesn't match the winding order of the original
			// // vertices, flip 'em
			if( pb_TriangleOps.GetWindingOrder(new Vector2[3]{
				points[indices[0]],
				points[indices[1]],
				points[indices[2]],
				}) != originalWinding)
				indices.Reverse();

			return true;
		}
	}
}
