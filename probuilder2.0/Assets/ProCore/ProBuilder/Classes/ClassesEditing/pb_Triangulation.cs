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
		private static TMesh _tmesh;

		/**
		 *	Initializing a Triangle.Mesh is a non-trivial performance hit.  Cache the instance
		 *	since it already clears the triangulation data on Mesh.Triangulate calls.
		 */
		private static TMesh GetTMesh(bool convex)
		{
			if(_tmesh == null)
			{
				Behavior b = new Behavior();
				b.Convex = convex;
				b.ConformingDelaunay = false;
				b.NoBisect = 2;			// prevent all splitting
				b.NoHoles = true;
				b.Jettison = false;		// don't jettison unused vertices
				_tmesh = new TMesh(b);
			}
			else if(_tmesh.Behavior.Convex != convex)
			{
				_tmesh.Behavior.Convex = convex;
			}

			return _tmesh;
		}

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
			triangles = null;
			int vertexCount = vertices.Count;

			if(vertexCount < 3)
				return false;

			if(vertexCount == 3)
				triangles = new List<int>() { 0, 1, 2 };

			Vector3[] facePoints = new Vector3[vertices.Count];

			for(int i = 0; i < vertices.Count; ++i)
				facePoints[i] = vertices[i].position;

			Vector3 normal = pb_Projection.FindBestPlane(facePoints).normal;
			Vector2[] points2d = pb_Projection.PlanarProject(facePoints, normal);

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
			int vertexCount = points.Count;

			indices = new List<int>();
			InputGeometry input = new InputGeometry(vertexCount);

			WindingOrder originalWinding = pbTriangleOps.GetWindingOrder(points);

			for(int i = 0; i < vertexCount; i++)
			{
				input.AddPoint(points[i].x, points[i].y, 2);
				input.AddSegment(i, (i + 1) % vertexCount, 2);
			}

			TMesh tm = GetTMesh(convex);

			tm.Triangulate(input);

			if(tm.Vertices.Count != points.Count)
			{
				Debug.LogWarning("Triangulation has inserted additional vertices.\nUsually this happens if the order in which points are selected is not in a clockwise or counter-clockwise order around the perimeter of the polygon.");
				return false;
			}

			// Ensures vertex indices are kept linear so that triangles match the points array.
			tm.Renumber(NodeNumbering.Linear);

			foreach(Triangle t in tm.Triangles)
			{
				// Triangle.NET assumes right-handed coordinates; flip tris
				indices.Add( t.P2 );
				indices.Add( t.P1 );
				indices.Add( t.P0 );
			}

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
