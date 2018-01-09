using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// The results of a raycast against a pb_Object.
	/// </summary>
	class pb_RaycastHit
	{
		public float distance;
		public Vector3 point;
		public Vector3 normal;
		public int face;

		public pb_RaycastHit(	float InDistance,
								Vector3 InPoint,
								Vector3 InNormal,
								int InFaceIndex)
		{
			distance = InDistance;
			point = InPoint;
			normal = InNormal;
			face = InFaceIndex;
		}
	}
}
