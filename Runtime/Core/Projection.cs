using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Functions for projecting 3d points to 2d space.
	/// </summary>
	public static class Projection
	{
		static Vector3 s_UAxis = Vector3.zero, s_VAxis = Vector3.zero;

		/// <summary>
		/// Project a collection of 3d positions to a 2d plane.
		/// </summary>
		/// <param name="verts">A collection of positions to project based on a direction.</param>
		/// <param name="planeNormal">The normal to project points from.</param>
		/// <returns>The verts array projected into 2d coordinates.</returns>
		public static Vector2[] PlanarProject(IEnumerable<Vector3> verts, Vector3 planeNormal)
		{
			return PlanarProject(verts.ToArray(), planeNormal, VectorToProjectionAxis(planeNormal));
		}

		internal static Vector2[] PlanarProject(ProBuilderMesh pb, Face face)
		{
			Vector3 normal = Math.Normal(pb, face);
			return PlanarProject(pb.positionsInternal, normal, VectorToProjectionAxis(normal), face.indices);
		}

		internal static Vector2[] PlanarProject(IList<Vertex> vertices, IList<int> indices)
		{
			int len = indices.Count;

			Vector3[] v = new Vector3[len];

			for(int i = 0; i < len; i++)
				v[i] = vertices[indices[i]].position;

			Vector3 normal = Math.Normal(vertices, indices);
			ProjectionAxis axis = VectorToProjectionAxis(normal);

			return PlanarProject(v, normal, axis, null);
		}

		internal static Vector2[] PlanarProject(Vector3[] verts, Vector3 planeNormal, ProjectionAxis projectionAxis, int[] indices = null)
		{
			int len = indices == null || indices.Length < 1 ? verts.Length : indices.Length;
			Vector2[] uvs = new Vector2[len];
			Vector3 vec = Vector3.zero;

			switch(projectionAxis)
			{
				case ProjectionAxis.X:
				case ProjectionAxis.XNegative:
					vec = Vector3.up;
					break;

				case ProjectionAxis.Y:
				case ProjectionAxis.YNegative:
					vec = Vector3.forward;
					break;

				case ProjectionAxis.Z:
				case ProjectionAxis.ZNegative:
					vec = Vector3.up;
					break;
			}

			/**
			 *	Assign vertices to UV coordinates
			 */
			Vector3 uAxis, vAxis;

			// get U axis
			uAxis = Vector3.Cross(planeNormal, vec);
			uAxis.Normalize();

			// calculate V axis relative to U
			vAxis = Vector3.Cross(uAxis, planeNormal);
			vAxis.Normalize();

			for(int i = 0; i < len; i++)
			{
				int x = indices != null ? indices[i] : i;
				float u, v;

				u = Vector3.Dot(uAxis, verts[x]);
				v = Vector3.Dot(vAxis, verts[x]);

				uvs[i] = new Vector2(u, v);
			}

			return uvs;
		}

		internal static void PlanarProject(Vector3[] verts, Vector2[] uvs, int[] indices, Vector3 planeNormal, ProjectionAxis projectionAxis)
		{
			Vector3 vec;

			switch(projectionAxis)
			{
				case ProjectionAxis.X:
				case ProjectionAxis.XNegative:
					vec = Vector3.up;
					break;

				case ProjectionAxis.Y:
				case ProjectionAxis.YNegative:
					vec = Vector3.forward;
					break;

				case ProjectionAxis.Z:
				case ProjectionAxis.ZNegative:
					vec = Vector3.up;
					break;

				default:
					vec = Vector3.up;
					break;
			}

			// get U axis
			Math.Cross(planeNormal, vec, ref s_UAxis.x, ref s_UAxis.y, ref s_UAxis.z);
			s_UAxis.Normalize();

			// calculate V axis relative to U
			Math.Cross(s_UAxis, planeNormal, ref s_VAxis.x, ref s_VAxis.y, ref s_VAxis.z);
			s_VAxis.Normalize();

			int len = indices.Length;

			for(int i = 0; i < len; i++)
			{
				int x = indices[i];

				uvs[x].x = Vector3.Dot(s_UAxis, verts[x]);
				uvs[x].y = Vector3.Dot(s_VAxis, verts[x]);
			}
		}

		internal static Vector2[] SphericalProject(IList<Vector3> vertices, IList<int> indices = null)
		{
			int len = indices == null ? vertices.Count : indices.Count;
			Vector2[] uv = new Vector2[len];
			Vector3 cen = Math.Average(vertices, indices);

			for(int i = 0; i < len; i++)
			{
				int indx = indices == null ? i : indices[i];
				Vector3 p = (vertices[indx] - cen);
				p.Normalize();
				uv[i].x = .5f + (Mathf.Atan2(p.z, p.x) / (2f * Mathf.PI));
				uv[i].y = .5f - (Mathf.Asin(p.y) / Mathf.PI);
			}

			return uv;
		}

		/// <summary>
		/// Returns a new set of points wound as a contour counter-clockwise.
		/// </summary>
		/// <param name="verts"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		internal static IList<Vector2> Sort(IList<Vector2> verts, SortMethod method = SortMethod.CounterClockwise)
		{
			Vector2 cen = Math.Average(verts);
			Vector2 up = Vector2.up;
			int count = verts.Count;

			List<SimpleTuple<float, Vector2>> angles = new List<SimpleTuple<float, Vector2>>(count);

			for(int i = 0; i < count; i++)
				angles.Add(new SimpleTuple<float, Vector2>(Math.SignedAngle(up, verts[i] - cen), verts[i]));

			angles.Sort((a, b) => { return a.item1 < b.item1 ? -1 : 1; });

			IList<Vector2> values = angles.Select(x => x.item2).ToList();

			if(method == SortMethod.Clockwise)
				values = values.Reverse().ToList();

			return values;
		}

		/// <summary>
		/// Given a ProjectionAxis, return  the appropriate Vector3 conversion.
		/// </summary>
		/// <param name="axis"></param>
		/// <returns></returns>
		internal static Vector3 ProjectionAxisToVector(ProjectionAxis axis)
		{
			switch(axis)
			{
				case ProjectionAxis.X:
					return Vector3.right;

				case ProjectionAxis.Y:
					return Vector3.up;

				case ProjectionAxis.Z:
					return Vector3.forward;

				case ProjectionAxis.XNegative:
					return -Vector3.right;

				case ProjectionAxis.YNegative:
					return -Vector3.up;

				case ProjectionAxis.ZNegative:
					return -Vector3.forward;

				default:
					return Vector3.zero;
			}
		}

		/// <summary>
		/// Returns a projection axis based on which axis is the largest
		/// </summary>
		/// <param name="plane"></param>
		/// <returns></returns>
		internal static ProjectionAxis VectorToProjectionAxis(Vector3 plane)
		{
			if(Mathf.Abs(plane.x) > Mathf.Abs(plane.y) && Mathf.Abs(plane.x) > Mathf.Abs(plane.z))
			{
				return plane.x > 0 ? ProjectionAxis.X : ProjectionAxis.XNegative;
			}
			else
			{
				if(Mathf.Abs(plane.y) > Mathf.Abs(plane.z))
					return plane.y > 0 ? ProjectionAxis.Y : ProjectionAxis.YNegative;
				else
					return plane.z > 0 ? ProjectionAxis.Z : ProjectionAxis.ZNegative;
			}
		}

		/// <summary>
		/// Find a plane that best fits a set of 3d points.
		/// </summary>
		/// <remarks>http://www.ilikebigbits.com/blog/2015/3/2/plane-from-points</remarks>
		/// <param name="points"></param>
		/// <param name="selector"></param>
		/// <param name="indices"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static Plane FindBestPlane<T>(IList<T> points, System.Func<T, Vector3> selector, IList<int> indices = null)
		{
			float 	xx = 0f, xy = 0f, xz = 0f,
					yy = 0f, yz = 0f, zz = 0f;

			bool ind = indices != null && indices.Count > 0;
			int len = ind ? indices.Count : points.Count;
			Vector3 c = Math.Average(points, selector, indices);

			for(int i = 0; i < len; i++)
			{
				Vector3 r = selector(points[ ind ? indices[i] : i ]) - c;

				xx += r.x * r.x;
				xy += r.x * r.y;
				xz += r.x * r.z;
				yy += r.y * r.y;
				yz += r.y * r.z;
				zz += r.z * r.z;
			}

			float det_x = yy * zz - yz * yz;
			float det_y = xx * zz - xz * xz;
			float det_z = xx * yy - xy * xy;

			Vector3 n;

			if(det_x > det_y && det_x > det_z)
				n = new Vector3(1f, (xz*yz - xy*zz) / det_x, (xy*yz - xz*yy) / det_x);
			else if(det_y > det_z)
				n = new Vector3((yz*xz - xy*zz) / det_y, 1f, (xy*xz - yz*xx) / det_y);
			else
				n = new Vector3((yz*xy - xz*yy) / det_z, (xz*xy - yz*xx) / det_z, 1f);

			n.Normalize();

			return new Plane(n, c);
		}

		/// <summary>
		/// Find a plane that best fits a set of 3d points.
		/// </summary>
		/// <remarks>http://www.ilikebigbits.com/blog/2015/3/2/plane-from-points</remarks>
		/// <param name="points">The points to find a plane for. Order does not matter.</param>
		/// <param name="indexes">If provided, only the vertices referenced by the indexes array will be considered.</param>
		/// <returns>A plane that best matches the layout of the points array.</returns>
		public static Plane FindBestPlane(Vector3[] points, int[] indexes = null)
		{
			float 	xx = 0f, xy = 0f, xz = 0f,
					yy = 0f, yz = 0f, zz = 0f;

            if (points == null)
                throw new System.ArgumentNullException("points");

			bool ind = indexes != null && indexes.Length > 0;
			int len = ind ? indexes.Length : points.Length;

			Vector3 c = Vector3.zero, n = Vector3.zero;

			for(int i = 0; i < len; i++)
			{
				c.x += points[ind ? indexes[i] : i].x;
				c.y += points[ind ? indexes[i] : i].y;
				c.z += points[ind ? indexes[i] : i].z;
			}

			c.x /= (float) len;
			c.y /= (float) len;
			c.z /= (float) len;

			for(int i = 0; i < len; i++)
			{
				Vector3 r = points[ ind ? indexes[i] : i ] - c;

				xx += r.x * r.x;
				xy += r.x * r.y;
				xz += r.x * r.z;
				yy += r.y * r.y;
				yz += r.y * r.z;
				zz += r.z * r.z;
			}

			float det_x = yy * zz - yz * yz;
			float det_y = xx * zz - xz * xz;
			float det_z = xx * yy - xy * xy;

			if(det_x > det_y && det_x > det_z)
			{
				n.x = 1f;
				n.y = (xz*yz - xy*zz) / det_x;
				n.z = (xy*yz - xz*yy) / det_x;
			}
			else if(det_y > det_z)
			{
				n.x = (yz*xz - xy*zz) / det_y;
				n.y = 1f;
				n.z = (xy*xz - yz*xx) / det_y;
			}
			else
			{
				n.x = (yz*xy - xz*yy) / det_z;
				n.y = (xz*xy - yz*xx) / det_z;
				n.z = 1f;
			}

			n.Normalize();

			return new Plane(n, c);
		}
	}
}
