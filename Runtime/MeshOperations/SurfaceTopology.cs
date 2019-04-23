using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Utilities for working with triangle and quad primitives.
    /// </summary>
    public static class SurfaceTopology
    {
        /// <summary>
        /// Convert a selection of faces from n-gons to triangles.
        /// <br />
        /// If a face is successfully converted to triangles, each new triangle is created as a separate face and the original face is deleted.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="faces">The faces to convert from quads to triangles.</param>
        /// <returns>Any new triangle faces created by breaking faces into individual triangles.</returns>
        public static Face[] ToTriangles(this ProBuilderMesh mesh, IList<Face> faces)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            if (faces == null)
                throw new System.ArgumentNullException("faces");

            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;

            List<FaceRebuildData> rebuild = new List<FaceRebuildData>();

            foreach (Face face in faces)
            {
                List<FaceRebuildData> res = BreakFaceIntoTris(face, vertices, lookup);
                rebuild.AddRange(res);
            }

            FaceRebuildData.Apply(rebuild, mesh, vertices, null);
            mesh.DeleteFaces(faces);
            mesh.ToMesh();

            return rebuild.Select(x => x.face).ToArray();
        }

        static List<FaceRebuildData> BreakFaceIntoTris(Face face, List<Vertex> vertices, Dictionary<int, int> lookup)
        {
            int[] tris = face.indexesInternal;
            int triCount = tris.Length;
            List<FaceRebuildData> rebuild = new List<FaceRebuildData>(triCount / 3);

            for (int i = 0; i < triCount; i += 3)
            {
                FaceRebuildData r = new FaceRebuildData();

                r.face = new Face(face);
                r.face.indexesInternal = new int[] { 0, 1, 2};

                r.vertices = new List<Vertex>() {
                    vertices[tris[i]],
                    vertices[tris[i + 1]],
                    vertices[tris[i + 2]]
                };

                r.sharedIndexes = new List<int>() {
                    lookup[tris[i]],
                    lookup[tris[i + 1]],
                    lookup[tris[i + 2]]
                };

                rebuild.Add(r);
            }

            return rebuild;
        }

        /// <summary>
        /// Attempt to extract the winding order for a face.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to test.</param>
        /// <returns>The winding order if successfull, unknown if not.</returns>
        public static WindingOrder GetWindingOrder(this ProBuilderMesh mesh, Face face)
        {
            Vector2[] p = Projection.PlanarProject(mesh.positionsInternal, face.distinctIndexesInternal);
            return GetWindingOrder(p);
        }

        static WindingOrder GetWindingOrder(IList<Vertex> vertices, IList<int> indexes)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            if (indexes == null)
                throw new ArgumentNullException("indexes");

            Vector2[] p = Projection.PlanarProject(vertices.Select(x => x.position).ToArray(), indexes);
            return GetWindingOrder(p);
        }

        /// <summary>
        /// Return the winding order of a set of ordered points.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order</remarks>
        /// <param name="points">A path of points in 2d space.</param>
        /// <returns>The winding order if found, WindingOrder.Unknown if not.</returns>
        public static WindingOrder GetWindingOrder(IList<Vector2> points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            float sum = 0f;

            int len = points.Count;

            // http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
            for (int i = 0; i < len; i++)
            {
                Vector2 a = points[i];
                Vector2 b = i < len - 1 ? points[i + 1] : points[0];

                sum += ((b.x - a.x) * (b.y + a.y));
            }

            return sum == 0f ? WindingOrder.Unknown : (sum > 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
        }

        /// <summary>
        /// Reverses the orientation of the middle edge in a quad.
        /// <![CDATA[
        /// ```
        /// .  _____        _____
        /// . |\    |      |    /|
        /// . |  \  |  =>  |  /  |
        /// . |____\|      |/____|
        /// ```
        /// ]]>
        /// </summary>
        /// <param name="mesh">The mesh that face belongs to.</param>
        /// <param name="face">The target face.</param>
        /// <returns>True if successful, false if not. Operation will fail if face does not contain two triangles with exactly 2 shared vertices.</returns>
        public static bool FlipEdge(this ProBuilderMesh mesh, Face face)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (face == null)
                throw new ArgumentNullException("face");

            int[] indexes = face.indexesInternal;

            if (indexes.Length != 6)
                return false;

            int[] mode = ArrayUtility.Fill<int>(1, indexes.Length);

            for (int x = 0; x < indexes.Length - 1; x++)
            {
                for (int y = x + 1; y < indexes.Length; y++)
                {
                    if (indexes[x] == indexes[y])
                    {
                        mode[x]++;
                        mode[y]++;
                    }
                }
            }

            if (mode[0] + mode[1] + mode[2] != 5 ||
                mode[3] + mode[4] + mode[5] != 5)
                return false;

            int i0 = indexes[mode[0] == 1 ? 0 : mode[1] == 1 ? 1 : 2];
            int i1 = indexes[mode[3] == 1 ? 3 : mode[4] == 1 ? 4 : 5];

            int used = -1;

            if (mode[0] == 2)
            {
                used = indexes[0];
                indexes[0] =  i1;
            }
            else if (mode[1] == 2)
            {
                used = indexes[1];
                indexes[1] = i1;
            }
            else if (mode[2] == 2)
            {
                used = indexes[2];
                indexes[2] = i1;
            }

            if (mode[3] == 2 && indexes[3] != used)
                indexes[3] = i0;
            else if (mode[4] == 2 && indexes[4] != used)
                indexes[4] = i0;
            else if (mode[5] == 2 && indexes[5] != used)
                indexes[5] = i0;

            face.InvalidateCache();

            return true;
        }

        /// <summary>
        /// Ensure that all adjacent face normals are pointing in a uniform direction. This function supports multiple islands of connected faces, but it may not unify each island the same way.
        /// </summary>
        /// <param name="mesh">The mesh that the faces belong to.</param>
        /// <param name="faces">The faces to make uniform.</param>
        /// <returns>The state of the action.</returns>
        public static ActionResult ConformNormals(this ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, faces);
            HashSet<Face> used = new HashSet<Face>();
            int count = 0;

            // this loop adds support for multiple islands of grouped selections
            for (int i = 0; i < wings.Count; i++)
            {
                if (used.Contains(wings[i].face))
                    continue;

                Dictionary<Face, bool> flags = new Dictionary<Face, bool>();

                GetWindingFlags(wings[i], true, flags);

                int flip = 0;

                foreach (var kvp in flags)
                    flip += kvp.Value ? 1 : -1;

                bool direction = flip > 0;

                foreach (var kvp in flags)
                {
                    if (direction != kvp.Value)
                    {
                        count++;
                        kvp.Key.Reverse();
                    }
                }

                used.UnionWith(flags.Keys);
            }

            if (count > 0)
                return new ActionResult(ActionResult.Status.Success, count > 1 ? string.Format("Flipped {0} faces", count) : "Flipped 1 face");
            else
                return new ActionResult(ActionResult.Status.NoChange, "Faces Uniform");
        }

        static void GetWindingFlags(WingedEdge edge, bool flag, Dictionary<Face, bool> flags)
        {
            flags.Add(edge.face, flag);

            WingedEdge next = edge;

            do
            {
                WingedEdge opp = next.opposite;

                if (opp != null && !flags.ContainsKey(opp.face))
                {
                    Edge cea = GetCommonEdgeInWindingOrder(next);
                    Edge ceb = GetCommonEdgeInWindingOrder(opp);

                    GetWindingFlags(opp, cea.a == ceb.a ? !flag : flag, flags);
                }

                next = next.next;
            }
            while (next != edge);
        }

        /// <summary>
        /// Ensure the opposite face to source matches the winding order.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult ConformOppositeNormal(WingedEdge source)
        {
            if (source == null || source.opposite == null)
                return new ActionResult(ActionResult.Status.Failure, "Source edge does not share an edge with another face.");

            Edge cea = GetCommonEdgeInWindingOrder(source);
            Edge ceb = GetCommonEdgeInWindingOrder(source.opposite);

            if (cea.a == ceb.a)
            {
                source.opposite.face.Reverse();

                return new ActionResult(ActionResult.Status.Success, "Reversed target face winding order.");
            }

            return new ActionResult(ActionResult.Status.NoChange, "Faces already unified.");
        }

        /// <summary>
        /// Iterate a face and return a new common edge where the edge indexes are true to the triangle winding order.
        /// </summary>
        /// <param name="wing"></param>
        /// <returns></returns>
        static Edge GetCommonEdgeInWindingOrder(WingedEdge wing)
        {
            int[] indexes = wing.face.indexesInternal;
            int len = indexes.Length;

            for (int i = 0; i < len; i += 3)
            {
                Edge e = wing.edge.local;
                int a = indexes[i], b = indexes[i + 1], c = indexes[i + 2];

                if (e.a == a && e.b == b)
                    return wing.edge.common;
                else if (e.a == b && e.b == a)
                    return new Edge(wing.edge.common.b, wing.edge.common.a);
                else if (e.a == b && e.b == c)
                    return wing.edge.common;
                else if (e.a == c && e.b == b)
                    return new Edge(wing.edge.common.b, wing.edge.common.a);
                else if (e.a == c && e.b == a)
                    return wing.edge.common;
                else if (e.a == a && e.b == c)
                    return new Edge(wing.edge.common.b, wing.edge.common.a);
            }

            return Edge.Empty;
        }

        /// <summary>
        /// Match a target face to the source face. Faces must be adjacent.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="lookup"></param>
        internal static void MatchNormal(Face source, Face target, Dictionary<int, int> lookup)
        {
            List<EdgeLookup> sourceEdges = EdgeLookup.GetEdgeLookup(source.edgesInternal, lookup).ToList();
            List<EdgeLookup> targetEdges = EdgeLookup.GetEdgeLookup(target.edgesInternal, lookup).ToList();

            bool superBreak = false;

            Edge src, tar;

            for (int i = 0; !superBreak && i < sourceEdges.Count; i++)
            {
                src = sourceEdges[i].common;

                for (int n = 0; !superBreak && n < targetEdges.Count; n++)
                {
                    tar = targetEdges[n].common;

                    if (src.Equals(tar))
                    {
                        if (src.a == tar.a)
                            target.Reverse();

                        superBreak = true;
                    }
                }
            }
        }
    }
}
