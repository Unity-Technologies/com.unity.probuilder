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
		/// <param name="mesh"></param>
		/// <returns>A list of deleted vertex indices.</returns>
		public static int[] RemoveUnusedVertices(this ProBuilderMesh mesh)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			List<int> del = new List<int>();
			HashSet<int> tris = new HashSet<int>(mesh.facesInternal.SelectMany(x => x.ToTriangles()));

			for(int i = 0; i < mesh.positionsInternal.Length; i++)
				if(!tris.Contains(i))
					del.Add(i);

			mesh.DeleteVertices(del);

			return del.ToArray();
		}

		/// <summary>
		/// Deletes the vertices from the passed index array, and handles rebuilding the sharedIndices array.
		/// </summary>
		/// <remarks>This function does not retriangulate the mesh. Ie, you are responsible for ensuring that indices
		/// deleted by this function are not referenced by any triangles.</remarks>
		/// <param name="mesh"></param>
		/// <param name="distinctIndexes"></param>
		public static void DeleteVertices(this ProBuilderMesh mesh, IEnumerable<int> distinctIndexes)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (distinctIndexes == null || !distinctIndexes.Any())
				return;

			Vertex[] vertices = Vertex.GetVertices(mesh);
			int originalVertexCount = vertices.Length;
			int[] offset = new int[originalVertexCount];

			List<int> sorted = new List<int>(distinctIndexes);

			sorted.Sort();

			vertices = vertices.SortedRemoveAt(sorted);

			// Add 1 because NearestIndexPriorToValue is 0 indexed.
			for(int i = 0; i < originalVertexCount; i++)
				offset[i] = ArrayUtility.NearestIndexPriorToValue(sorted, i) + 1;

			foreach(Face face in mesh.facesInternal)
			{
				int[] indices = face.indices;

				for(int i = 0; i < indices.Length; i++)
					indices[i] -= offset[indices[i]];

				face.InvalidateCache();
			}

			// remove from sharedIndices & shift to account for deletions
			IEnumerable<KeyValuePair<int, int>> common = mesh.sharedIndicesInternal.ToDictionary().Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));
			IEnumerable<KeyValuePair<int, int>> commonUV = mesh.sharedIndicesUVInternal.ToDictionary().Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));

			mesh.SetVertices(vertices);
			mesh.SetSharedIndexes(common);
			mesh.SetSharedIndexesUV(commonUV);
		}
	}
}
