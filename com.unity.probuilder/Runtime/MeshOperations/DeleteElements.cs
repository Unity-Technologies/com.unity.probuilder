using UnityEngine;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Functions for removing vertices and triangles from a mesh.
    /// </summary>
    public static class DeleteElements
    {
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

        [Obsolete("Use MeshValidation.RemoveDegenerateTriangles")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static int[] RemoveDegenerateTriangles(this ProBuilderMesh mesh)
        {
            List<int> removed = new List<int>();
            MeshValidation.RemoveDegenerateTriangles(mesh, removed);
            return removed.ToArray();
        }

        [Obsolete("Use MeshValidation.RemoveUnusedVertices")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static int[] RemoveUnusedVertices(this ProBuilderMesh mesh)
        {
            List<int> removed = new List<int>();
            MeshValidation.RemoveUnusedVertices(mesh, removed);
            return removed.ToArray();
        }
    }
}
