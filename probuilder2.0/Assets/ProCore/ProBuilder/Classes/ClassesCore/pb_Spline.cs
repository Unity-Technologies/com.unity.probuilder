using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public static class pb_Spline
	{
		/**
		 *	Create a new pb_Object by extruding along a bezier spline.
		 */
		public static pb_Object Extrude(IList<pb_BezierPoint> points, float radius = .5f, int columns = 32, int rows = 16, bool closeLoop = false)
		{
			pb_Object pb = null;
			Extrude(points, radius, columns, rows, closeLoop, ref pb);
			return pb;
		}

		/**
		 *	Update a pb_Object with new geometry from a bezier spline.
		 */
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
			if(points.Count < 2)
				return;

			int cnt = points.Count;
			int rowsPlus1 = System.Math.Max(4, rows + 1);
			int rowsPlus1Times2 = rowsPlus1 * 2;
			int vertexCount = ((closeLoop ? cnt : cnt - 1) * 2) * rowsPlus1Times2;
			bool vertexCountsMatch = vertexCount == (target == null ? 0 : target.vertexCount);

			Vector3[] positions = new Vector3[vertexCount];
			pb_Face[] faces = vertexCountsMatch ? null : new pb_Face[(closeLoop ? cnt : cnt - 1) * rowsPlus1];

			int triangleIndex = 0, faceIndex = 0, vertexIndex = 0;

			for(int i = 0; i < (closeLoop ? cnt : cnt - 1); i++)
			{
				float secant_a, secant_b;

 				Quaternion rotation_a = GetRingRotation(points, i, closeLoop, out secant_a);
 				Quaternion rotation_b = GetRingRotation(points, (i+1)%cnt, closeLoop, out secant_b);

				Vector3[] ringA = VertexRing(rotation_a, points[i], radius, rowsPlus1);
				Vector3[] ringB = VertexRing(rotation_b, points[(i+1)%cnt], radius, rowsPlus1);

				System.Array.Copy(ringA, 0, positions, vertexIndex, rowsPlus1Times2);
				vertexIndex += rowsPlus1Times2;
				System.Array.Copy(ringB, 0, positions, vertexIndex, rowsPlus1Times2);
				vertexIndex += rowsPlus1Times2;

				if(!vertexCountsMatch)
				{
					for(int n = 0; n < rowsPlus1Times2; n += 2)
					{
						faces[faceIndex] = new pb_Face(new int[6] {
							triangleIndex, triangleIndex + 1, triangleIndex + rowsPlus1Times2,
							triangleIndex + rowsPlus1Times2, triangleIndex + 1, triangleIndex + rowsPlus1Times2 + 1 } );

						faceIndex++;
						triangleIndex += 2;
					}

					triangleIndex += rowsPlus1Times2;
				}
			}

			if(target != null)
			{
				if(faces != null)
				{
					target.GeometryWithVerticesFaces(positions, faces);
				}
				else
				{
					target.SetVertices(positions);
					target.ToMesh();
					target.Refresh(RefreshMask.UV | RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents);
				}
			}
			else
			{
				target = pb_Object.CreateInstanceWithVerticesFaces(positions, faces);
			}
		}

		private static Quaternion GetRingRotation(IList<Vector3> points, int i, bool closeLoop, out float secant)
		{
			int cnt = points.Count;
			Vector3 dir;

			if(closeLoop || (i > 0 && i < cnt-1))
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
