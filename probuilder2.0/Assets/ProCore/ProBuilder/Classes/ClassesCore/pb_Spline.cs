using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public static class pb_Spline
	{
		/**
		 *	Extrude a bezier spline.
		 */
		public static pb_Object Extrude(IList<pb_BezierPoint> points, float radius = .5f, int segments = 16, bool closeLoop = false)
		{
			pb_Object pb = null;
			Extrude(points, radius, segments, closeLoop, ref pb);
			return pb;
		}

		public static void Extrude(IList<pb_BezierPoint> bezierPoints, float radius, int segments, bool closeLoop, ref pb_Object target)
		{
			List<Vector3> positions = new List<Vector3>(segments + 1);

			int c = bezierPoints.Count;

			for( int i = 0; i < (closeLoop ? c : c - 1); i++ )
			{
				for(int n = 0; n < segments; n++)
				{
					float s = (closeLoop && i >= c -1) ? segments - 1 : segments; // (!closeLoop && (i >= c - 2)) ? segments - 1 : segments;
					positions.Add( pb_BezierPoint.CubicPosition(bezierPoints[i], bezierPoints[(i+1)%c], n / s) );
				}
			}

			Extrude(positions, radius, segments, ref target);
		}

		public static void Extrude(IList<Vector3> points, float radius, int segments, ref pb_Object target)
		{
			List<Vector3> positions = new List<Vector3>();
			List<pb_Face> faces = new List<pb_Face>();

			int cnt = points.Count;
			int index = 0;
			int s2 = segments * 2;

			for(int i = 0; i < cnt - 1; i++)
			{
				float secant_a, secant_b;

 				Quaternion rotation_a = GetRingRotation(points, i, out secant_a);
 				Quaternion rotation_b = GetRingRotation(points, i+1, out secant_b);

				Vector3[] ringA = VertexRing(rotation_a, points[i], radius, segments);
				Vector3[] ringB = VertexRing(rotation_b, points[i+1%cnt], radius, segments);

				positions.AddRange(ringA);
				positions.AddRange(ringB);

				for(int n = 0; n < s2; n += 2)
				{
					faces.Add( new pb_Face(new int[6] {
						index, index + 1, index + s2,
						index + s2, index + 1, index + s2 + 1 } ));

					index += 2;
				}

				index += segments * 2;
			}

			if(target != null)
				target.GeometryWithVerticesFaces(positions.ToArray(), faces.ToArray());
			else
				target = pb_Object.CreateInstanceWithVerticesFaces(positions.ToArray(), faces.ToArray());
		}

		private static Quaternion GetRingRotation(IList<Vector3> points, int i, out float secant)
		{
			int cnt = points.Count;
			Vector3 dir;

			if(i > 0 && i < cnt-1)
			{
				int a = i < 1 ? cnt-1 : i-1;
				int b = i;
				int c = (i+1) % cnt;

				Vector3 coming = (points[b] - points[a]).normalized;
				Vector3 leaving = (points[c] - points[b]).normalized;

				dir = (coming + leaving) * .5f;

				secant = pb_Math.Secant(Vector3.Angle(coming, dir) * Mathf.Deg2Rad);
			}
			else
			{
				if(i < 1)
					dir = points[i+1] - points[i];
				else
					dir = points[i] - points[i-1];

				secant = 1f;
			}

			dir.Normalize();

			return Quaternion.LookRotation(dir);
		}

		private static Vector3[] VertexRing(Quaternion orientation, Vector3 offset, float radius, int segments)
		{
			Vector3[] v = new Vector3[segments * 2];

			for (int i = 0; i < segments; i++)
			{
				float rad0 = (i / (float)(segments - 1)) * 360f * Mathf.Deg2Rad;
				int n = (i + 1) % segments;
				float rad1 = (n / (float)(segments - 1)) * 360f * Mathf.Deg2Rad;

				v[i*2] = offset + (orientation * new Vector3(Mathf.Cos(rad0) * radius, Mathf.Sin(rad0) * radius, 0f));
				v[i*2+1] = offset + (orientation * new Vector3(Mathf.Cos(rad1) * radius, Mathf.Sin(rad1) * radius, 0f));
			}

			return v;
		}
	}
}
