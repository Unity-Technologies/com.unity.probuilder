using UnityEngine;

namespace ProBuilder2.Common
{

	public static class pb_VectorUtility
	{
		public static float Distance2D(Vector4 a, Vector4 b)
		{
			return Mathf.Sqrt( (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) );
		}

		public static float SqrDistance2D(Vector4 a, Vector4 b)
		{
			return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
		}

		/**
		 *	Subtract a - b keeping b.z and b.w intact.
		 */
		public static Vector4 Subtract2D(Vector4 a, Vector4 b)
		{
			return new Vector4(a.x - b.x, a.y - b.y, b.z, b.w);
		}

		/**
		 *	Subtract a - b keeping b.z and b.w intact.
		 */
		public static Vector4 Subtract2D(Vector2 a, Vector4 b)
		{
			return new Vector4(a.x - b.x, a.y - b.y, b.z, b.w);
		}

		/**
		 *	Add (a + b) keeping b.z and b.w intact.
		 */
		public static Vector4 Add2D(Vector4 a, Vector4 b)
		{
			return new Vector4(a.x + b.x, a.y + b.y, b.z, b.w);
		}

		/**
		 *	Add (a + b) keeping b.z and b.w intact.
		 */
		public static Vector4 Add2D(Vector2 a, Vector4 b)
		{
			return new Vector4(a.x + b.x, a.y + b.y, b.z, b.w);
		}
	}
}
