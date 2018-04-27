using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
		/// <param name="pb"></param>
		/// <returns>A list of deleted vertex indices.</returns>
		public static int[] RemoveUnusedVertices(this ProBuilderMesh pb)
		{
			List<int> del = new List<int>();
			HashSet<int> tris = new HashSet<int>(Face.AllTriangles(pb.facesInternal));

			for(int i = 0; i < pb.positionsInternal.Length; i++)
				if(!tris.Contains(i))
					del.Add(i);

			pb.DeleteVerticesWithIndices(del);

			return del.ToArray();
		}

		/// <summary>
		/// Deletes the vertices from the passed index array, and handles rebuilding the sharedIndices array.
		/// </summary>
		/// <remarks>This function does not retriangulate the mesh. Ie, you are responsible for ensuring that indices
		/// deleted by this function are not referenced by any triangles.</remarks>
		/// <param name="pb"></param>
		/// <param name="distInd"></param>
		public static void DeleteVerticesWithIndices(this ProBuilderMesh pb, IEnumerable<int> distInd)
		{
			if(distInd == null || distInd.Count() < 1)
				return;

			Vertex[] vertices = Vertex.GetVertices(pb);
			int originalVertexCount = vertices.Length;
			int[] offset = new int[originalVertexCount];

			List<int> sorted = new List<int>(distInd);

			sorted.Sort();

			vertices = vertices.SortedRemoveAt(sorted);

			// Add 1 because NearestIndexPriorToValue is 0 indexed.
			for(int i = 0; i < originalVertexCount; i++)
				offset[i] = InternalUtility.NearestIndexPriorToValue(sorted, i) + 1;

			foreach(Face face in pb.facesInternal)
			{
				int[] indices = face.indices;

				for(int i = 0; i < indices.Length; i++)
					indices[i] -= offset[indices[i]];

				face.Reverse();
			}

			// remove from sharedIndices & shift to account for deletions
			IEnumerable<KeyValuePair<int, int>> common = pb.sharedIndicesInternal.ToDictionary().Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));
			IEnumerable<KeyValuePair<int, int>> commonUV = pb.sharedIndicesUVInternal.ToDictionary().Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));

			pb.SetVertices(vertices);
			pb.SetSharedIndices(common);
			pb.SetSharedIndicesUV(commonUV);
		}
	}
}
