using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Methods for testing that mesh topology is correct.
    /// </summary>
	public static class MeshValidation
	{
	    /// <summary>
	    /// Check if any face on a mesh contains degenerate triangles. A degenerate triangle does not have any area.
	    /// </summary>
	    /// <param name="mesh">The mesh to test for degenerate triangles.</param>
	    /// <returns>True if any face contains a degenerate triangle, false if no degenerate triangles are found.</returns>
        /// <seealso cref="RemoveDegenerateTriangles"/>
        public static bool ContainsDegenerateTriangles(this ProBuilderMesh mesh)
        {
            return ContainsDegenerateTriangles(mesh, mesh.facesInternal);
        }

        /// <summary>
        /// Check if any face contains degenerate triangles. A degenerate triangle does not have any area.
        /// </summary>
        /// <param name="mesh">The mesh to test for degenerate triangles.</param>
        /// <param name="faces">The faces to test for degenerate triangles.</param>
        /// <returns>True if any face contains a degenerate triangle, false if no degenerate triangles are found.</returns>
        /// <seealso cref="RemoveDegenerateTriangles"/>
        public static bool ContainsDegenerateTriangles(this ProBuilderMesh mesh, IList<Face> faces)
        {
            var positions = mesh.positionsInternal;

            foreach (var face in faces)
            {
                var indices = face.indexesInternal;

                for (int i = 0; i < indices.Length; i += 3)
                {
                    float area = Math.TriangleArea(
                        positions[indices[i + 0]],
                        positions[indices[i + 1]],
                        positions[indices[i + 2]]);

                    if (area <= Mathf.Epsilon)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if any face contains degenerate triangles. A degenerate triangle does not have any area.
        /// </summary>
        /// <param name="mesh">The mesh to test for degenerate triangles.</param>
        /// <param name="face">The face to test for degenerate triangles.</param>
        /// <returns>True if any triangle within the face contains a degenerate triangle, false if no degenerate triangles are found.</returns>
        /// <seealso cref="RemoveDegenerateTriangles"/>
        public static bool ContainsDegenerateTriangles(this ProBuilderMesh mesh, Face face)
        {
            var positions = mesh.positionsInternal;
            var indices = face.indexesInternal;

            for (int i = 0; i < indices.Length; i += 3)
            {
                float area = Math.TriangleArea(
                    positions[indices[i + 0]],
                    positions[indices[i + 1]],
                    positions[indices[i + 2]]);

                if (area <= Mathf.Epsilon)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Iterates through all faces in a mesh and removes triangles with an area less than float.Epsilon, or with
        /// indexes that point to the same vertex. This function also enforces the rule that a face must contain no
        /// coincident vertices.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="removed">An optional list to be populated with the removed indices. If no degenerate triangles are found, this list will contain no elements.</param>
        /// <returns>True if degenerate triangles were found and removed, false if no degenerate triangles were found.</returns>
        public static bool RemoveDegenerateTriangles(ProBuilderMesh mesh, List<int> removed = null)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Dictionary<int, int> m_Lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> m_LookupUV = mesh.sharedTextureLookup;
            Vector3[] m_Positions = mesh.positionsInternal;
            Dictionary<int, int> m_RebuiltLookup = new Dictionary<int, int>(m_Lookup.Count);
            Dictionary<int, int> m_RebuiltLookupUV = new Dictionary<int, int>(m_LookupUV.Count);
            List<Face> m_RebuiltFaces = new List<Face>(mesh.faceCount);
            Dictionary<int, int> m_DuplicateIndexFilter = new Dictionary<int, int>(8);

            foreach (Face face in mesh.facesInternal)
            {
                m_DuplicateIndexFilter.Clear();
                List<int> tris = new List<int>();
                int[] ind = face.indexesInternal;

                for (int i = 0; i < ind.Length; i += 3)
                {
                    float area = Math.TriangleArea(m_Positions[ind[i + 0]], m_Positions[ind[i + 1]], m_Positions[ind[i + 2]]);

                    if (area > Mathf.Epsilon)
                    {
                        // Index in the positions array
                        int triangleIndexA = ind[i],
                            triangleIndexB = ind[i+1],
                            triangleIndexC = ind[i+2];

                        // Common index (also called SharedIndexHandle)
                        int sharedIndexA = m_Lookup[triangleIndexA],
                            sharedIndexB = m_Lookup[triangleIndexB],
                            sharedIndexC = m_Lookup[triangleIndexC];

                        // test if there are any duplicates in the triangle
                        if (!(sharedIndexA == sharedIndexB || sharedIndexA == sharedIndexC || sharedIndexB == sharedIndexC))
                        {
                            int index;

                            // catch case where face has two distinct vertices that are in fact coincident.
                            if (!m_DuplicateIndexFilter.TryGetValue(sharedIndexA, out index))
                                m_DuplicateIndexFilter.Add(sharedIndexA, triangleIndexA);
                            else
                                triangleIndexA = index;

                            if (!m_DuplicateIndexFilter.TryGetValue(sharedIndexB, out index))
                                m_DuplicateIndexFilter.Add(sharedIndexB, triangleIndexB);
                            else
                                triangleIndexB = index;

                            if (!m_DuplicateIndexFilter.TryGetValue(sharedIndexC, out index))
                                m_DuplicateIndexFilter.Add(sharedIndexC, triangleIndexC);
                            else
                                triangleIndexC = index;

                            tris.Add(triangleIndexA);
                            tris.Add(triangleIndexB);
                            tris.Add(triangleIndexC);

                            if (!m_RebuiltLookup.ContainsKey(triangleIndexA))
                                m_RebuiltLookup.Add(triangleIndexA, sharedIndexA);
                            if (!m_RebuiltLookup.ContainsKey(triangleIndexB))
                                m_RebuiltLookup.Add(triangleIndexB, sharedIndexB);
                            if (!m_RebuiltLookup.ContainsKey(triangleIndexC))
                                m_RebuiltLookup.Add(triangleIndexC, sharedIndexC);

                            if (m_LookupUV.ContainsKey(triangleIndexA) && !m_RebuiltLookupUV.ContainsKey(triangleIndexA))
                                m_RebuiltLookupUV.Add(triangleIndexA, m_LookupUV[triangleIndexA]);
                            if (m_LookupUV.ContainsKey(triangleIndexB) && !m_RebuiltLookupUV.ContainsKey(triangleIndexB))
                                m_RebuiltLookupUV.Add(triangleIndexB, m_LookupUV[triangleIndexB]);
                            if (m_LookupUV.ContainsKey(triangleIndexC) && !m_RebuiltLookupUV.ContainsKey(triangleIndexC))
                                m_RebuiltLookupUV.Add(triangleIndexC, m_LookupUV[triangleIndexC]);
                        }
                    }
                }

                if (tris.Count > 0)
                {
                    face.indexesInternal = tris.ToArray();
                    m_RebuiltFaces.Add(face);
                }
            }

            mesh.faces = m_RebuiltFaces;
            mesh.SetSharedVertices(m_RebuiltLookup);
            mesh.SetSharedTextures(m_RebuiltLookupUV);

            return RemoveUnusedVertices(mesh, removed);
        }

        /// <summary>
        /// Removes vertices that no face references.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="removed">An optional list to be populated with the removed indices. If no vertices are removed, this list will contain no elements.</param>
        /// <returns>A list of deleted vertex indexes.</returns>
        public static bool RemoveUnusedVertices(ProBuilderMesh mesh, List<int> removed = null)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            bool saveRemoved = removed != null;

            if(saveRemoved)
                removed.Clear();

            var del = saveRemoved ? removed : new List<int>();

            var tris = new HashSet<int>(mesh.facesInternal.SelectMany(x => x.indexes));

            for (int i = 0; i < mesh.positionsInternal.Length; i++)
                if (!tris.Contains(i))
                    del.Add(i);

            mesh.DeleteVertices(del);

            return del.Any();
        }

        /// <summary>
        /// Rebuild a collection of indexes accounting for the removal of a collection of indices.
        /// </summary>
        /// <param name="indices">The indices to rebuild.</param>
        /// <param name="removed">A sorted collection indices that were removed.</param>
        /// <returns>A new list of indices pointing to the same vertex as they were prior to the removal of some entries.</returns>
        internal static List<int> RebuildIndexes(IEnumerable<int> indices, List<int> removed)
        {
            var res = new List<int>();
            var rmc = removed.Count;

            foreach (var index in indices)
            {
                var nearestIndex = ArrayUtility.NearestIndexPriorToValue(removed, index) + 1;

                // don't add back into the indices collection if the index was removed
                if (nearestIndex > -1 && nearestIndex < rmc && removed[nearestIndex] == index)
                    continue;

                res.Add(index - nearestIndex);
            }

            return res;
        }

        /// <summary>
        /// Rebuild a collection of indexes accounting for the removal of a collection of indices.
        /// </summary>
        /// <param name="edges">The indices to rebuild.</param>
        /// <param name="removed">A sorted collection indices that were removed.</param>
        /// <returns>A new list of indices pointing to the same vertex as they were prior to the removal of some entries.</returns>
        internal static List<Edge> RebuildEdges(IEnumerable<Edge> edges, List<int> removed)
        {
            var res = new List<Edge>();
            var rmc = removed.Count;

            foreach (var edge in edges)
            {
                var nearestIndexA = ArrayUtility.NearestIndexPriorToValue(removed, edge.a) + 1;
                var nearestIndexB = ArrayUtility.NearestIndexPriorToValue(removed, edge.b) + 1;

                // don't add back into the indices collection if the index was removed
                if ((nearestIndexA > -1 && nearestIndexA < rmc && removed[nearestIndexA] == edge.a) ||
                    (nearestIndexB > -1 && nearestIndexB < rmc && removed[nearestIndexB] == edge.b))
                    continue;

                res.Add(new Edge(edge.a - nearestIndexA, edge.b - nearestIndexB));
            }

            return res;
        }

        internal static void RebuildSelectionIndexes(ProBuilderMesh mesh, ref Face[] faces, ref Edge[] edges, ref int[] indices, IEnumerable<int> removed)
        {
            var rm = removed.ToList();
            rm.Sort();

            if (faces != null && faces.Length > 0)
                faces = faces.Where(x => mesh.facesInternal.Contains(x)).ToArray();

            if(edges != null && edges.Length > 0)
                edges = RebuildEdges(edges, rm).ToArray();

            if(indices != null && indices.Length > 0)
                indices = RebuildIndexes(indices, rm).ToArray();
        }

        /// <summary>
        /// Remove any degenerate triangles on a mesh, keeping the selection intact.
        /// </summary>
        /// <returns>
        /// 0 if mesh is valid, or greater than 0 if corrections were made. If this method returns greater than 0, you will need to
        /// rebuild the mesh using <see cref="ProBuilderMesh.ToMesh"/> and <see cref="ProBuilderMesh.Refresh"/>.
        /// </returns>
        internal static int EnsureMeshIsValid(ProBuilderMesh mesh)
        {
            if (ContainsDegenerateTriangles(mesh))
            {
                var faces = mesh.selectedFacesInternal;
                var edges = mesh.selectedEdgesInternal;
                var indices = mesh.selectedIndexesInternal;

                List<int> removed = new List<int>();

                if (RemoveDegenerateTriangles(mesh, removed))
                {
                    mesh.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(mesh.positionsInternal);
                    
                    RebuildSelectionIndexes(mesh, ref faces, ref edges, ref indices, removed);
                    mesh.selectedFacesInternal = faces;
                    mesh.selectedEdgesInternal = edges;
                    mesh.selectedIndexesInternal = indices;
                    return removed.Count;
                }
            }

            return 0;
        }
	}
}
