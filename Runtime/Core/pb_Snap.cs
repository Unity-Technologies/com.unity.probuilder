using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Snapping functions and ProGrids compatibility.
	/// </summary>
	static class pb_Snap
	{
		/// <summary>
		/// Round value to nearest snpVal increment.
		/// </summary>
		/// <param name="vertex"></param>
		/// <param name="snpVal"></param>
		/// <returns></returns>
		public static Vector3 SnapValue(Vector3 vertex, float snpVal)
		{
			// snapValue is a global setting that comes from ProGrids
			return new Vector3(
				snpVal * Mathf.Round(vertex.x / snpVal),
				snpVal * Mathf.Round(vertex.y / snpVal),
				snpVal * Mathf.Round(vertex.z / snpVal));
		}

		/// <summary>
		/// Round value to nearest snpVal increment.
		/// </summary>
		/// <param name="val"></param>
		/// <param name="snpVal"></param>
		/// <returns></returns>
		public static float SnapValue(float val, float snpVal)
		{
			return snpVal * Mathf.Round(val / snpVal);
		}

		/// <summary>
		///	An override that accepts a vector3 to use as a mask for which values to snap.  Ex;
		///	Snap((.3f, 3f, 41f), (0f, 1f, .4f)) only snaps Y and Z values (to 1 & .4 unit increments).
		/// </summary>
		/// <param name="vertex"></param>
		/// <param name="snap"></param>
		/// <returns></returns>
		public static Vector3 SnapValue(Vector3 vertex, Vector3 snap)
		{
			float _x = vertex.x, _y = vertex.y, _z = vertex.z;
			Vector3 v = new Vector3(
				( Mathf.Abs(snap.x) < 0.0001f ? _x : snap.x * Mathf.Round(_x / snap.x) ),
				( Mathf.Abs(snap.y) < 0.0001f ? _y : snap.y * Mathf.Round(_y / snap.y) ),
				( Mathf.Abs(snap.z) < 0.0001f ? _z : snap.z * Mathf.Round(_z / snap.z) )
				);
			return v;
		}
	}
}
