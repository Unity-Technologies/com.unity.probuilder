using System.Linq;
using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Provides helper functions for working with selected faces, edges, and vertices.
    /// </summary>
    public static class ElementSelection
    {
        const int k_MaxHoleIterations = 2048;

        /// <summary>
        /// Creates a list of <see cref="Face"/> objects where each face is connected to a specific <see cref="Edge"/> in the ProBuilderMesh.
        /// </summary>
        /// <param name="mesh">The ProBuilder mesh containing the edge.</param>
        /// <param name="edge">The edge to evaluate.</param>
        /// <param name="neighborFaces">Specify an empty list of faces for the method to fill.</param>
        public static void GetNeighborFaces(ProBuilderMesh mesh, Edge edge, List<Face> neighborFaces)
        {
            var lookup = mesh.sharedVertexLookup;

            Edge uni = new Edge(lookup[edge.a], lookup[edge.b]);
            Edge e = new Edge(0, 0);

            for (int i = 0; i < mesh.facesInternal.Length; i++)
            {
                Edge[] edges = mesh.facesInternal[i].edgesInternal;
                for (int n = 0; n < edges.Length; n++)
                {
                    e.a = edges[n].a;
                    e.b = edges[n].b;

                    if ((uni.a == lookup[e.a] && uni.b == lookup[e.b]) ||
                        (uni.a == lookup[e.b] && uni.b == lookup[e.a]))
                    {
                        neighborFaces.Add(mesh.facesInternal[i]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of <![CDATA[SimpleTuple<Face, Edge>]]> where each face is connected to the passed edge.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        internal static List<SimpleTuple<Face, Edge>> GetNeighborFaces(ProBuilderMesh mesh, Edge edge)
        {
            List<SimpleTuple<Face, Edge>> faces = new List<SimpleTuple<Face, Edge>>();
            var lookup = mesh.sharedVertexLookup;

            Edge uni = new Edge(lookup[edge.a], lookup[edge.b]);
            Edge e = new Edge(0, 0);

            for (int i = 0; i < mesh.facesInternal.Length; i++)
            {
                Edge[] edges = mesh.facesInternal[i].edgesInternal;
                for (int n = 0; n < edges.Length; n++)
                {
                    e.a = edges[n].a;
                    e.b = edges[n].b;

                    if ((uni.a == lookup[e.a] && uni.b == lookup[e.b]) ||
                        (uni.a == lookup[e.b] && uni.b == lookup[e.a]))
                    {
                        faces.Add(new SimpleTuple<Face, Edge>(mesh.facesInternal[i], edges[n]));
                        break;
                    }
                }
            }
            return faces;
        }

        /// <summary>
        /// Gets all faces connected to each index taking into account shared vertices.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        internal static List<Face> GetNeighborFaces(ProBuilderMesh mesh, int[] indexes)
        {
            var lookup = mesh.sharedVertexLookup;
            List<Face> neighboring = new List<Face>();
            HashSet<int> shared = new HashSet<int>();

            foreach (int tri in indexes)
                shared.Add(lookup[tri]);

            for (int i = 0; i < mesh.facesInternal.Length; i++)
            {
                int[] dist = mesh.facesInternal[i].distinctIndexesInternal;

                for (int n = 0; n < dist.Length; n++)
                {
                    if (shared.Contains(lookup[dist[n]]))
                    {
                        neighboring.Add(mesh.facesInternal[i]);
                        break;
                    }
                }
            }

            return neighboring;
        }

        /// <summary>
        /// Returns a unique array of Edges connected to the passed vertex indexes.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        internal static Edge[] GetConnectedEdges(ProBuilderMesh mesh, int[] indexes)
        {
            var lookup = mesh.sharedVertexLookup;

            List<Edge> connectedEdges = new List<Edge>();

            HashSet<int> shared = new HashSet<int>();

            for (int i = 0; i < indexes.Length; i++)
                shared.Add(lookup[indexes[i]]);

            HashSet<Edge> used = new HashSet<Edge>();

            Edge uni = new Edge(0, 0);

            foreach (var face in mesh.facesInternal)
            {
                foreach (var edge in face.edges)
                {
                    Edge key = new Edge(lookup[edge.a], lookup[edge.b]);

                    if (shared.Contains(key.a) || shared.Contains(key.b) && !used.Contains(uni))
                    {
                        connectedEdges.Add(edge);
                        used.Add(key);
                    }
                }
            }

            return connectedEdges.ToArray();
        }

        /// <summary>
        /// Returns all the edges that are on the perimeter of this set of selected faces.
        /// </summary>
        /// <param name="mesh">The mesh containing the faces.</param>
        /// <param name="faces">The faces to search for perimeter edge paths.</param>
        /// <returns>A list of the edges on the perimeter of each group of adjacent faces.</returns>
        public static IEnumerable<Edge> GetPerimeterEdges(this ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            List<Edge> faceEdges = faces.SelectMany(x => x.edgesInternal).ToList(); // actual edges
            var sharedIndexesDictionary = mesh.sharedVertexLookup;
            int edgeCount = faceEdges.Count;

            // translate all face edges to universal edges
            Dictionary<Edge, List<Edge>> dup = new Dictionary<Edge, List<Edge>>();
            List<Edge> list;

            for (int i = 0; i < edgeCount; i++)
            {
                Edge uni = new Edge(sharedIndexesDictionary[faceEdges[i].a], sharedIndexesDictionary[faceEdges[i].b]);

                if (dup.TryGetValue(uni, out list))
                    list.Add(faceEdges[i]);
                else
                    dup.Add(uni, new List<Edge>() { faceEdges[i] });
            }

            return dup.Where(x => x.Value.Count < 2).Select(x => x.Value[0]);
        }

        /// <summary>
        /// Returns the indexes of perimeter edges in a given element group.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        internal static int[] GetPerimeterEdges(ProBuilderMesh mesh, IList<Edge> edges)
        {
            int edgeCount = edges != null ? edges.Count : 0;

            // Figure out how many connections each edge has to other edges in the selection
            var universal = mesh.GetSharedVertexHandleEdges(edges).ToArray();

            int[] connections = new int[universal.Length];

            for (int i = 0; i < universal.Length - 1; i++)
            {
                for (int n = i + 1; n < universal.Length; n++)
                {
                    if (universal[i].a == universal[n].a || universal[i].a == universal[n].b ||
                        universal[i].b == universal[n].a || universal[i].b == universal[n].b)
                    {
                        connections[i]++;
                        connections[n]++;
                    }
                }
            }

            int min = Math.Min(connections);
            List<int> perimeter = new List<int>();

            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] <= min)
                    perimeter.Add(i);
            }

            return perimeter.Count != edgeCount ? perimeter.ToArray() : new int[] {};
        }

        /// <summary>
        /// Returns an array of faces where each face has at least one non-shared edge.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        internal static IEnumerable<Face> GetPerimeterFaces(ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            var lookup = mesh.sharedVertexLookup;
            Dictionary<Edge, List<Face>> sharedEdges = new Dictionary<Edge, List<Face>>();

            /**
             * To be considered a perimeter face, at least one edge must not share
             * any boundary with another face.
             */

            foreach (Face face in faces)
            {
                foreach (Edge e in face.edgesInternal)
                {
                    Edge edge = new Edge(lookup[e.a], lookup[e.b]);

                    if (sharedEdges.ContainsKey(edge))
                        sharedEdges[edge].Add(face);
                    else
                        sharedEdges.Add(edge, new List<Face>() { face });
                }
            }

            return sharedEdges.Where(x => x.Value.Count < 2).Select(x => x.Value[0]).Distinct();
        }

        internal static int[] GetPerimeterVertices(ProBuilderMesh mesh, int[] indexes, Edge[] universal_edges_all)
        {
            int len = indexes.Length;
            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
            int[] universal = new int[len];

            for (int i = 0; i < len; i++)
                universal[i] = mesh.GetSharedVertexHandle(indexes[i]);

            int[] connections = new int[indexes.Length];

            for (int i = 0; i < indexes.Length - 1; i++)
            {
                for (int n = i + 1; n < indexes.Length; n++)
                {
                    if (universal_edges_all.Contains(universal[i], universal[n]))
                    {
                        connections[i]++;
                        connections[n]++;
                    }
                }
            }

            int min = Math.Min(connections);
            List<int> perimeter = new List<int>();
            for (int i = 0; i < len; i++)
            {
                if (connections[i] <= min)
                    perimeter.Add(i);
            }

            return perimeter.Count < len ? perimeter.ToArray() : new int[] {};
        }

        static WingedEdge EdgeRingNext(WingedEdge edge)
        {
            if (edge == null)
                return null;

            WingedEdge next = edge.next, prev = edge.previous;
            int i = 0;

            while (next != prev && next != edge)
            {
                next = next.next;

                if (next == prev)
                    return null;

                prev = prev.previous;

                i++;
            }

            if (i % 2 == 0 || next == edge)
                next = null;

            return next;
        }

        /// <summary>
        /// Iterates through face edges and builds a list using the opposite edge.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        internal static IEnumerable<Edge> GetEdgeRing(ProBuilderMesh pb, IEnumerable<Edge> edges)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb);
            List<EdgeLookup> edgeLookup = EdgeLookup.GetEdgeLookup(edges, pb.sharedVertexLookup).ToList();
            edgeLookup = edgeLookup.Distinct().ToList();

            Dictionary<Edge, WingedEdge> wings_dic = new Dictionary<Edge, WingedEdge>();

            for (int i = 0; i < wings.Count; i++)
                if (!wings_dic.ContainsKey(wings[i].edge.common))
                    wings_dic.Add(wings[i].edge.common, wings[i]);

            HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

            for (int i = 0, c = edgeLookup.Count; i < c; i++)
            {
                WingedEdge we;

                if (!wings_dic.TryGetValue(edgeLookup[i].common, out we) || used.Contains(we.edge))
                    continue;

                WingedEdge cur = we;

                while (cur != null)
                {
                    if (!used.Add(cur.edge)) break;
                    cur = EdgeRingNext(cur);
                    if (cur != null && cur.opposite != null) cur = cur.opposite;
                }

                cur = EdgeRingNext(we.opposite);
                if (cur != null && cur.opposite != null) cur = cur.opposite;

                // run in both directions
                while (cur != null)
                {
                    if (!used.Add(cur.edge)) break;
                    cur = EdgeRingNext(cur);
                    if (cur != null && cur.opposite != null) cur = cur.opposite;
                }
            }

            return used.Select(x => x.local);
        }

        /// <summary>
        /// Iterates through face edges and builds a list using the opposite edge, iteratively.
        /// </summary>
        /// <param name="pb">The probuilder mesh</param>
        /// <param name="edges">The edges already selected</param>
        /// <returns>The new selected edges</returns>
        internal static IEnumerable<Edge> GetEdgeRingIterative(ProBuilderMesh pb, IEnumerable<Edge> edges)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb);
            List<EdgeLookup> edgeLookup = EdgeLookup.GetEdgeLookup(edges, pb.sharedVertexLookup).ToList();
            edgeLookup = edgeLookup.Distinct().ToList();

            Dictionary<Edge, WingedEdge> wings_dic = new Dictionary<Edge, WingedEdge>();

            for (int i = 0; i < wings.Count; i++)
                if (!wings_dic.ContainsKey(wings[i].edge.common))
                    wings_dic.Add(wings[i].edge.common, wings[i]);

            HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

            for (int i = 0, c = edgeLookup.Count; i < c; i++)
            {
                WingedEdge we;

                if (!wings_dic.TryGetValue(edgeLookup[i].common, out we))
                    continue;

                WingedEdge cur = we;

                if (!used.Contains(cur.edge))
                    used.Add(cur.edge);
                var next = EdgeRingNext(cur);
                if (next != null && next.opposite != null && !used.Contains(next.edge))
                    used.Add(next.edge);
                var prev = EdgeRingNext(cur.opposite);
                if (prev != null && prev.opposite != null && !used.Contains(prev.edge))
                    used.Add(prev.edge);
            }

            return used.Select(x => x.local);
        }

        /// <summary>
        /// Attempts to find edges along an Edge loop.
        ///
        /// http://wiki.blender.org/index.php/Doc:2.4/Manual/Modeling/Meshes/Selecting/Edges says:
        /// First check to see if the selected element connects to only 3 other edges.
        /// If the edge in question has already been added to the list, the selection ends.
        /// Of the 3 edges that connect to the current edge, the ones that share a face with the current edge are eliminated
        /// and the remaining edge is added to the list and is made the current edge.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="edges"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        internal static bool GetEdgeLoop(ProBuilderMesh mesh, IEnumerable<Edge> edges, out Edge[] loop)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);
            IEnumerable<EdgeLookup> m_edgeLookup = EdgeLookup.GetEdgeLookup(edges, mesh.sharedVertexLookup);
            HashSet<EdgeLookup> sources = new HashSet<EdgeLookup>(m_edgeLookup);
            HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

            for (int i = 0; i < wings.Count; i++)
            {
                if (used.Contains(wings[i].edge) || !sources.Contains(wings[i].edge))
                    continue;

                bool completeLoop = GetEdgeLoopInternal(wings[i], wings[i].edge.common.b, used);

                // loop didn't close
                if (!completeLoop)
                    GetEdgeLoopInternal(wings[i], wings[i].edge.common.a, used);
            }

            loop = used.Select(x => x.local).ToArray();

            return true;
        }

        /// <summary>
        /// Attempts to find edges along an Edge loop in an iterative way
        ///
        /// Adds two edges to the selection, one at each extremity
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="lastEdgesAdded"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        internal static bool GetEdgeLoopIterative(ProBuilderMesh mesh, IEnumerable<Edge> edges, out Edge[] loop)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);
            IEnumerable<EdgeLookup> m_edgeLookup = EdgeLookup.GetEdgeLookup(edges, mesh.sharedVertexLookup);
            HashSet<EdgeLookup> sources = new HashSet<EdgeLookup>(m_edgeLookup);
            HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

            for (int i = 0; i < wings.Count; i++)
            {
                if (!sources.Contains(wings[i].edge))
                    continue;

                GetEdgeLoopInternalIterative(wings[i], wings[i].edge.common, used);
            }

            loop = used.Select(x => x.local).ToArray();

            return true;
        }

        static bool GetEdgeLoopInternal(WingedEdge start, int startIndex, HashSet<EdgeLookup> used)
        {
            int ind = startIndex;
            WingedEdge cur = start;

            do
            {
                used.Add(cur.edge);

                List<WingedEdge> spokes = GetSpokes(cur, ind, true).DistinctBy(x => x.edge.common).ToList();

                cur = null;

                if (spokes.Count == 4)
                {
                    cur = spokes[2];
                    ind = cur.edge.common.a == ind ? cur.edge.common.b : cur.edge.common.a;
                }
            }
            while (cur != null && !used.Contains(cur.edge));

            return cur != null;
        }

        static void GetEdgeLoopInternalIterative(WingedEdge start, Edge edge, HashSet<EdgeLookup> used)
        {
            int indA = edge.a;
            int indB = edge.b;
            WingedEdge cur = start;

            if (!used.Contains(cur.edge))
                used.Add(cur.edge);

            List<WingedEdge> spokesA = GetSpokes(cur, indA, true).DistinctBy(x => x.edge.common).ToList();
            List<WingedEdge> spokesB = GetSpokes(cur, indB, true).DistinctBy(x => x.edge.common).ToList();

            if (spokesA.Count == 4)
            {
                cur = spokesA[2];

                if (!used.Contains(cur.edge))
                    used.Add(cur.edge);
            }
            if (spokesB.Count == 4)
            {
                cur = spokesB[2];

                if (!used.Contains(cur.edge))
                    used.Add(cur.edge);
            }
        }

        static WingedEdge NextSpoke(WingedEdge wing, int pivot, bool opp)
        {
            if (opp)
                return wing.opposite;
            if (wing.next.edge.common.Contains(pivot))
                return wing.next;
            if (wing.previous.edge.common.Contains(pivot))
                return wing.previous;
            return null;
        }

        /// <summary>
        /// Return all edges connected to @wing with @sharedIndex as the pivot point. The first entry in the list is always the queried wing.
        /// </summary>
        /// <param name="wing"></param>
        /// <param name="sharedIndex"></param>
        /// <param name="allowHoles"></param>
        /// <returns></returns>
        internal static List<WingedEdge> GetSpokes(WingedEdge wing, int sharedIndex, bool allowHoles = false)
        {
            List<WingedEdge> spokes = new List<WingedEdge>();
            WingedEdge cur = wing;
            bool opp = false;

            do
            {
                // https://fogbugz.unity3d.com/f/cases/1241105/
                if (spokes.Contains(cur))
                    return spokes;

                spokes.Add(cur);
                cur = NextSpoke(cur, sharedIndex, opp);
                opp = !opp;

                // we've looped around as far as it's gon' go
                if (cur != null && cur.edge.common.Equals(wing.edge.common))
                    return spokes;
            }
            while (cur != null);

            if (!allowHoles)
                return null;

            // if the first loop didn't come back, that means there was a hole in the geo
            // do the loop again using the opposite wing
            cur = wing.opposite;
            opp = false;
            List<WingedEdge> fragment = new List<WingedEdge>();

            // if mesh is non-manifold this situation could arise
            while (cur != null && !cur.edge.common.Equals(wing.edge.common))
            {
                fragment.Add(cur);
                cur = NextSpoke(cur, sharedIndex, opp);
                opp = !opp;
            }

            fragment.Reverse();
            spokes.AddRange(fragment);

            return spokes;
        }

        /// <summary>
        /// Expand the selected faces to include any face touching the perimeter edges.
        /// This corresponds to the [Grow Selection](../manual/Selection_Grow.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to grow out from.</param>
        /// <param name="maxAngleDiff">Specify the maximum difference (in degrees) between the normals on the selected face and those on the perimeter face.</param>
        /// <returns>The original faces selection, plus any new faces added as a result of the grow operation.</returns>
        public static HashSet<Face> GrowSelection(ProBuilderMesh mesh, IEnumerable<Face> faces, float maxAngleDiff = -1f)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, true);
            HashSet<Face> source = new HashSet<Face>(faces);
            HashSet<Face> neighboring = new HashSet<Face>();

            Vector3 srcNormal = Vector3.zero;
            bool checkAngle = maxAngleDiff > 0f;

            for (int i = 0; i < wings.Count; i++)
            {
                if (!source.Contains(wings[i].face))
                    continue;

                if (checkAngle)
                    srcNormal = Math.Normal(mesh, wings[i].face);

                using (var it = new WingedEdgeEnumerator(wings[i]))
                {
                    while (it.MoveNext())
                    {
                        var w = it.Current;

                        if (w.opposite != null && !source.Contains(w.opposite.face))
                        {
                            if (checkAngle)
                            {
                                Vector3 oppNormal = Math.Normal(mesh, w.opposite.face);

                                if (Vector3.Angle(srcNormal, oppNormal) < maxAngleDiff)
                                    neighboring.Add(w.opposite.face);
                            }
                            else
                            {
                                neighboring.Add(w.opposite.face);
                            }
                        }
                    }
                }
            }

            return neighboring;
        }

        static readonly Vector3 Vector3_Zero = new Vector3(0f, 0f, 0f);

        internal static void Flood(WingedEdge wing, HashSet<Face> selection)
        {
            Flood(null, wing, Vector3_Zero, -1f, selection);
        }

        internal static void Flood(ProBuilderMesh pb, WingedEdge wing, Vector3 wingNrm, float maxAngle, HashSet<Face> selection)
        {
            WingedEdge next = wing;

            do
            {
                WingedEdge opp = next.opposite;

                if (opp != null && !selection.Contains(opp.face))
                {
                    if (maxAngle > 0f)
                    {
                        Vector3 oppNormal = Math.Normal(pb, opp.face);

                        if (Vector3.Angle(wingNrm, oppNormal) < maxAngle)
                        {
                            if (selection.Add(opp.face))
                                Flood(pb, opp, oppNormal, maxAngle, selection);
                        }
                    }
                    else
                    {
                        if (selection.Add(opp.face))
                            Flood(pb, opp, wingNrm, maxAngle, selection);
                    }
                }

                next = next.next;
            }
            while (next != wing);
        }

        /// <summary>
        /// Recursively adds all faces touching any of the selected faces to the selection.
        ///
        /// This corresponds to the [Grow Selection](../manual/Selection_Grow.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The selected faces.</param>
        /// <param name="maxAngleDiff">Specify the maximum difference (in degrees) between the normals on the selected face and those on the perimeter face.</param>
        /// <returns>The original faces selection, plus any new faces added as a result of the grow operation.</returns>
        public static HashSet<Face> FloodSelection(ProBuilderMesh mesh, IList<Face> faces, float maxAngleDiff)
        {
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, true);
            HashSet<Face> source = new HashSet<Face>(faces);
            HashSet<Face> flood = new HashSet<Face>();

            for (int i = 0; i < wings.Count; i++)
            {
                if (!flood.Contains(wings[i].face) && source.Contains(wings[i].face))
                {
                    flood.Add(wings[i].face);
                    Flood(mesh, wings[i], maxAngleDiff > 0f ? Math.Normal(mesh, wings[i].face) : Vector3_Zero, maxAngleDiff, flood);
                }
            }
            return flood;
        }

        /// <summary>
        /// Finds and returns a face loop.
        ///
        /// This is the equivalent of the [Select Face Loop](../manual/Selection_Loop_Face.html) and
        /// [Select Face Ring](../manual/Selection_Ring_Face.html) actions.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to scan for loops.</param>
        /// <param name="ring">Toggles between loop and ring. Ring and loop are arbritary with faces, so this parameter just toggles between which gets scanned first.</param>
        /// <returns>A collection of faces gathered by extending a ring or loop,</returns>
        public static HashSet<Face> GetFaceLoop(ProBuilderMesh mesh, Face[] faces, bool ring = false)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            HashSet<Face> loops = new HashSet<Face>();
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);

            foreach (Face face in faces)
                loops.UnionWith(GetFaceLoop(wings, face, ring));

            return loops;
        }

        /// <summary>
        /// Finds and returns both a face ring and loop from the selected faces.
        /// This is the equivalent of the [Select Face Loop](../manual/Selection_Loop_Face.html) and
        /// [Select Face Ring](../manual/Selection_Ring_Face.html) actions.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to scan for ring and loops.</param>
        /// <returns>A collection of faces gathered by extending in a ring and loop.</returns>
        public static HashSet<Face> GetFaceRingAndLoop(ProBuilderMesh mesh, Face[] faces)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            HashSet<Face> loops = new HashSet<Face>();
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);

            foreach (Face face in faces)
            {
                loops.UnionWith(GetFaceLoop(wings, face, true));
                loops.UnionWith(GetFaceLoop(wings, face, false));
            }

            return loops;
        }

        /// <summary>
        /// Get a face loop or ring from a set of winged edges.
        /// </summary>
        /// <param name="wings"></param>
        /// <param name="face"></param>
        /// <param name="ring"></param>
        /// <returns></returns>
        static HashSet<Face> GetFaceLoop(List<WingedEdge> wings, Face face, bool ring)
        {
            HashSet<Face> loop = new HashSet<Face>();

            if (face == null)
                return loop;

            WingedEdge start = wings.FirstOrDefault(x => x.face == face);

            if (start == null)
                return loop;

            if (ring)
                start = start.next ?? start.previous;

            for (int i = 0; i < 2; i++)
            {
                WingedEdge cur = start;

                if (i == 1)
                {
                    if (start.opposite != null && start.opposite.face != null)
                        cur = start.opposite;
                    else
                        break;
                }

                do
                {
                    if (!loop.Add(cur.face))
                        break;

                    if (cur.Count() != 4)
                        break;

                    // count == 4 assures us next.next is valid, but opposite can still be null
                    cur = cur.next.next.opposite;
                }
                while (cur != null && cur.face != null);
            }

            return loop;
        }

        /// <summary>
        /// Find any holes touching one of the passed vertex indexes.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        internal static List<List<Edge>> FindHoles(ProBuilderMesh mesh, IEnumerable<int> indexes)
        {
            HashSet<int> common = mesh.GetSharedVertexHandles(indexes);
            List<List<Edge>> holes = new List<List<Edge>>();
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);

            foreach (List<WingedEdge> hole in FindHoles(wings, common))
                holes.Add(hole.Select(x => x.edge.local).ToList());

            return holes;
        }

        /// <summary>
        /// Find any holes touching one of the passed common indexes.
        /// </summary>
        /// <param name="wings"></param>
        /// <param name="common"></param>
        /// <returns></returns>
        internal static List<List<WingedEdge>> FindHoles(List<WingedEdge> wings, HashSet<int> common)
        {
            HashSet<WingedEdge> used = new HashSet<WingedEdge>();
            List<List<WingedEdge>> holes = new List<List<WingedEdge>>();

            for (int i = 0; i < wings.Count; i++)
            {
                WingedEdge c = wings[i];

                // if this edge has been added to a hole already, or the edge isn't in the approved list of indexes,
                // or if there's an opposite face, this edge doesn't belong to a hole.  move along.
                if (c.opposite != null || used.Contains(c) || !(common.Contains(c.edge.common.a) || common.Contains(c.edge.common.b)))
                    continue;

                List<WingedEdge> hole = new List<WingedEdge>();
                WingedEdge it = c;
                int ind = it.edge.common.a;

                int counter = 0;

                while (it != null && counter++ < k_MaxHoleIterations)
                {
                    used.Add(it);
                    hole.Add(it);

                    ind = it.edge.common.a == ind ? it.edge.common.b : it.edge.common.a;
                    it = FindNextEdgeInHole(it, ind);

                    if (it == c)
                        break;
                }

                List<SimpleTuple<int, int>> splits = new List<SimpleTuple<int, int>>();

                // check previous wings for y == x (closed loop).
                for (int n = 0; n < hole.Count; n++)
                {
                    WingedEdge wing = hole[n];

                    for (int p = n - 1; p > -1; p--)
                    {
                        if (wing.edge.common.b == hole[p].edge.common.a)
                        {
                            splits.Add(new SimpleTuple<int, int>(p, n));
                            break;
                        }
                    }
                }

                // create new lists from each segment
                // holes paths are nested, with holes
                // possibly split between multiple nested
                // holes
                //
                //  [2, 0]                                     [5, 3]
                //      [0, 9]                                     [3, 11]
                //      [9, 10]                                    [11, 10]
                //              [10, 7]                                    [10, 2]
                //                      [7, 6]             or with split            [2, 0]
                //                      [6, 1]             nesting ->               [0, 9]
                //                      [1, 4]                                      [9, 10]
                //                      [4, 7]  <- (y == x)                [10, 7]
                //              [7, 8]                                      [7, 6]
                //              [8, 5]                                      [6, 1]
                //              [5, 3]                                      [1, 4]
                //              [3, 11]                                     [4, 7]
                //              [11, 10]    <- (y == x)                [7, 8]
                // [10, 2]                      <- (y == x)                [8, 5]
                //
                // paths may also contain multiple segments non-tiered

                int splitCount = splits.Count;

                splits.Sort((x, y) => x.item1.CompareTo(y.item1));

                int[] shift = new int[splitCount];

                // Debug.Log(hole.ToString("\n") + "\n" + splits.ToString("\n"));

                for (int n = splitCount - 1; n > -1; n--)
                {
                    int x = splits[n].item1, y = splits[n].item2 - shift[n];
                    int range = (y - x) + 1;

                    List<WingedEdge> section = hole.GetRange(x, range);

                    hole.RemoveRange(x, range);

                    for (int m = n - 1; m > -1; m--)
                        if (splits[m].item2 > splits[n].item2)
                            shift[m] += range;

                    // verify that this path has at least one index that was asked for
                    if (splitCount < 2 || section.Any(w => common.Contains(w.edge.common.a)) || section.Any(w => common.Contains(w.edge.common.b)))
                        holes.Add(section);
                }
            }

            return holes;
        }

        static WingedEdge FindNextEdgeInHole(WingedEdge wing, int common)
        {
            WingedEdge next = wing.GetAdjacentEdgeWithCommonIndex(common);
            int counter = 0;
            while (next != null && next != wing && counter++ < k_MaxHoleIterations)
            {
                if (next.opposite == null)
                    return next;

                next = next.opposite.GetAdjacentEdgeWithCommonIndex(common);
            }

            return null;
        }
    }
}
