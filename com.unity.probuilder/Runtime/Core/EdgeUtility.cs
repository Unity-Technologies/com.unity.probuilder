using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Utilities for working with pb_Edge.
    /// </summary>
    static class EdgeUtility
    {
        /// <summary>
        /// Returns new edges where each edge is composed not of vertex indexes, but rather the index in ProBuilderMesh.sharedIndexes of each vertex.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static IEnumerable<Edge> GetSharedVertexHandleEdges(this ProBuilderMesh mesh, IEnumerable<Edge> edges)
        {
            return edges.Select(x => GetSharedVertexHandleEdge(mesh, x));
        }

        public static Edge GetSharedVertexHandleEdge(this ProBuilderMesh mesh, Edge edge)
        {
            return new Edge(mesh.sharedVertexLookup[edge.a], mesh.sharedVertexLookup[edge.b]);
        }

        /// <summary>
        /// Converts a universal edge to local.  Does *not* guarantee that edges will be valid (indexes belong to the same face and edge).
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        internal static Edge GetEdgeWithSharedVertexHandles(this ProBuilderMesh mesh, Edge edge)
        {
            return new Edge(mesh.sharedVerticesInternal[edge.a][0], mesh.sharedVerticesInternal[edge.b][0]);
        }

        /// <summary>
        /// Given a local edge, this guarantees that both indexes belong to the same face.
        /// Note that this will only return the first valid edge found - there will usually
        /// be multiple matches (well, 2 if your geometry is sane).
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="edge"></param>
        /// <param name="validEdge"></param>
        /// <returns></returns>
        public static bool ValidateEdge(ProBuilderMesh mesh, Edge edge, out SimpleTuple<Face, Edge> validEdge)
        {
            Face[] faces = mesh.facesInternal;
            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;

            Edge universal = GetSharedVertexHandleEdge(mesh, edge);

            for (int i = 0; i < faces.Length; i++)
            {
                int dist_x = -1,
                    dist_y = -1,
                    shared_x = -1,
                    shared_y = -1;

                if (faces[i].distinctIndexesInternal.ContainsMatch(sharedIndexes[universal.a].arrayInternal, out dist_x, out shared_x) &&
                    faces[i].distinctIndexesInternal.ContainsMatch(sharedIndexes[universal.b].arrayInternal, out dist_y, out shared_y))
                {
                    int x = faces[i].distinctIndexesInternal[dist_x];
                    int y = faces[i].distinctIndexesInternal[dist_y];

                    validEdge = new SimpleTuple<Face, Edge>(faces[i], new Edge(x, y));
                    return true;
                }
            }

            validEdge = new SimpleTuple<Face, Edge>();

            return false;
        }

        /// <summary>
        /// Fast contains. Doesn't account for shared indexes
        /// </summary>
        internal static bool Contains(this Edge[] edges, Edge edge)
        {
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].Equals(edge))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Fast contains. Doesn't account for shared indexes
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal static bool Contains(this Edge[] edges, int x, int y)
        {
            for (int i = 0; i < edges.Length; i++)
            {
                if ((x == edges[i].a && y == edges[i].b) || (x == edges[i].b && y == edges[i].a))
                    return true;
            }

            return false;
        }

        internal static int IndexOf(this ProBuilderMesh mesh, IList<Edge> edges, Edge edge)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Equals(edge, mesh.sharedVertexLookup))
                    return i;
            }

            return -1;
        }

        internal static int[] AllTriangles(this Edge[] edges)
        {
            int[] arr = new int[edges.Length * 2];
            int n = 0;

            for (int i = 0; i < edges.Length; i++)
            {
                arr[n++] = edges[i].a;
                arr[n++] = edges[i].b;
            }
            return arr;
        }

        internal static Face GetFace(this ProBuilderMesh mesh, Edge edge)
        {
            Face res = null;

            foreach (var face in mesh.facesInternal)
            {
                var edges = face.edgesInternal;

                for (int i = 0, c = edges.Length; i < c; i++)
                {
                    if (edge.Equals(edges[i]))
                        return face;

                    if (edges.Contains(edges[i]))
                        res = face;
                }
            }

            return res;
        }
    }
}
