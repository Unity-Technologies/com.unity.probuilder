using UnityEngine;

namespace ProBuilder2.Common
{
	[System.Serializable]
	public struct pb_BezierPoint
	{
		public Vector3 position;
		public Vector3 tangentIn;
		public Vector3 tangentOut;

		public pb_BezierPoint(Vector3 position, Vector3 tangentIn, Vector3 tangentOut)
		{
			this.position = position;
			this.tangentIn = tangentIn;
			this.tangentOut = tangentOut;
		}

		public static Vector3 QuadraticPosition(pb_BezierPoint a, pb_BezierPoint b, float t)
		{
			float x = (1f- t) * (1f - t) * a.position.x + 2f * (1f - t) * t * a.tangentOut.x + t * t * b.position.x;
			float y = (1f- t) * (1f - t) * a.position.y + 2f * (1f - t) * t * a.tangentOut.y + t * t * b.position.y;
			float z = (1f- t) * (1f - t) * a.position.z + 2f * (1f - t) * t * a.tangentOut.z + t * t * b.position.z;
			return new Vector3(x, y, z);
		}

		public static Vector3 CubicPosition(pb_BezierPoint a, pb_BezierPoint b, float t)
		{
			t = Mathf.Clamp01(t);

			float oneMinusT = 1f - t;

			return 	oneMinusT * oneMinusT * oneMinusT * a.position +
					3f * oneMinusT * oneMinusT * t * a.tangentOut +
					3f * oneMinusT * t * t * b.tangentIn +
					t * t * t * b.position;
		}
	}
}
