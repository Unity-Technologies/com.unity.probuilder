using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Functions for removing vertices and triangles from a mesh.
	 */
	public static class pb_DeleteVertices
	{
		/**
		 * Removes vertices that no face references.
		 */
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

		/**
		 *	Deletes the vertices from the passed index array.  Handles rebuilding the sharedIndices array.  Does not retriangulate face.
		 */
		public static void DeleteVerticesWithIndices(this pb_Object pb, IEnumerable<int> distInd)
		{
			// pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			// vertices = vertices.RemoveAt(distInd);
			// pb_Face[] nFaces = pb.faces;

			// // shift all other face indices down to account for moved vertex positions
			// for(int i = 0; i < nFaces.Length; i++)
			// {
			// 	int[] tris = nFaces[i].indices;

			// 	for(int n = 0; n < tris.Length; n++)
			// 	{
			// 		int sub = 0;
			// 		foreach(int d in distInd)
			// 		{
			// 			if(tris[n] > d)
			// 				sub++;
			// 		}
			// 		tris[n] -= sub;
			// 	}

			// 	nFaces[i].SetIndices(tris);
			// }

			// // shift all other face indices in the shared index array down to account for moved vertex positions
			// pb_IntArray[] si = pb.sharedIndices;
			// pb_IntArray[] su = pb.sharedIndicesUV;
			// pb_IntArrayUtility.RemoveValuesAndShift(ref si, distInd);
			// pb_IntArrayUtility.RemoveValuesAndShift(ref su, distInd);
			// pb.SetSharedIndices(si);
			// pb.SetSharedIndicesUV(su);
			// pb.SetVertices(vertices);
			// pb.SetFaces(nFaces);
			// pb.ToMesh();

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
				offset[i] = pbUtil.NearestIndexPriorToValue(sorted, i) + 1;

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
			pb.ToMesh();
		}
	}
}
