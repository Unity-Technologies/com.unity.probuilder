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
			Vector2[] uvs = new Vector2[len];
			Vector3 vec = Vector3.zero;
			Vector3 normal = pb_Math.Normal(vertices, indices);
			ProjectionAxis axis = VectorToProjectionAxis(normal);

			switch(axis)
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
			
			Vector3 uAxis = Vector3.Cross(normal, vec);
			uAxis.Normalize();
			Vector3 vAxis = Vector3.Cross(uAxis, normal);
			vAxis.Normalize();
			
			for(int i = 0; i < len; i++)
				uvs[i] = new Vector2(
					Vector3.Dot(uAxis, vertices[indices[i]].position),
					Vector3.Dot(vAxis, vertices[indices[i]].position));

			return uvs;
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
	}
}
