using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Functions for removing vertices and triangles from a mesh.
    /// </summary>
    public static class DeleteElements
    {
        /// <summary>
        /// Removes vertices that no face references.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <returns>A list of deleted vertex indexes.</returns>
        public static int[] RemoveUnusedVertices(this ProBuilderMesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            List<int> del = new List<int>();
            HashSet<int> tris = new HashSet<int>(mesh.facesInternal.SelectMany(x => x.indexes));

            for (int i = 0; i < mesh.positionsInternal.Length; i++)
                if (!tris.Contains(i))
                    del.Add(i);

            mesh.DeleteVertices(del);

            return del.ToArray();
        }

        /// <summary>
        /// Deletes the vertices from the passed index array, and handles rebuilding the sharedIndexes array.
        /// </summary>
        /// <remarks>This function does not retriangulate the mesh. Ie, you are responsible for ensuring that indexes
        /// deleted by this function are not referenced by any triangles.</remarks>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="distinctIndexes">A list of vertices to delete. Note that this must not contain duplicates.</param>
        public static void DeleteVertices(this ProBuilderMesh mesh, IEnumerable<int> distinctIndexes)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (distinctIndexes == null || !distinctIndexes.Any())
                return;

            Vertex[] vertices = mesh.GetVertices();
            int originalVertexCount = vertices.Length;
            int[] offset = new int[originalVertexCount];

            List<int> sorted = new List<int>(distinctIndexes);

            sorted.Sort();

            vertices = vertices.SortedRemoveAt(sorted);

            // Add 1 because NearestIndexPriorToValue is 0 indexed.
            for (int i = 0; i < originalVertexCount; i++)
                offset[i] = ArrayUtility.NearestIndexPriorToValue(sorted, i) + 1;

            foreach (Face face in mesh.facesInternal)
            {
                int[] indexes = face.indexesInternal;

                for (int i = 0; i < indexes.Length; i++)
                    indexes[i] -= offset[indexes[i]];

                face.InvalidateCache();
            }

            // remove from sharedIndexes & shift to account for deletions
            var common = mesh.sharedVertexLookup.Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));
            var commonUV = mesh.sharedTextureLookup.Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));

            mesh.SetVertices(vertices);
            mesh.SetSharedVertices(common);
            mesh.SetSharedTextures(commonUV);
        }

        /// <summary>
        /// Removes a face from a mesh.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to remove.</param>
        /// <returns>An array of vertex indexes that were deleted as a result of face deletion.</returns>
        public static int[] DeleteFace(this ProBuilderMesh mesh, Face face)
        {
            return DeleteFaces(mesh, new Face[] { face });
        }

        /// <summary>
        /// Delete a collection of faces from a mesh.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to remove.</param>
        /// <returns>An array of vertex indexes that were deleted as a result of deletion.</returns>
        public static int[] DeleteFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            return DeleteFaces(mesh, faces.Select(x => System.Array.IndexOf(mesh.facesInternal, x)).ToList());
        }

        /// <summary>
        /// Delete a collection of faces from a mesh.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faceIndexes">The indexes of faces to remove (corresponding to the @"UnityEngine.ProBuilder.ProBuilderMesh.faces" collection.</param>
        /// <returns>An array of vertex indexes that were deleted as a result of deletion.</returns>
        public static int[] DeleteFaces(this ProBuilderMesh mesh, IList<int> faceIndexes)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faceIndexes == null)
                throw new ArgumentNullException("faceIndexes");

            Face[] faces = new Face[faceIndexes.Count];

            for (int i = 0; i < faces.Length; i++)
                faces[i] = mesh.facesInternal[faceIndexes[i]];

            List<int> indexesToRemove = faces.SelectMany(x => x.distinctIndexesInternal).Distinct().ToList();
            indexesToRemove.Sort();

            int vertexCount = mesh.positionsInternal.Length;

            Face[] nFaces = mesh.facesInternal.RemoveAt(faceIndexes);
            var vertices = mesh.GetVertices().SortedRemoveAt(indexesToRemove);

            Dictionary<int, int> shiftmap = new Dictionary<int, int>();

            for (var i = 0; i < vertexCount; i++)
                shiftmap.Add(i, ArrayUtility.NearestIndexPriorToValue<int>(indexesToRemove, i) + 1);

            // shift all other face indexes down to account for moved vertex positions
            for (var i = 0; i < nFaces.Length; i++)
            {
                int[] tris = nFaces[i].indexesInternal;

                for (var n = 0; n < tris.Length; n++)
                    tris[n] -= shiftmap[tris[n]];

                nFaces[i].indexesInternal = tris;
            }

            mesh.SetVertices(vertices);
            mesh.sharedVerticesInternal = SharedVertex.SortedRemoveAndShift(mesh.sharedVertexLookup, indexesToRemove);
            mesh.sharedTextures = SharedVertex.SortedRemoveAndShift(mesh.sharedTextureLookup, indexesToRemove);
            mesh.facesInternal = nFaces;
            int[] array = indexesToRemove.ToArray();

            return array;
        }

        /// <summary>
        /// Iterates through all faces in a mesh and removes triangles with an area less than float.Epsilon, or with
        /// indexes that point to the same vertex. This function also enforces the rule that a face must contain no
        /// coincident vertices.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <returns>The number of vertices deleted as a result of the degenerate triangle cleanup.</returns>
        public static int[] RemoveDegenerateTriangles(this ProBuilderMesh mesh)
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
            return mesh.RemoveUnusedVertices();
        }
    }
}
