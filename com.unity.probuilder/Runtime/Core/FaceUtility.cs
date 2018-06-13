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
		/// Add offset to each value in the indexes array.
		/// </summary>
		/// <param name="face">Target face to apply the offset to.</param>
		/// <param name="offset">The value to add to each index.</param>
		public static void ShiftIndexes(this Face face, int offset)
		{
            if (face == null)
                throw new ArgumentNullException("face");

			int[] indexes = face.indexesInternal;
			for (int i = 0, c = indexes.Length; i < c; i++)
				indexes[i] += offset;
			face.InvalidateCache();
		}

		/// <summary>
		/// Find the smallest value in the triangles array.
		/// </summary>
		/// <returns>The smallest value in the indexes array.</returns>
		static int SmallestIndexValue(this Face face)
		{
			int[] indexes = face.indexesInternal;
			int smallest = indexes[0];

			for (int i = 1; i < indexes.Length; i++)
			{
				if (indexes[i] < smallest)
					smallest = indexes[i];
			}

			return smallest;
		}

        /// <summary>
        /// Finds the smallest value in the indexes array, then offsets by subtracting that value from each index.
        /// </summary>
        /// <example>
        /// ```
        /// // sets the indexes array to `{0, 1, 2}`.
        /// new pb_Face(3,4,5).ShiftIndexesToZero();
        /// ```
        /// </example>
        /// <param name="face">The face that will have it's triangle array offset.</param>
        public static void ShiftIndexesToZero(this Face face)
		{
            if (face == null)
                throw new ArgumentNullException("face");

            int offset = SmallestIndexValue(face);
			int[] indexes = face.indexesInternal;
			int[] distinct = face.distinctIndexesInternal;
			Edge[] edges = face.edgesInternal;

			for (int i = 0; i < indexes.Length; i++)
				indexes[i] -= offset;

			for (int i = 0; i < distinct.Length; i++)
				distinct[i] -= offset;

			for (int i = 0; i < edges.Length; i++)
			{
				edges[i].a -= offset;
				edges[i].b -= offset;
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

            int[] indexes = face.indexesInternal;
			Array.Reverse(indexes);
			face.indexesInternal = indexes;
		}
	}
}
