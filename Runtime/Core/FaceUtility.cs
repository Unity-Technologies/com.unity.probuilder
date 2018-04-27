namespace UnityEngine.ProBuilder
{
	public static class FaceUtility
	{
		/// <summary>
		/// Return all edges, including non-perimeter ones.
		/// </summary>
		/// <returns></returns>
		[System.Obsolete]
		public static Edge[] GetAllEdges(this Face face)
		{
			int[] indices = face.indices;

			Edge[] edges = new Edge[indices.Length];

			for (var i = 0; i < indices.Length; i += 3)
			{
				edges[i] = new Edge(indices[i + 0], indices[i + 1]);
				edges[i + 1] = new Edge(indices[i + 1], indices[i + 2]);
				edges[i + 2] = new Edge(indices[i + 2], indices[i + 0]);
			}

			return edges;
		}

		/// <summary>
		/// Add offset to each value in the indices array.
		/// </summary>
		/// <param name="face">Target face to apply the offset to.</param>
		/// <param name="offset"></param>
		public static void ShiftIndices(this Face face, int offset)
		{
			int[] indices = face.indices;
			for (int i = 0, c = indices.Length; i < c; i++)
				indices[i] += offset;
			face.InvalidateCache();
		}

		/// <summary>
		/// Returns the smallest value in the indices array.
		/// </summary>
		/// <returns></returns>
		static int SmallestIndexValue(this Face face)
		{
			int[] indices = face.indices;
			int smallest = indices[0];

			for (int i = 0; i < indices.Length; i++)
			{
				if (indices[i] < smallest)
					smallest = indices[i];
			}

			return smallest;
		}

		/// <summary>
		/// Shifts all triangles to be zero indexed.
		/// Ex:
		/// new pb_Face(3,4,5).ShiftIndicesToZero();
		/// Sets the pb_Face index array to 0,1,2
		/// </summary>
		public static void ShiftIndicesToZero(this Face face)
		{
			int offset = SmallestIndexValue(face);
			int[] indices = face.indices;
			int[] distinct = face.distinctIndices;
			Edge[] edges = face.edgesInternal;

			for (int i = 0; i < indices.Length; i++)
				indices[i] -= offset;

			for (int i = 0; i < distinct.Length; i++)
				distinct[i] -= offset;

			for (int i = 0; i < edges.Length; i++)
			{
				edges[i].x -= offset;
				edges[i].y -= offset;
			}

			face.InvalidateCache();
		}

		public static void Reverse(this Face face)
		{
			int[] indices = face.indices;
			System.Array.Reverse(indices);
			face.indices = indices;
		}
	}
}
