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
		// shared indexes pointers (must match vertexes length)
		public List<int> sharedIndexes;
		// shared UV indexes pointers (must match vertexes length)
		public List<int> sharedIndexesUV;
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
			return string.Format("{0}\n{1}", vertexes.ToString(", "), sharedIndexes.ToString(", "));
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
			pb.faces = _faces;
			pb.SetSharedIndexes(lookup);
			pb.SetSharedIndexesUV(lookupUV);
		}

		/// <summary>
		/// Shift face rebuild data to appropriate positions and update the vertex, face, and shared indexes arrays.
		/// </summary>
		/// <param name="newFaces"></param>
		/// <param name="vertexes"></param>
		/// <param name="faces"></param>
		/// <param name="sharedIndexes"></param>
		/// <param name="sharedIndexesUV"></param>
		public static void Apply(
			IEnumerable<FaceRebuildData> newFaces,
			List<Vertex> vertexes,
			List<Face> faces,
			Dictionary<int, int> sharedIndexes,
			Dictionary<int, int> sharedIndexesUV = null)
		{
			int index = vertexes.Count;

			foreach(FaceRebuildData rd in newFaces)
			{
				Face face = rd.face;
				int faceVertexCount = rd.vertexes.Count;

				bool hasSharedIndexes = sharedIndexes != null && rd.sharedIndexes != null && rd.sharedIndexes.Count == faceVertexCount;
				bool hasSharedIndexesUV = sharedIndexesUV != null && rd.sharedIndexesUV != null && rd.sharedIndexesUV.Count == faceVertexCount;

				for(int n = 0; n < faceVertexCount; n++)
				{
					int localIndex = n;

					if(sharedIndexes != null)
						sharedIndexes.Add(localIndex + index, hasSharedIndexes ? rd.sharedIndexes[localIndex] : -1);

					if(sharedIndexesUV != null)
						sharedIndexesUV.Add(localIndex + index, hasSharedIndexesUV ? rd.sharedIndexesUV[localIndex] : -1);
				}

				rd._appliedOffset = index;
				int[] faceIndexes = face.indexesInternal;

				for(int n = 0, c = faceIndexes.Length; n < c; n++)
					faceIndexes[n] += index;

				index += rd.vertexes.Count;
				face.indexesInternal = faceIndexes;
				faces.Add(face);
				vertexes.AddRange(rd.vertexes);
			}
		}
	}
}
