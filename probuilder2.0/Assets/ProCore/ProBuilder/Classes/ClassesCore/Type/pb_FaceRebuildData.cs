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

		/**
		 *	Shift face rebuild data to appropriate positions and update the vertex, face, and
		 *	shared indices arrays.
		 */
		public static void Apply(IList<pb_FaceRebuildData> new_faces,
			ref List<pb_Vertex> vertices,
			ref List<pb_Face> faces,
			ref Dictionary<int, int> sharedIndices,
			ref Dictionary<int, int> sharedIndicesUV)
		{
			int index = vertices.Count;

			for(int i = 0; i < new_faces.Count; i++)
			{
				pb_FaceRebuildData rd = new_faces[i];
				pb_Face face = rd.face;

				for(int n = 0; n < face.distinctIndices.Length; n++)
				{
					int localIndex = face.distinctIndices[n];

					if(sharedIndices != null && rd.sharedIndices != null)
						sharedIndices.Add(localIndex + index, rd.sharedIndices[localIndex]);

					if(sharedIndicesUV != null && rd.sharedIndicesUV != null)
						sharedIndicesUV.Add(localIndex + index, rd.sharedIndicesUV[localIndex]);
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
