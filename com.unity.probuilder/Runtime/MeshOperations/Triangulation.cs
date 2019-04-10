using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Poly2Tri;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Wrapper around Triangle.NET triangulation methods. https://github.com/zon/triangle
    /// </summary>
    static class Triangulation
    {
        /// <summary>
        /// Given a set of points this method will format the points into a boundary contour and triangulate, returning
        /// a set of indexes that corresponds to the original ordering.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="indexes"></param>
        /// <param name="convex"></param>
        /// <returns></returns>
        public static bool SortAndTriangulate(IList<Vector2> points, out List<int> indexes, bool convex = false)
        {
            IList<Vector2> sorted = Projection.Sort(points, SortMethod.CounterClockwise);

            Dictionary<int, int> map = new Dictionary<int, int>();

            for (int i = 0; i < sorted.Count; i++)
                map.Add(i, points.IndexOf(sorted[i]));

            if (!Triangulate(sorted, out indexes, convex))
                return false;

            for (int i = 0; i < indexes.Count; i++)
                indexes[i] = map[indexes[i]];

            return true;
        }

        /// <summary>
        /// Attempts to triangulate a set of vertices. If unordered is specified as false vertices will not be re-ordered before triangulation.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="unordered"></param>
        /// <param name="convex"></param>
        /// <returns></returns>
        public static bool TriangulateVertices(IList<Vertex> vertices, out List<int> triangles, bool unordered = true, bool convex = false)
        {
            Vector3[] facePoints = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
                facePoints[i] = vertices[i].position;

            return TriangulateVertices(facePoints, out triangles, unordered, convex);
        }

        public static bool TriangulateVertices(Vector3[] vertices, out List<int> triangles, bool unordered = true, bool convex = false)
        {
            triangles = null;
            int vertexCount = vertices == null ? 0 : vertices.Length;

            if (vertexCount < 3)
                return false;

            if (vertexCount == 3)
            {
                triangles = new List<int>() { 0, 1, 2 };
                return true;
            }

            Vector2[] points2d = Projection.PlanarProject(vertices);

            if (unordered)
                return SortAndTriangulate(points2d, out triangles, convex);

            return Triangulate(points2d, out triangles, convex);
        }

        /// <summary>
        /// Given a set of points ordered counter-clockwise along a contour, return triangle indexes.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="indexes"></param>
        /// <param name="convex">Triangulation may optionally be set to convex, which will result in some a convex shape.</param>
        /// <returns></returns>
        public static bool Triangulate(IList<Vector2> points, out List<int> indexes, bool convex = false)
        {
            indexes = new List<int>();

            int index = 0;

            Triangulatable soup = convex
                ? new PointSet(points.Select(x => new TriangulationPoint(x.x, x.y, index++)).ToList())
                : (Triangulatable) new Polygon(points.Select(x => new PolygonPoint(x.x, x.y, index++)));

            try
            {
                P2T.Triangulate(TriangulationAlgorithm.DTSweep, soup);
            }
            catch (System.Exception e)
            {
                Log.Info("Triangulation failed: " + e.ToString());
                return false;
            }

            foreach (DelaunayTriangle d in soup.Triangles)
            {
                if (d.Points[0].Index < 0 || d.Points[1].Index < 0 || d.Points[2].Index < 0)
                {
                    Log.Info("Triangulation failed: Additional vertices were inserted.");
                    return false;
                }

                indexes.Add(d.Points[0].Index);
                indexes.Add(d.Points[1].Index);
                indexes.Add(d.Points[2].Index);
            }

            WindingOrder originalWinding = SurfaceTopology.GetWindingOrder(points);

            // if the re-triangulated first tri doesn't match the winding order of the original
            // vertices, flip 'em
            if (SurfaceTopology.GetWindingOrder(new Vector2[3]
            {
                points[indexes[0]],
                points[indexes[1]],
                points[indexes[2]],
            }) != originalWinding)
                indexes.Reverse();

            return true;
        }
    }
}
