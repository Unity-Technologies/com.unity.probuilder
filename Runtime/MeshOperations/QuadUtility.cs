
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Provides a helper function to manage converting triangulated polygons to [quads](../manual/gloss.html#quad).
    /// </summary>
    public static class QuadUtility
    {
        /// <summary>
        /// Converts the faces to quads if possible.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The list of faces to process.</param>
        /// <param name="smoothing">True to apply smoothing.</param>
        /// <returns>A list of the processed faces.</returns>
        public static List<Face> ToQuads(this ProBuilderMesh mesh, IList<Face> faces, bool smoothing = true)
        {
            HashSet<Face> processed = new HashSet<Face>();

            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, faces, true);

            // build a lookup of the strength of edge connections between triangle faces
            Dictionary<EdgeLookup, float> connections = new Dictionary<EdgeLookup, float>();

            for (int i = 0; i < wings.Count; i++)
            {
                using (var it = new WingedEdgeEnumerator(wings[i]))
                {
                    while (it.MoveNext())
                    {
                        var border = it.Current;

                        if (border.opposite != null && !connections.ContainsKey(border.edge))
                        {
                            float score = mesh.GetQuadScore(border, border.opposite);
                            connections.Add(border.edge, score);
                        }
                    }
                }
            }

            List<SimpleTuple<Face, Face>> quads = new List<SimpleTuple<Face, Face>>();

            // move through each face and find it's best quad neighbor
            foreach (WingedEdge face in wings)
            {
                if (!processed.Add(face.face))
                    continue;

                float bestScore = 0f;
                Face buddy = null;

                using (var it = new WingedEdgeEnumerator(face))
                {
                    while (it.MoveNext())
                    {
                        var border = it.Current;

                        if (border.opposite != null && processed.Contains(border.opposite.face))
                            continue;

                        float borderScore;

                        // only add it if the opposite face's best score is also this face
                        if (connections.TryGetValue(border.edge, out borderScore) &&
                            borderScore > bestScore &&
                            face.face == GetBestQuadConnection(border.opposite, connections))
                        {
                            bestScore = borderScore;
                            buddy = border.opposite.face;
                        }
                    }
                }

                if (buddy != null)
                {
                    processed.Add(buddy);
                    quads.Add(new SimpleTuple<Face, Face>(face.face, buddy));
                }
            }

            // don't collapse coincident vertices if smoothing is enabled, we need the original normals intact
            return MergeElements.MergePairs(mesh, quads, smoothing);
        }

        static Face GetBestQuadConnection(WingedEdge wing, Dictionary<EdgeLookup, float> connections)
        {
            float score = 0f;
            Face face = null;

            using (var it = new WingedEdgeEnumerator(wing))
            {
                while (it.MoveNext())
                {
                    var border = it.Current;

                    float s = 0f;

                    if (connections.TryGetValue(border.edge, out s) && s > score)
                    {
                        score = connections[border.edge];
                        face = border.opposite.face;
                    }
                }
            }

            return face;
        }

        /**
         * Get a weighted value for the quality of a quad composed of two triangles. 0 is terrible, 1 is perfect.
         * normalThreshold will discard any quads where the dot product of their normals is less than the threshold.
         * @todo Abstract the quad detection to a separate class so it can be applied to pb_Objects.
         */
        static float GetQuadScore(this ProBuilderMesh mesh, WingedEdge left, WingedEdge right, float normalThreshold = .9f)
        {
            Vertex[] vertices = mesh.GetVertices();

            int[] quad = WingedEdge.MakeQuad(left, right);

            if (quad == null)
                return 0f;

            // first check normals
            Vector3 leftNormal = Math.Normal(vertices[quad[0]].position, vertices[quad[1]].position, vertices[quad[2]].position);
            Vector3 rightNormal = Math.Normal(vertices[quad[2]].position, vertices[quad[3]].position, vertices[quad[0]].position);

            float score = Vector3.Dot(leftNormal, rightNormal);

            if (score < normalThreshold)
                return 0f;

            // next is right-angle-ness check
            Vector3 a = (vertices[quad[1]].position - vertices[quad[0]].position);
            Vector3 b = (vertices[quad[2]].position - vertices[quad[1]].position);
            Vector3 c = (vertices[quad[3]].position - vertices[quad[2]].position);
            Vector3 d = (vertices[quad[0]].position - vertices[quad[3]].position);

            a.Normalize();
            b.Normalize();
            c.Normalize();
            d.Normalize();

            float da = Mathf.Abs(Vector3.Dot(a, b));
            float db = Mathf.Abs(Vector3.Dot(b, c));
            float dc = Mathf.Abs(Vector3.Dot(c, d));
            float dd = Mathf.Abs(Vector3.Dot(d, a));

            score += 1f - ((da + db + dc + dd) * .25f);

            // and how close to parallel the opposite sides area
            score += Mathf.Abs(Vector3.Dot(a, c)) * .5f;
            score += Mathf.Abs(Vector3.Dot(b, d)) * .5f;

            // the three tests each contribute 1
            return score * .33f;
        }
    }
}
