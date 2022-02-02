using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Provides functions for beveling edges.
    /// </summary>
    public static class Bevel
    {
        /// <summary>
        /// Applies a bevel to a set of edges.
        ///
        /// This is the equivalent of the [Bevel (Edge)](../manual/Edge_Bevel.html) action.
        /// </summary>
        /// <param name="mesh">Target mesh.</param>
        /// <param name="edges">A set of edges to apply bevelling to.</param>
        /// <param name="amount">A value from 0 (do not bevel) to 1 (bevel the entire face).</param>
        /// <returns>The new faces created to form the bevel.</returns>
        public static List<Face> BevelEdges(ProBuilderMesh mesh, IList<Edge> edges, float amount)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            List<EdgeLookup> m_edges = EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);
            List<FaceRebuildData> appendFaces = new List<FaceRebuildData>();

            Dictionary<Face, List<int>> ignore = new Dictionary<Face, List<int>>();
            HashSet<int> slide = new HashSet<int>();
            int beveled = 0;

            Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>> holes = new Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>>();

            // test every edge that will be moved along to make sure the bevel distance is appropriate.  if it's not, adjust the max bevel amount
            // to suit.
            Dictionary<int, List<WingedEdge>> spokes = WingedEdge.GetSpokes(wings);
            HashSet<int> tested_common = new HashSet<int>();

            foreach (EdgeLookup e in m_edges)
            {
                if (tested_common.Add(e.common.a))
                {
                    foreach (WingedEdge w in spokes[e.common.a])
                    {
                        Edge le = w.edge.local;
                        amount = Mathf.Min(Vector3.Distance(vertices[le.a].position, vertices[le.b].position) - .001f, amount);
                    }
                }

                if (tested_common.Add(e.common.b))
                {
                    foreach (WingedEdge w in spokes[e.common.b])
                    {
                        Edge le = w.edge.local;
                        amount = Mathf.Min(Vector3.Distance(vertices[le.a].position, vertices[le.b].position) - .001f, amount);
                    }
                }
            }

            if (amount < .001f)
            {
                Log.Info("Bevel Distance > Available Surface");
                return null;
            }

            // iterate selected edges and move each leading edge back along it's direction
            // storing information about adjacent faces in the process
            foreach (EdgeLookup lup in m_edges)
            {
                WingedEdge we = wings.FirstOrDefault(x => x.edge.Equals(lup));

                if (we == null || we.opposite == null)
                    continue;

                beveled++;

                ignore.AddOrAppend(we.face, we.edge.common.a);
                ignore.AddOrAppend(we.face, we.edge.common.b);
                ignore.AddOrAppend(we.opposite.face, we.edge.common.a);
                ignore.AddOrAppend(we.opposite.face, we.edge.common.b);

                // after initial slides go back and split indirect triangles at the intersecting index into two vertices
                slide.Add(we.edge.common.a);
                slide.Add(we.edge.common.b);

                SlideEdge(vertices, we, amount);
                SlideEdge(vertices, we.opposite, amount);

                appendFaces.AddRange(GetBridgeFaces(vertices, we, we.opposite, holes));
            }

            if (beveled < 1)
            {
                Log.Info("Cannot Bevel Open Edges");
                return null;
            }

            // grab the "createdFaces" array now so that the selection returned is just the bridged faces
            // then add holes later
            var createdFaces = new List<Face>(appendFaces.Select(x => x.face));

            Dictionary<Face, List<SimpleTuple<WingedEdge, int>>> sorted = new Dictionary<Face, List<SimpleTuple<WingedEdge, int>>>();

            // sort the adjacent but affected faces into winged edge groups where each group contains a set of
            // unique winged edges pointing to the same face
            foreach (int c in slide)
            {
                IEnumerable<WingedEdge> matches = wings.Where(x => x.edge.common.Contains(c) && !(ignore.ContainsKey(x.face) && ignore[x.face].Contains(c)));

                HashSet<Face> used = new HashSet<Face>();

                foreach (WingedEdge match in matches)
                {
                    if (!used.Add(match.face))
                        continue;

                    sorted.AddOrAppend(match.face, new SimpleTuple<WingedEdge, int>(match, c));
                }
            }

            // now go through those sorted faces and apply the vertex exploding, keeping track of any holes created
            foreach (KeyValuePair<Face, List<SimpleTuple<WingedEdge, int>>> kvp in sorted)
            {
                // common index & list of vertices it was split into
                Dictionary<int, List<int>> appended;

                FaceRebuildData f = VertexEditing.ExplodeVertex(vertices, kvp.Value, amount, out appended);

                if (f == null)
                    continue;

                appendFaces.Add(f);

                foreach (var apv in appended)
                {
                    // organize holes by new face so that later we can compare the winding of the new face to the hole face
                    // holes are sorted by key: common index value: face, vertex list
                    holes.AddOrAppend(apv.Key, new SimpleTuple<FaceRebuildData, List<int>>(f, apv.Value));
                }
            }

            FaceRebuildData.Apply(appendFaces, mesh, vertices);
            int removed = mesh.DeleteFaces(sorted.Keys).Length;
            mesh.sharedTextures = new SharedVertex[0];
            mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);

            // @todo don't rebuild indexes, keep 'em cached
            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
            lookup = mesh.sharedVertexLookup;
            List<HashSet<int>> holesCommonIndexes = new List<HashSet<int>>();

            // offset the indexes of holes and cull any potential holes that are less than 3 indexes (not a hole :)
            foreach (KeyValuePair<int, List<SimpleTuple<FaceRebuildData, List<int>>>> hole in holes)
            {
                // less than 3 indexes in hole path; ain't a hole
                if (hole.Value.Sum(x => x.item2.Count) < 3)
                    continue;

                HashSet<int> holeCommon = new HashSet<int>();

                foreach (SimpleTuple<FaceRebuildData, List<int>> path in hole.Value)
                {
                    int offset = path.item1.Offset() - removed;

                    for (int i = 0; i < path.item2.Count; i++)
                        holeCommon.Add(lookup[path.item2[i] + offset]);
                }

                holesCommonIndexes.Add(holeCommon);
            }

            List<WingedEdge> modified = WingedEdge.GetWingedEdges(mesh, appendFaces.Select(x => x.face));

            // now go through the holes and create faces for them
            vertices = new List<Vertex>(mesh.GetVertices());

            List<FaceRebuildData> holeFaces = new List<FaceRebuildData>();

            foreach (HashSet<int> h in holesCommonIndexes)
            {
                // even if a set of hole indexes made it past the initial culling, the distinct part
                // may have reduced the index count
                if (h.Count < 3)
                {
                    continue;
                }
                // skip sorting the path if it's just a triangle
                if (h.Count < 4)
                {
                    List<Vertex> v = new List<Vertex>(mesh.GetVertices(h.Select(x => sharedIndexes[x][0]).ToList()));
                    holeFaces.Add(AppendElements.FaceWithVertices(v));
                }
                // if this hole has > 3 indexes, it needs a tent pole triangulation, which requires sorting into the perimeter order
                else
                {
                    List<int> holePath = WingedEdge.SortCommonIndexesByAdjacency(modified, h);
                    if (holePath != null)
                    {
                        List<Vertex> v =
                            new List<Vertex>(mesh.GetVertices(holePath.Select(x => sharedIndexes[x][0]).ToList()));
                        holeFaces.AddRange(AppendElements.TentCapWithVertices(v));
                    }
                }
            }

            FaceRebuildData.Apply(holeFaces, mesh, vertices);
            mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);

            // go through new faces and conform hole normals
            // get a hash of just the adjacent and bridge faces
            // HashSet<pb_Face> adjacent = new HashSet<pb_Face>(appendFaces.Select(x => x.face));
            // and also just the filled holes
            HashSet<Face> newFaces = new HashSet<Face>(holeFaces.Select(x => x.face));
            newFaces.UnionWith(createdFaces);
            // now append filled holes to the full list of added faces
            appendFaces.AddRange(holeFaces);

            List<WingedEdge> allNewFaceEdges = WingedEdge.GetWingedEdges(mesh, appendFaces.Select(x => x.face));

            for (int i = 0; i < allNewFaceEdges.Count && newFaces.Count > 0; i++)
            {
                WingedEdge wing = allNewFaceEdges[i];

                if (newFaces.Contains(wing.face))
                {
                    newFaces.Remove(wing.face);

                    // find first edge whose opposite face isn't a filled hole* then
                    // conform normal by that.
                    // *or is a filled hole but has already been conformed
                    using (var it = new WingedEdgeEnumerator(wing))
                    {
                        while (it.MoveNext())
                        {
                            var w = it.Current;

                            if (w.opposite != null && !newFaces.Contains(w.opposite.face))
                            {
                                w.face.submeshIndex = w.opposite.face.submeshIndex;
                                w.face.uv = new AutoUnwrapSettings(w.opposite.face.uv);
                                SurfaceTopology.ConformOppositeNormal(w.opposite);
                                break;
                            }
                        }
                    }
                }
            }

            mesh.ToMesh();

            return createdFaces;
        }

        static readonly int[] k_BridgeIndexesTri = new int[] { 2, 1, 0 };

        static List<FaceRebuildData> GetBridgeFaces(
            IList<Vertex> vertices,
            WingedEdge left,
            WingedEdge right,
            Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>> holes)
        {
            List<FaceRebuildData> faces = new List<FaceRebuildData>();

            FaceRebuildData rf = new FaceRebuildData();

            EdgeLookup a = left.edge;
            EdgeLookup b = right.edge;

            rf.vertices = new List<Vertex>()
            {
                vertices[a.local.a],
                vertices[a.local.b],
                vertices[a.common.a == b.common.a ? b.local.a : b.local.b],
                vertices[a.common.a == b.common.a ? b.local.b : b.local.a]
            };

            Vector3 an = Math.Normal(vertices, left.face.indexesInternal);
            Vector3 bn = Math.Normal(rf.vertices, k_BridgeIndexesTri);

            int[] triangles = new int[] { 2, 1, 0, 2, 3, 1 };

            if (Vector3.Dot(an, bn) < 0f)
                System.Array.Reverse(triangles);

            rf.face = new Face(
                    triangles,
                    left.face.submeshIndex,
                    AutoUnwrapSettings.tile,
                    -1,
                    -1,
                    -1,
                    false);

            faces.Add(rf);

            holes.AddOrAppend(a.common.a, new SimpleTuple<FaceRebuildData, List<int>>(rf, new List<int>() { 0, 2 }));
            holes.AddOrAppend(a.common.b, new SimpleTuple<FaceRebuildData, List<int>>(rf, new List<int>() { 1, 3 }));

            return faces;
        }

        static void SlideEdge(IList<Vertex> vertices, WingedEdge we, float amount)
        {
            we.face.manualUV = true;
            we.face.textureGroup = -1;

            Edge slide_x = GetLeadingEdge(we, we.edge.common.a);
            Edge slide_y = GetLeadingEdge(we, we.edge.common.b);

            if (!slide_x.IsValid() || !slide_y.IsValid())
                return;

            Vertex x = (vertices[slide_x.a] - vertices[slide_x.b]);
            x.Normalize();

            Vertex y = (vertices[slide_y.a] - vertices[slide_y.b]);
            y.Normalize();

            // need the pb_Vertex value to be modified, not reassigned in this array (which += does)
            vertices[we.edge.local.a].Add(x * amount);
            vertices[we.edge.local.b].Add(y * amount);
        }

        static Edge GetLeadingEdge(WingedEdge wing, int common)
        {
            if (wing.previous.edge.common.a == common)
                return new Edge(wing.previous.edge.local.b, wing.previous.edge.local.a);
            else if (wing.previous.edge.common.b == common)
                return new Edge(wing.previous.edge.local.a, wing.previous.edge.local.b);
            else if (wing.next.edge.common.a == common)
                return new Edge(wing.next.edge.local.b, wing.next.edge.local.a);
            else if (wing.next.edge.common.b == common)
                return new Edge(wing.next.edge.local.a, wing.next.edge.local.b);

            return Edge.Empty;
        }
    }
}
