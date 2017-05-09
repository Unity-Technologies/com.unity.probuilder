using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

// using TMesh = TriangleNet.Mesh;
// using TriangleNet;
// using TriangleNet.Data;
// using TriangleNet.Geometry;
using Poly2Tri;

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
			int index = 0;
			Polygon poly = new Polygon( points.Select(x => new PolygonPoint(x.x, x.y, index++)) );

			indices = new List<int>();

			P2T.Triangulate(poly);

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach(DelaunayTriangle d in poly.Triangles)
			{
				sb.AppendLine(string.Format("{2}  -  {0}, {1}", d.Points[0].X, d.Points[0].Y, d.Points[0].Index));
				sb.AppendLine(string.Format("{2}  -  {0}, {1}", d.Points[1].X, d.Points[1].Y, d.Points[1].Index));
				sb.AppendLine(string.Format("{2}  -  {0}, {1}", d.Points[2].X, d.Points[2].Y, d.Points[2].Index));

				if(d.Points[0].Index < 0 || d.Points[1].Index < 0 || d.Points[2].Index < 0)
					return false;

				indices.Add( d.Points[0].Index );
				indices.Add( d.Points[1].Index );
				indices.Add( d.Points[2].Index );
			}

			Debug.Log(sb.ToString());

			WindingOrder originalWinding = pbTriangleOps.GetWindingOrder(points);

			// // if the re-triangulated first tri doesn't match the winding order of the original
			// // vertices, flip 'em
			if( pbTriangleOps.GetWindingOrder(new Vector2[3]{
				points[indices[0]],
				points[indices[1]],
				points[indices[2]],
				}) != originalWinding)
				indices.Reverse();

			return true;
		}
	}
}
