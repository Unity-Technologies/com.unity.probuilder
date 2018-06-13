using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Information required to append a face to a pb_Object.
	/// </summary>
	sealed class FaceRebuildData
	{
#pragma warning disable 0649
		// new pb_Face
		public Face face;
		// new vertexes (all vertexes required to rebuild, not just new)
		public List<Vertex> vertexes;
		// shared indices pointers (must match vertexes length)
		public List<int> sharedIndices;
		// shared UV indices pointers (must match vertexes length)
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
			return string.Format("{0}\n{1}", vertexes.ToString(", "), sharedIndices.ToString(", "));
		}

		public static void Apply(
			IEnumerable<FaceRebuildData> newFaces,
			ProBuilderMesh pb,
			List<Vertex> vertexes = null,
			List<Face> faces = null,
			Dictionary<int, int> lookup = null,
			Dictionary<int, int> lookupUV = null)
		{
			List<Face> _faces = faces == null ? new List<Face>(pb.facesInternal) : faces;

			if(vertexes == null)
				vertexes = new List<Vertex>( Vertex.GetVertexes(pb) );

			if(lookup == null)
				lookup = pb.sharedIndexesInternal.ToDictionary();

			if(lookupUV == null)
				lookupUV = pb.sharedIndexesUVInternal != null ? pb.sharedIndexesUVInternal.ToDictionary() : null;

			FaceRebuildData.Apply(newFaces, vertexes, _faces, lookup, lookupUV);

			pb.SetVertexes(vertexes);
			pb.SetFaces(_faces.ToArray());
			pb.SetSharedIndexes(lookup);
			pb.SetSharedIndexesUV(lookupUV);
		}

		/// <summary>
		/// Shift face rebuild data to appropriate positions and update the vertex, face, and shared indices arrays.
		/// </summary>
		/// <param name="newFaces"></param>
		/// <param name="vertexes"></param>
		/// <param name="faces"></param>
		/// <param name="sharedIndices"></param>
		/// <param name="sharedIndicesUV"></param>
		public static void Apply(
			IEnumerable<FaceRebuildData> newFaces,
			List<Vertex> vertexes,
			List<Face> faces,
			Dictionary<int, int> sharedIndices,
			Dictionary<int, int> sharedIndicesUV = null)
		{
			int index = vertexes.Count;

			foreach(FaceRebuildData rd in newFaces)
			{
				Face face = rd.face;
				int faceVertexCount = rd.vertexes.Count;

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
				int[] indices = face.indexesInternal;

				for(int n = 0, c = indices.Length; n < c; n++)
					indices[n] += index;

				index += rd.vertexes.Count;
				face.indexesInternal = indices;
				faces.Add(face);
				vertexes.AddRange(rd.vertexes);
			}
		}
	}
}
