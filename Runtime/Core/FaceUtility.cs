using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Static helper functions for working with @"UnityEngine.ProBuilder.Face" type.
	/// </summary>
	public static class FaceUtility
	{
		/// <summary>
		/// Add offset to each value in the indices array.
		/// </summary>
		/// <param name="face">Target face to apply the offset to.</param>
		/// <param name="offset">The value to add to each index.</param>
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
		/// Find the smallest value in the triangles array.
		/// </summary>
		/// <returns>The smallest value in the indices array.</returns>
		static int SmallestIndexValue(this Face face)
		{
			int[] indices = face.indices;
			int smallest = indices[0];

			for (int i = 1; i < indices.Length; i++)
			{
				if (indices[i] < smallest)
					smallest = indices[i];
			}

			return smallest;
		}

        /// <summary>
        /// Finds the smallest value in the indices array, then offsets by subtracting that value from each index.
        /// </summary>
        /// <example>
        /// ```
        /// // sets the indices array to `{0, 1, 2}`.
        /// new pb_Face(3,4,5).ShiftIndexesToZero();
        /// ```
        /// </example>
        /// <param name="face">The face that will have it's triangle array offset.</param>
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

		/// <summary>
		/// Reverse the order of the triangle array. This has the effect of reversing the direction that this face renders.
		/// </summary>
		/// <param name="face">The target face.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void Reverse(this Face face)
		{
            if (face == null)
                throw new ArgumentNullException("face");

            int[] indices = face.indices;
			Array.Reverse(indices);
			face.indices = indices;
		}
	}
}
