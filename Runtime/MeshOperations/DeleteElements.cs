using UnityEngine;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Provides functions for removing vertices and triangles from a mesh.
    /// </summary>
    public static class DeleteElements
    {
        /// <summary>
        /// Deletes the vertices from the specified index array and rebuilds the <see cref="ProBuilderMesh.sharedVertices"/> array.
        /// </summary>
        /// <remarks>This function does not retriangulate the mesh. This means that you are responsible for ensuring that the indexes
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

        public static int[] DeleteVerticesAndConnectedFaces(this ProBuilderMesh mesh, IEnumerable<int> coincidentVertices)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            if (coincidentVertices == null)
                throw new ArgumentNullException(nameof(coincidentVertices));

            var requested = coincidentVertices.Distinct().ToList();
            if (requested.Count == 0)
                return new int[0];

            requested.Sort();

            // Find faces that reference any of the requested vertex indices
            var requestedSet = new HashSet<int>(requested);
            var facesToRemove = new List<int>();

            for (int i = 0; i < mesh.facesInternal.Length; i++)
            {
                var face = mesh.facesInternal[i];
                foreach (var idx in face.distinctIndexesInternal)
                {
                    if (requestedSet.Contains(idx))
                    {
                        facesToRemove.Add(i);
                        break;
                    }
                }
            }

            // Remove faces first (this will update vertices and shared structures)
            int[] removedByFace = new int[0];
            if (facesToRemove.Count > 0)
            {
                removedByFace = mesh.DeleteFaces(facesToRemove);
            }

            Array.Sort(removedByFace);
            var removedByFaceSet = new HashSet<int>(removedByFace);

            // Determine which originally requested indices were NOT already removed by face deletion
            var remaining = requested.Where(x => !removedByFaceSet.Contains(x)).ToList();

            // If there are still vertices to remove, adjust their indices to account for vertices already removed,
            // then call the low-level DeleteVertices which expects indexes that are not referenced by triangles.
            if (remaining.Count > 0)
            {
                // removedByFace is sorted; for each original index, compute how many removed indices are < index
                var adjusted = new List<int>(remaining.Count);
                for (int i = 0; i < remaining.Count; i++)
                {
                    int orig = remaining[i];
                    int shift = removedByFace.Length > 0 ? ArrayUtility.NearestIndexPriorToValue(removedByFace, orig) + 1 : 0;
                    adjusted.Add(orig - shift);
                }

                // Call the low-level delete (assumes no triangles reference these adjusted indices)
                mesh.DeleteVertices(adjusted);
            }

            // Return the list of original indices removed (both from faces and remaining vertex deletions)
            var result = new List<int>(removedByFace);
            result.AddRange(remaining);
            result.Sort();
            return result.ToArray();
        }

        /// <summary>
        /// Removes a face from a mesh.
        ///
        /// This is the equivalent of the [Delete Faces](../manual/Face_Delete.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to remove.</param>
        /// <returns>An array of vertex indices that ProBuilder deleted when it removed the specified face.</returns>
        public static int[] DeleteFace(this ProBuilderMesh mesh, Face face)
        {
            return DeleteFaces(mesh, new Face[] { face });
        }

        /// <summary>
        /// Deletes a collection of faces from a mesh.
        ///
        /// This is the equivalent of the [Delete Faces](../manual/Face_Delete.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to remove.</param>
        /// <returns>An array of vertex indices that ProBuilder deleted when it removed the specified faces.</returns>
        public static int[] DeleteFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            return DeleteFaces(mesh, faces.Select(x => System.Array.IndexOf(mesh.facesInternal, x)).ToList());
        }

        /// <summary>
        /// Deletes a list of faces from a mesh.
        ///
        /// This is the equivalent of the [Delete Faces](../manual/Face_Delete.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faceIndexes">The indices of faces to remove (corresponding to the <see cref="ProBuilderMesh.faces"/> collection.</param>
        /// <returns>An array of vertex indices that ProBuilder deleted when it removed the specified faces.</returns>
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

        /// <summary>Obsolete. Use `MeshValidation.RemoveDegenerateTriangles` instead.</summary>
        /// <param name="mesh">The source mesh.</param>
        /// <returns>The list of removed triangles</returns>
        [Obsolete("Use MeshValidation.RemoveDegenerateTriangles")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static int[] RemoveDegenerateTriangles(this ProBuilderMesh mesh)
        {
            List<int> removed = new List<int>();
            MeshValidation.RemoveDegenerateTriangles(mesh, removed);
            return removed.ToArray();
        }

        /// <summary>Obsolete. Use `MeshValidation.RemoveUnusedVertices` instead.</summary>
        /// <param name="mesh">The source mesh.</param>
        /// <returns>The list of removed vertices</returns>
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
