using UnityEngine;
using System.Linq;	// List.Reverse
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public static class pb_Projection
	{
		/**
		 * Maps an array of 3d points to 2d space given the vertices' normal.
		 * Optionally provides an override to set ProjectionAxis.
		 * u = Vector3.Dot( Vector3.Cross(planeNormal, tangentApproximation), verts[i] );
		 * v = Vector3.Dot( Vector3.Cross(uDirection, planeNormal), verts[i] );
		 * \sa GetProjectionAxis()
		 */
		public static Vector2[] PlanarProject(IList<Vector3> verts, Vector3 planeNormal)
		{
			return PlanarProject(verts, planeNormal, VectorToProjectionAxis(planeNormal));
		}

		public static Vector2[] PlanarProject(pb_Object pb, pb_Face face)
		{
			Vector3 normal = pb_Math.Normal(pb, face);
			return PlanarProject(pb.vertices, normal, VectorToProjectionAxis(normal), face.indices);
		}

		public static Vector2[] PlanarProject(IList<Vector3> verts, Vector3 planeNormal, ProjectionAxis projectionAxis, IList<int> indices = null)
		{
			int len = indices == null || indices.Count < 1 ? verts.Count : indices.Count;
			Vector2[] uvs = new Vector2[len];
			Vector3 vec = Vector3.zero;

			switch(projectionAxis)
			{
				case ProjectionAxis.X:
				case ProjectionAxis.X_Negative:
					vec = Vector3.up;
					break;

				case ProjectionAxis.Y:
				case ProjectionAxis.Y_Negative:
					vec = Vector3.forward;
					break;
				
				case ProjectionAxis.Z:
				case ProjectionAxis.Z_Negative:
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

		public static Vector2[] PlanarProject(IList<pb_Vertex> vertices, IList<int> indices)
		{
			int len = indices.Count;

			Vector3[] v = new Vector3[len];

			for(int i = 0; i < len; i++)
				v[i] = vertices[indices[i]].position;

			Vector3 normal = pb_Math.Normal(vertices, indices);
			ProjectionAxis axis = VectorToProjectionAxis(normal);

			return PlanarProject(v, normal, axis, null);
		}

		public static Vector2[] SphericalProject(IList<Vector3> vertices, IList<int> indices = null)
		{
			int len = indices == null ? vertices.Count : indices.Count;
			Vector2[] uv = new Vector2[len];
			Vector3 cen = pb_Math.Average(vertices, indices);

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

		/**
		 *	Returns a new set of points wound as a contour counter-clockwise.
		 */
		public static IList<Vector2> Sort(IList<Vector2> verts, SortMethod method = SortMethod.CounterClockwise)
		{
			Vector2 cen = pb_Math.Average(verts);
			Vector2 up = Vector2.up;
			int count = verts.Count;

			List<pb_Tuple<float, Vector2>> angles = new List<pb_Tuple<float, Vector2>>(count);

			for(int i = 0; i < count; i++)
				angles.Add(new pb_Tuple<float, Vector2>(pb_Math.SignedAngle(up, verts[i] - cen), verts[i]));

			angles.Sort((a, b) => { return a.Item1 < b.Item1 ? -1 : 1; });
			
			IList<Vector2> values = angles.Select(x => x.Item2).ToList();

			if(method == SortMethod.Clockwise)
				values.Reverse();

			return values;
		}

		/**
		 * Given a ProjectionAxis, return  the appropriate Vector3 conversion.
		 */
		public static Vector3 ProjectionAxisToVector(ProjectionAxis axis)
		{
			switch(axis)
			{
				case ProjectionAxis.X:
					return Vector3.right;

				case ProjectionAxis.Y:
					return Vector3.up;

				case ProjectionAxis.Z:
					return Vector3.forward;

				case ProjectionAxis.X_Negative:
					return -Vector3.right;

				case ProjectionAxis.Y_Negative:
					return -Vector3.up;

				case ProjectionAxis.Z_Negative:
					return -Vector3.forward;

				default:
					return Vector3.zero;
			}
		}

		/*
		 *	Returns a projection axis based on which axis is the largest
		 */
		public static ProjectionAxis VectorToProjectionAxis(Vector3 plane)
		{
			if(Mathf.Abs(plane.x) > Mathf.Abs(plane.y) && Mathf.Abs(plane.x) > Mathf.Abs(plane.z))
			{
				return plane.x > 0 ? ProjectionAxis.X : ProjectionAxis.X_Negative;
			}
			else
			{ 
				if(Mathf.Abs(plane.y) > Mathf.Abs(plane.z))
					return plane.y > 0 ? ProjectionAxis.Y : ProjectionAxis.Y_Negative;
				else
					return plane.z > 0 ? ProjectionAxis.Z : ProjectionAxis.Z_Negative;
			}
		}

		/**
		 *	Find a plane that best fits a set of 3d points.
		 *	
		 *	http://www.ilikebigbits.com/blog/2015/3/2/plane-from-points
		 */
		public static Plane FindBestPlane<T>(IList<T> points, System.Func<T, Vector3> selector, IList<int> indices = null)
		{
			float 	xx = 0f, xy = 0f, xz = 0f,
					yy = 0f, yz = 0f, zz = 0f;

			bool ind = indices != null && indices.Count > 0;
			int len = ind ? indices.Count : points.Count;
			Vector3 c = pb_Math.Average(points, selector, indices);

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

		/**
		 *	Find a plane that best fits a set of 3d points.
		 *	
		 *	http://www.ilikebigbits.com/blog/2015/3/2/plane-from-points
		 */
		public static Plane FindBestPlane(IList<Vector3> points, IList<int> indices = null)
		{
			float 	xx = 0f, xy = 0f, xz = 0f,
					yy = 0f, yz = 0f, zz = 0f;

			bool ind = indices != null && indices.Count > 0;
			int len = ind ? indices.Count : points.Count;

			Vector3 c = pb_Math.Average(points, indices);

			for(int i = 0; i < len; i++)
			{
				Vector3 r = points[ ind ? indices[i] : i ] - c;

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

		/**
		 *	Find a plane that best fits a set of 3d points.
		 *	
		 *	http://www.ilikebigbits.com/blog/2015/3/2/plane-from-points
		 */
		public static Plane FindBestPlane(IList<Vector3> points, IList<pb_Face> faces)
		{
			float 	xx = 0f, xy = 0f, xz = 0f,
					yy = 0f, yz = 0f, zz = 0f;

			int len = 0;
			Vector3 c = Vector3.zero, v = Vector3.zero;

			for(int i = 0; i < faces.Count; i++)
			{
				int size = faces[i].distinctIndices.Length;
				len += size;

				for(int n = 0; n < size; n++)
				{
					v = points[faces[i].distinctIndices[n]];
					c.x += v.x;
					c.y += v.y;
					c.z += v.z;
				}
			}

			c.x /= (float) len;
			c.y /= (float) len;
			c.z /= (float) len;

			for(int i = 0; i < faces.Count; i++)
			{
				int size = faces[i].distinctIndices.Length;

				for(int n = 0; n < size; n++)
				{
					v = points[faces[i].distinctIndices[n]];

					v.x -= c.x;
					v.y -= c.y;
					v.z -= c.z;

					xx += v.x * v.x;
					xy += v.x * v.y;
					xz += v.x * v.z;
					yy += v.y * v.y;
					yz += v.y * v.z;
					zz += v.z * v.z;
				}
			}

			float det_x = yy * zz - yz * yz;
			float det_y = xx * zz - xz * xz;
			float det_z = xx * yy - xy * xy;

			if(det_x > det_y && det_x > det_z)
			{
				v.x = 1f;
				v.y = (xz*yz - xy*zz) / det_x;
				v.z = (xy*yz - xz*yy) / det_x;
			}
			else if(det_y > det_z)
			{
				v.x = (yz*xz - xy*zz) / det_y;
				v.y = 1f;
				v.z = (xy*xz - yz*xx) / det_y;
			}
			else
			{
				v.x = (yz*xy - xz*yy) / det_z;
				v.y = (xz*xy - yz*xx) / det_z;
				v.z = 1f;
			}

			v.Normalize();

			return new Plane(v, c);
		}
	}
}
