using System;

namespace UnityEngine.ProBuilder
{
	public static class FaceUtility
	{
		/// <summary>
		/// Add offset to each value in the indices array.
		/// </summary>
		/// <param name="face">Target face to apply the offset to.</param>
		/// <param name="offset"></param>
		public static void ShiftIndexes(this Face face, int offset)
		{
            if (face == null)
                throw new ArgumentNullException("face");

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
        /// new pb_Face(3,4,5).ShiftIndexesToZero();
        /// Sets the pb_Face index array to 0,1,2
        /// </summary>
        public static void ShiftIndexesToZero(this Face face)
		{
            if (face == null)
                throw new ArgumentNullException("face");

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
            if (face == null)
                throw new ArgumentNullException("face");

            int[] indices = face.indices;
			System.Array.Reverse(indices);
			face.indices = indices;
		}
	}
}
