using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	/**
	 *	Extension methods for Vector classes.
	 */
	public static class pb_Vector
	{
		public const float FLT_COMPARE_RESOLUTION = 1000f;

		/**
		 *	Return the rounded hashcode for a vector2
		 */
		public static int GetHashCode(Vector2 v)
		{
			// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
			int hash = 27;

			unchecked
			{
				hash = hash * 29 + Convert.ToInt32(v.x * FLT_COMPARE_RESOLUTION);
				hash = hash * 29 + Convert.ToInt32(v.y * FLT_COMPARE_RESOLUTION);
			}

			return hash;
		}

		/**
		 *	Return the hashcode for a vector3 without first converting it to pb_IntVec3.
		 */
		public static int GetHashCode(Vector3 v)
		{
			// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
			int hash = 27;

			unchecked
			{
				hash = hash * 29 + Convert.ToInt32(v.x * FLT_COMPARE_RESOLUTION);
				hash = hash * 29 + Convert.ToInt32(v.y * FLT_COMPARE_RESOLUTION);
				hash = hash * 29 + Convert.ToInt32(v.z * FLT_COMPARE_RESOLUTION);
			}

			return hash;
		}

		/**
		 *	Return the hashcode for a vector3 without first converting it to pb_IntVec3.
		 */
		public static int GetHashCode(Vector4 v)
		{
			// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
			int hash = 27;

			unchecked
			{
				hash = hash * 29 + Convert.ToInt32(v.x * FLT_COMPARE_RESOLUTION);
				hash = hash * 29 + Convert.ToInt32(v.y * FLT_COMPARE_RESOLUTION);
				hash = hash * 29 + Convert.ToInt32(v.z * FLT_COMPARE_RESOLUTION);
				hash = hash * 29 + Convert.ToInt32(v.w * FLT_COMPARE_RESOLUTION);
			}

			return hash;
		}
	}
}
