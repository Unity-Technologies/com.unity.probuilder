using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A winged-edge data structure holds references to an edge, the previous and next edge in it's triangle, it's connected face, and the opposite edge (common).
    /// </summary>
    /// <example>
    ///  ```
    /// .       /   (face)    /
    /// . prev /             / next
    /// .     /    edge     /
    /// .    /_ _ _ _ _ _ _/
    /// .    |- - - - - - -|
    /// .    |  opposite   |
    /// .    |             |
    /// .    |             |
    /// .    |             |
    /// ```
    /// </example>
    /// <inheritdoc cref="IEquatable{T}" />
    /// <inheritdoc cref="IEnumerable{T}" />
    public sealed class WingedEdge : IEquatable<WingedEdge>
    {
        /// <value>
        /// The local and shared edge that this edge belongs to.
        /// </value>
        public EdgeLookup edge { get; private set; }

        /// <value>
        /// The connected face that this wing belongs to.
        /// </value>
        public Face face { get; private set; }

        /// <value>
        /// The WingedEdge that is connected to the edge.y vertex.
        /// </value>
        public WingedEdge next { get; private set; }

        /// <value>
        /// The WingedEdge that is connected to the edge.x vertex.
        /// </value>
        public WingedEdge previous { get; private set; }

        /// <value>
        /// The WingedEdge that is on the opposite side of this edge.
        /// </value>
        public WingedEdge opposite { get; private set; }

        WingedEdge() {}

        /// <summary>
        /// Equality comparision tests for local edge equality, disregarding other values.
        /// </summary>
        /// <param name="other">The WingedEdge to compare against.</param>
        /// <returns>True if the local edges are equal, false if not.</returns>
        public bool Equals(WingedEdge other)
        {
            return !ReferenceEquals(other, null) && edge.local.Equals(other.edge.local);
        }

        /// <summary>
        /// Equality comparision tests for local edge equality, disregarding other values.
        /// </summary>
        /// <param name="obj">The WingedEdge to compare against.</param>
        /// <returns>True if the local edges are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            WingedEdge be = obj as WingedEdge;

            if (be != null && this.Equals(be))
                return true;

            if (obj is Edge && this.edge.local.Equals((Edge)obj))
                return true;

            return false;
        }

        /// <summary>
        /// Get a hash code for this edge.
        /// </summary>
        /// <returns>WingedEdge comparison only considers the local edge. As such, this returns the local edge hashcode.</returns>
        public override int GetHashCode()
        {
            return edge.local.GetHashCode();
        }

        /// <summary>
        /// How many edges are in this sequence.
        /// </summary>
        /// <returns>The number of WingedEdges that are connected by walking the @"UnityEngine.ProBuilder.WingedEdge.next" property.</returns>
        public int Count()
        {
            WingedEdge current = this;
            int count = 0;

            do
            {
                count++;
                current = current.next;
            }
            while (current != null && !ReferenceEquals(current, this));

            return count;
        }

        /// <returns>A string representation of the properties contained within this object.</returns>
        public override string ToString()
        {
            return string.Format("Common: {0}\nLocal: {1}\nOpposite: {2}\nFace: {3}",
                edge.common.ToString(),
                edge.local.ToString(),
                opposite == null ? "null" : opposite.edge.ToString(),
                face.ToString());
        }

        /// <summary>
        /// Given two adjacent triangle wings, attempt to create a single quad.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        internal static int[] MakeQuad(WingedEdge left, WingedEdge right)
        {
            // Both faces must be triangles in order to be considered a quad when combined
            if (left.Count() != 3 || right.Count() != 3)
                return null;

            EdgeLookup[] all = new EdgeLookup[6]
            {
                left.edge,
                left.next.edge,
                left.next.next.edge,
                right.edge,
                right.next.edge,
                right.next.next.edge
            };

            int[] dup = new int[6];
            int matches = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int n = 3; n < 6; n++)
                {
                    if (all[i].Equals(all[n]))
                    {
                        matches++;
                        dup[i] = 1;
                        dup[n] = 1;
                        break;
                    }
                }
            }

            // Edges are either not adjacent, or share more than one edge
            if (matches != 1)
                return null;

            int qi = 0;

            EdgeLookup[] edges = new EdgeLookup[4];

            for (int i = 0; i < 6; i++)
                if (dup[i] < 1)
                    edges[qi++] = all[i];

            int[] quad = new int[4] { edges[0].local.a, edges[0].local.b, -1, -1 };

            int c1 = edges[0].common.b, c2 = -1;

            if (edges[1].common.a == c1)
            {
                quad[2] = edges[1].local.b;
                c2 = edges[1].common.b;
            }
            else if (edges[2].common.a == c1)
            {
                quad[2] = edges[2].local.b;
                c2 = edges[2].common.b;
            }
            else if (edges[3].common.a == c1)
            {
                quad[2] = edges[3].local.b;
                c2 = edges[3].common.b;
            }

            if (edges[1].common.a == c2)
                quad[3] = edges[1].local.b;
            else if (edges[2].common.a == c2)
                quad[3] = edges[2].local.b;
            else if (edges[3].common.a == c2)
                quad[3] = edges[3].local.b;

            if (quad[2] == -1 || quad[3] == -1)
                return null;

            return quad;
        }

        /// <summary>
        /// Return the @"UnityEngine.ProBuilder.WingedEdge.previous" or @"UnityEngine.ProBuilder.WingedEdge.next" WingedEdge if it contains the passed common (shared) index.
        /// </summary>
        /// <param name="common">The common index to search next and previous for.</param>
        /// <returns>The next or previous WingedEdge that contains common, or null if not found.</returns>
        public WingedEdge GetAdjacentEdgeWithCommonIndex(int common)
        {
            if (next.edge.common.Contains(common))
                return next;
            else if (previous.edge.common.Contains(common))
                return previous;

            return null;
        }

        /// <summary>
        /// Order a face's edges in sequence.
        /// The first edge is used as a starting point.
        /// </summary>
        /// <param name="face">The source face.</param>
        /// <returns>A new set of edges where each edge y value matches the next edge x.</returns>
        public static List<Edge> SortEdgesByAdjacency(Face face)
        {
            if (face == null || face.edgesInternal == null)
                throw new ArgumentNullException("face");
            List<Edge> edges = new List<Edge>(face.edgesInternal);
            SortEdgesByAdjacency(edges);
            return edges;
        }

        /// <summary>
        /// Sort edges list by adjacency, such that each edge's common y value matches the next edge's common x.
        /// </summary>
        /// <param name="edges">The edges to sort in-place.</param>
        public static void SortEdgesByAdjacency(List<Edge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException("edges");

            for (int i = 1; i < edges.Count; i++)
            {
                int want = edges[i - 1].b;

                for (int n = i + 1; n < edges.Count; n++)
                {
                    if (edges[n].a == want || edges[n].b == want)
                    {
                        Edge swap = edges[n];
                        edges[n] = edges[i];
                        edges[i] = swap;
                    }
                }
            }
        }

        /// <summary>
        /// Get a dictionary of common indexes and all WingedEdge values touching the index.
        /// </summary>
        /// <param name="wings">The wings to search for spokes.</param>
        /// <returns>A dictionary where each key is a common index with a list of each winged edge touching it.</returns>
        public static Dictionary<int, List<WingedEdge>> GetSpokes(List<WingedEdge> wings)
        {
            if (wings == null)
                throw new ArgumentNullException("wings");

            Dictionary<int, List<WingedEdge>> spokes = new Dictionary<int, List<WingedEdge>>();
            List<WingedEdge> l = null;

            for (int i = 0; i < wings.Count; i++)
            {
                if (spokes.TryGetValue(wings[i].edge.common.a, out l))
                    l.Add(wings[i]);
                else
                    spokes.Add(wings[i].edge.common.a, new List<WingedEdge>() { wings[i] });

                if (spokes.TryGetValue(wings[i].edge.common.b, out l))
                    l.Add(wings[i]);
                else
                    spokes.Add(wings[i].edge.common.b, new List<WingedEdge>() { wings[i] });
            }

            return spokes;
        }

        /// <summary>
        /// Given a set of winged edges and list of common indexes, attempt to create a complete path of indexes where each is connected by edge.
        /// <br />
        /// May be clockwise or counter-clockwise ordered, or null if no path is found.
        /// </summary>
        /// <param name="wings">The wings to be sorted.</param>
        /// <param name="common">The common indexes to be sorted.</param>
        /// <returns></returns>
        internal static List<int> SortCommonIndexesByAdjacency(List<WingedEdge> wings, HashSet<int> common)
        {
            List<Edge> matches = wings.Where(x => common.Contains(x.edge.common.a) && common.Contains(x.edge.common.b)).Select(y => y.edge.common).ToList();

            // if edge count != index count there isn't a full perimeter
            if (matches.Count != common.Count)
                return null;

            SortEdgesByAdjacency(matches);
            return matches.Select(x => x.a).ToList();
        }

        /// <summary>
        /// Create a new list of WingedEdge values for a ProBuilder mesh.
        /// </summary>
        /// <param name="mesh">The mesh from which faces will read.</param>
        /// <param name="oneWingPerFace">Optionally restrict the list to only include one WingedEdge per-face.</param>
        /// <returns>A new list of WingedEdge values gathered from @"UnityEngine.ProBuilder.ProBuilderMesh.faces".</returns>
        public static List<WingedEdge> GetWingedEdges(ProBuilderMesh mesh, bool oneWingPerFace = false)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            return GetWingedEdges(mesh, mesh.facesInternal, oneWingPerFace);
        }

        /// <summary>
        /// Create a new list of WingedEdge values for a ProBuilder mesh.
        /// </summary>
        /// <param name="mesh">Target ProBuilderMesh.</param>
        /// <param name="faces">Which faces to include in the WingedEdge list.</param>
        /// <param name="oneWingPerFace">If `oneWingPerFace` is true the returned list will contain a single winged edge per-face (but still point to all edges).</param>
        /// <returns>A new list of WingedEdge values gathered from faces.</returns>
        public static List<WingedEdge> GetWingedEdges(ProBuilderMesh mesh, IEnumerable<Face> faces, bool oneWingPerFace = false)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            var lookup = mesh.sharedVertexLookup;

            List<WingedEdge> winged = new List<WingedEdge>();
            Dictionary<Edge, WingedEdge> opposites = new Dictionary<Edge, WingedEdge>();

            foreach (Face f in faces)
            {
                List<Edge> edges = SortEdgesByAdjacency(f);
                int edgeLength = edges.Count;
                WingedEdge first = null, prev = null;

                for (int n = 0; n < edgeLength; n++)
                {
                    Edge e = edges[n];

                    WingedEdge w = new WingedEdge();
                    w.edge = new EdgeLookup(lookup[e.a], lookup[e.b], e.a, e.b);
                    w.face = f;
                    if (n < 1)
                        first = w;

                    if (n > 0)
                    {
                        w.previous = prev;
                        prev.next = w;
                    }

                    if (n == edgeLength - 1)
                    {
                        w.next = first;
                        first.previous = w;
                    }

                    prev = w;

                    WingedEdge opp;

                    if (opposites.TryGetValue(w.edge.common, out opp))
                    {
                        opp.opposite = w;
                        w.opposite = opp;
                    }
                    else
                    {
                        w.opposite = null;
                        opposites.Add(w.edge.common, w);
                    }

                    if (!oneWingPerFace || n < 1)
                        winged.Add(w);
                }
            }

            return winged;
        }
    }
}
