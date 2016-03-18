using UnityEngine;

namespace ProBuilder2.Common
{

	public static class pb_VectorUtility
	{
		public static float Distance2D(Vector4 a, Vector4 b)
		{
			return Mathf.Sqrt( (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) );
		}
	}
}
