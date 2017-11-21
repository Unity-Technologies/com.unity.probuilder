using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	/// <summary>
	/// Information required to append a face to a pb_Object.
	/// </summary>
	class pb_FaceRebuildData
	{
#pragma warning disable 0649
		// new pb_Face
		public pb_Face face;
		// new vertices (all vertices required to rebuild, not just new)
		public List<pb_Vertex> vertices;
		// shared indices pointers (must match vertices length)
		public List<int> sharedIndices;
		// shared UV indices pointers (must match vertices length)
		public List<int> sharedIndicesUV;
		// The offset applied to this face via Apply() call.
		private int _appliedOffset = 0;
#pragma warning restore 0649

		/**
		 * If this face has been applied to a pb_Object via Apply() this returns the index offset applied.
		 */
		public int Offset()
		{
			return _appliedOffset;
		}

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

			if(vertices == null)
				vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );

			if(lookup == null)
				lookup = pb.sharedIndices.ToDictionary();

			if(lookupUV == null)
				lookupUV = pb.sharedIndicesUV != null ? pb.sharedIndicesUV.ToDictionary() : null;

			pb_FaceRebuildData.Apply(newFaces, vertices, _faces, lookup, lookupUV);

			pb.SetVertices(vertices);
			pb.SetFaces(_faces.ToArray());
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(lookupUV);
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
				int faceVertexCount = rd.vertices.Count;

				bool hasSharedIndices 	= sharedIndices != null && rd.sharedIndices != null && rd.sharedIndices.Count == faceVertexCount;
				bool hasSharedIndicesUV = sharedIndicesUV != null && rd.sharedIndicesUV != null && rd.sharedIndicesUV.Count == faceVertexCount;

				for(int n = 0; n < faceVertexCount; n++)
				{
					int localIndex = n;

					if(sharedIndices != null)
						sharedIndices.Add(localIndex + index, hasSharedIndices ? rd.sharedIndices[localIndex] : -1);

					if(sharedIndicesUV != null)
						sharedIndicesUV.Add(localIndex + index, hasSharedIndicesUV ? rd.sharedIndicesUV[localIndex] : -1);
				}

				rd._appliedOffset = index;

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
