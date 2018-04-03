using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for removing vertices and triangles from a mesh.
	/// </summary>
	public static class pb_DeleteVertices
	{
		/// <summary>
		/// Removes vertices that no face references.
		/// </summary>
		/// <param name="pb"></param>
		/// <returns>A list of deleted vertex indices.</returns>
		public static int[] RemoveUnusedVertices(this pb_Object pb)
		{
			List<int> del = new List<int>();
			HashSet<int> tris = new HashSet<int>(pb_Face.AllTriangles(pb.faces));

			for(int i = 0; i < pb.vertices.Length; i++)
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
		public static void DeleteVerticesWithIndices(this pb_Object pb, IEnumerable<int> distInd)
		{
			if(distInd == null || distInd.Count() < 1)
				return;

			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			int originalVertexCount = vertices.Length;
			int[] offset = new int[originalVertexCount];

			List<int> sorted = new List<int>(distInd);

			sorted.Sort();

			vertices = vertices.SortedRemoveAt(sorted);

			// Add 1 because NearestIndexPriorToValue is 0 indexed.
			for(int i = 0; i < originalVertexCount; i++)
				offset[i] = pb_Util.NearestIndexPriorToValue(sorted, i) + 1;

			foreach(pb_Face face in pb.faces)
			{
				int[] indices = face.indices;

				for(int i = 0; i < indices.Length; i++)
					indices[i] -= offset[indices[i]];

				face.RebuildCaches();
			}

			// remove from sharedIndices & shift to account for deletions
			IEnumerable<KeyValuePair<int, int>> common = pb.sharedIndices.ToDictionary().Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));
			IEnumerable<KeyValuePair<int, int>> commonUV = pb.sharedIndicesUV.ToDictionary().Where(x => sorted.BinarySearch(x.Key) < 0).Select(y => new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value));

			pb.SetVertices(vertices);
			pb.SetSharedIndices(common);
			pb.SetSharedIndicesUV(commonUV);
		}
	}
}
