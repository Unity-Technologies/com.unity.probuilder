using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public static class pb_Spline
	{
		private static Quaternion GetRingRotation(IList<pb_BezierPoint> points, int i, bool closeLoop, out float secant)
		{
			int cnt = points.Count;
			Vector3 dir;

			if((i > 0 && i < cnt-1) || closeLoop)
			{
				int a = i < 1 ? cnt-1 : i-1;
				int b = i;
				int c = (i+1) % cnt;

				Vector3 coming = (points[b].position - points[a].position).normalized;
				Vector3 leaving = (points[c].position - points[b].position).normalized;

				dir = (coming + leaving) * .5f;

				secant = pb_Math.Secant(Vector3.Angle(coming, dir) * Mathf.Deg2Rad);
			}
			else
			{
				if(i < 1)
					dir = points[i+1].position - points[i].position;
				else
					dir = points[i].position - points[i-1].position;

				secant = 1f;
			}

			dir.Normalize();

			return Quaternion.LookRotation(dir);
		}

		public static pb_Object Extrude(IList<pb_BezierPoint> points, float radius = .5f, int segments = 16, bool closeLoop = false)
		{
			List<Vector3> positions = new List<Vector3>();
			List<pb_Face> faces = new List<pb_Face>();

			int cnt = points.Count;
			int index = 0;
			int s2 = segments * 2;

			for(int i = 0; i < cnt; i ++)
			{
				if(i >= cnt - 1 && !closeLoop)
					break;

				float secant_a, secant_b;

 				Quaternion rotation_a = GetRingRotation(points, i, closeLoop, out secant_a);
 				Quaternion rotation_b = GetRingRotation(points, i+1 % cnt, closeLoop, out secant_b);

				Vector3[] ringA = VertexRing(rotation_a, points[i].position, radius, segments);
				Vector3[] ringB = VertexRing(rotation_b, points[i+1%cnt].position, radius, segments);

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

			return pb_Object.CreateInstanceWithVerticesFaces(positions.ToArray(), faces.ToArray());
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
