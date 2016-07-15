using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public class pb_FaceRebuildData
	{
		// new pb_Face
		public pb_Face face;
		// new vertices (all vertices required to rebuild, not just new)
		public List<pb_Vertex> vertices;
		// shared indices pointers (must match vertices length)
		public List<int> sharedIndices;
		// shared UV indices pointers (must match vertices length)
		public List<int> sharedIndicesUV;

		public override string ToString()
		{
			return string.Format("{0}\n{1}", vertices.ToString(", "), sharedIndices.ToString(", "));
		}

		public static void Apply(
			IEnumerable<pb_FaceRebuildData> newFaces,
			pb_Object pb,
			List<pb_Vertex> vertices = null,
			List<pb_Face> faces = null,
			Dictionary<int, int> lookup = null,
			Dictionary<int, int> lookupUV = null)
		{
			List<pb_Face> _faces = faces == null ? new List<pb_Face>(pb.faces) : faces;
			pb_FaceRebuildData.Apply(newFaces, vertices, _faces, lookup, lookupUV);
			pb.SetVertices(vertices);
			pb.SetFaces(_faces.ToArray());
		}

		/**
		 *	Shift face rebuild data to appropriate positions and update the vertex, face, and
		 *	shared indices arrays.
		 */
		public static void Apply(
			IEnumerable<pb_FaceRebuildData> newFaces,
			List<pb_Vertex> vertices,
			List<pb_Face> faces,
			Dictionary<int, int> sharedIndices,
			Dictionary<int, int> sharedIndicesUV = null)
		{
			int index = vertices.Count;

			foreach(pb_FaceRebuildData rd in newFaces)
			{
				pb_Face face = rd.face;
				int faceVertexCount = face.distinctIndices.Length;
				bool hasSharedIndices = sharedIndices != null && rd.sharedIndices != null && rd.sharedIndices.Count == faceVertexCount;
				bool hasSharedIndicesUV = sharedIndicesUV != null && rd.sharedIndicesUV != null && rd.sharedIndicesUV.Count == faceVertexCount;

				for(int n = 0; n < faceVertexCount; n++)
				{
					int localIndex = face.distinctIndices[n];

					if(sharedIndices != null)
						sharedIndices.Add(localIndex + index, hasSharedIndices ? rd.sharedIndices[localIndex] : -1);

					if(sharedIndicesUV != null)
						sharedIndicesUV.Add(localIndex + index, hasSharedIndicesUV ? rd.sharedIndicesUV[localIndex] : -1);
				}


				for(int n = 0; n < face.indices.Length; n++)
					face.indices[n] += index;

				face.RebuildCaches();

				index += rd.vertices.Count;

				faces.Add(face);
				vertices.AddRange(rd.vertices);
			}
		}
	}
}
