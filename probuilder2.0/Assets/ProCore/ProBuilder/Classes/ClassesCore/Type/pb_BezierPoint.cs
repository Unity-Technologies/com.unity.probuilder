using UnityEngine;

namespace ProBuilder2.Common
{
	[System.Serializable]
	public struct pb_BezierPoint
	{
		public Vector3 position;
		public Vector3 tangent;

		public pb_BezierPoint(Vector3 position, Vector3 tangent)
		{
			this.position = position;
			this.tangent = tangent;
		}
	}
}
