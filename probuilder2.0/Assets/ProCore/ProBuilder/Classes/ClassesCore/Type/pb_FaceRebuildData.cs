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
	}
}
