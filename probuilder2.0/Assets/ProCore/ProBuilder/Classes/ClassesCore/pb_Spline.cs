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
		public static pb_Object Extrude(IList<pb_BezierPoint> points, float radius = .5f, int columns = 32, int rows = 16, bool closeLoop = false)
		{
			pb_Object pb = null;
			Extrude(points, radius, columns, rows, closeLoop, ref pb);
			return pb;
		}

		public static void Extrude(IList<pb_BezierPoint> bezierPoints, float radius, int columns, int rows, bool closeLoop, ref pb_Object target)
		{
			int c = bezierPoints.Count;
			int cols = columns;
			List<Vector3> positions = new List<Vector3>(cols * c);

			for( int i = 0; i < (closeLoop ? c : c - 1); i++ )
			{
				for(int n = 0; n < ((!closeLoop && i >= c - 2) ? cols + 1 : cols); n++)
				{
					float s = cols;
					positions.Add( pb_BezierPoint.CubicPosition(bezierPoints[i], bezierPoints[(i+1)%c], n / s) );
				}
			}

			Extrude(positions, radius, rows, closeLoop, ref target);
		}

		public static void Extrude(IList<Vector3> points, float radius, int rows, bool closeLoop, ref pb_Object target)
		{
			List<Vector3> positions = new List<Vector3>();
			List<pb_Face> faces = new List<pb_Face>();

			int cnt = points.Count;
			int index = 0;
			int rowsPlus1 = System.Math.Max(4, rows + 1);
			int s2 = rowsPlus1 * 2;

			for(int i = 0; i < (closeLoop ? cnt : cnt - 1); i++)
			{
				float secant_a, secant_b;

 				Quaternion rotation_a = GetRingRotation(points, i, out secant_a);
 				Quaternion rotation_b = GetRingRotation(points, (i+1)%cnt, out secant_b);

				Vector3[] ringA = VertexRing(rotation_a, points[i], radius, rowsPlus1);
				Vector3[] ringB = VertexRing(rotation_b, points[(i+1)%cnt], radius, rowsPlus1);

				positions.AddRange(ringA);
				positions.AddRange(ringB);

				for(int n = 0; n < s2; n += 2)
				{
					faces.Add( new pb_Face(new int[6] {
						index, index + 1, index + s2,
						index + s2, index + 1, index + s2 + 1 } ));

					index += 2;
				}

				index += s2;
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

			if(pb_Math.Approx3(dir, Vector3.up) || pb_Math.Approx3(dir, Vector3.zero))
				return Quaternion.identity;

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
