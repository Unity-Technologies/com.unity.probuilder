using System;
using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Extension methods for Vector classes.
	/// </summary>
	static class pb_Vector
	{
		public const float FLT_COMPARE_RESOLUTION = 1000f;

		private static int HashFloat(float f)
		{
			ulong u = (ulong) (f * FLT_COMPARE_RESOLUTION);
			return (int) (u % int.MaxValue);
		}

		/**
		 *	Return the rounded hashcode for a vector2
		 */
		public static int GetHashCode(Vector2 v)
		{
			// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
			int hash = 27;

			unchecked
			{
				hash = hash * 29 + HashFloat(v.x);
				hash = hash * 29 + HashFloat(v.y);
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
				hash = hash * 29 + HashFloat(v.x);
				hash = hash * 29 + HashFloat(v.y);
				hash = hash * 29 + HashFloat(v.z);
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
				hash = hash * 29 + HashFloat(v.x);
				hash = hash * 29 + HashFloat(v.y);
				hash = hash * 29 + HashFloat(v.z);
				hash = hash * 29 + HashFloat(v.w);
			}

			return hash;
		}
	}
}
