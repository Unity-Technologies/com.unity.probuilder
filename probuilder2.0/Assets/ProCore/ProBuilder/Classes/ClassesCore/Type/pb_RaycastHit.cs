using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_RaycastHit
	{
		public float Distance;
		public Vector3 Point;
		public Vector3 Normal;
		public int FaceIndex;

		public pb_RaycastHit(float InDistance,
								Vector3 InPoint,
								Vector3 InNormal,
								int InFaceIndex)
		{
			Distance = InDistance;
			Point = InPoint;
			Normal = InNormal;
			FaceIndex = InFaceIndex;
		}
	}
}