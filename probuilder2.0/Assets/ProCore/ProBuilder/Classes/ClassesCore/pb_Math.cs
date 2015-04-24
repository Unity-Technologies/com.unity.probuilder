using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Math
{

	/**
	 * Geometry math and Array extensions.
	 */
	public static class pb_Math
	{
		public const float PHI = 1.618033988749895f;

#region Geometry

		// implementation snagged from: http://stackoverflow.com/questions/839899/how-do-i-calculate-a-point-on-a-circles-circumference
		public static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
		{
			// Convert from degrees to radians via multiplication by PI/180        
			float x = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * angleInDegrees)) + origin.x;
			float y = (float)(radius * Mathf.Sin( Mathf.Deg2Rad * angleInDegrees)) + origin.y;

			return new Vector2(x, y);
		}

		/**
		 * Provided a radius, latitudinal and longitudinal angle, return a position. 
		 */
		public static Vector3 PointInSphere(float radius, float latitudeAngle, float longitudeAngle)
		{
			float x = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * latitudeAngle) * Mathf.Sin( Mathf.Deg2Rad * longitudeAngle));
			float y = (float)(radius * Mathf.Sin( Mathf.Deg2Rad * latitudeAngle) * Mathf.Sin( Mathf.Deg2Rad * longitudeAngle));
			float z = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * longitudeAngle));
			
			// round numbers to 2nd decimal
			x = (float)System.Math.Round(x, 2);
			y = (float)System.Math.Round(y, 2);
			z = (float)System.Math.Round(z, 2);

			return new Vector3(x, y, z);
		}

		/**
		 * Returns the Area of a triangle.
		 */
		public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
		{
			float da = Vector3.Distance(a, b);
			float db = Vector3.Distance(b, c);
			float dc = Vector3.Distance(c, a);
			float p = (da + db + dc) / 2f;

			return Mathf.Sqrt( p*(p-da)*(p-db)*(p-dc) );

		}

		/**
		 * Returns a new point by rotating the Vector2 around an origin point.
		 * @param v this - Vector2 original point.
		 * @param origin The origin point to use as a pivot point.
		 * @param theta Angle to rotate in Degrees.
		 */
		public static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
		{
			float cx = origin.x, cy = origin.y;	// origin
			float px = v.x, py = v.y;			// point

			float s = Mathf.Sin(theta * Mathf.Deg2Rad);
			float c = Mathf.Cos(theta * Mathf.Deg2Rad);

			// translate point back to origin:
			px -= cx;
			py -= cy;

			// rotate point
			float xnew = px * c + py * s;
			float ynew = -px * s + py * c;

			// translate point back:
			px = xnew + cx;
			py = ynew + cy;
			
			return new Vector2(px, py);
		}

		/**
		 * Scales a Vector2 using origin as the pivot point.
		 */
		public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
		{
			Vector2 tp = v-origin;
			tp = Vector2.Scale(tp, scale);
			tp += origin;
			
			return tp;
		}

		/**
		 *	Return the perpindicular direction to a 2d line
		 */
		public static Vector2 Perpendicular(Vector2 a, Vector2 b)
		{
			float x = a.x;
			float y = a.y;

			float x2 = b.x;
			float y2 = b.y;

			return new Vector2( -(y2-y), x2-x ).normalized;
		}

		/**
		 *	Return the perpindicular direction to a unit vector
		 */
		public static Vector2 Perpendicular(Vector2 a)
		{
			return new Vector2(-a.y, a.x).normalized;
		}

		/**
		 * Reflects a point @point across line @a @b
		 */
		public static Vector2 ReflectPoint(Vector2 point, Vector2 a, Vector2 b)
		{
			Vector2 line = b-a;
			Vector2 perp = new Vector2(-line.y, line.x);	// skip normalize

			float dist = Mathf.Sin( Vector2.Angle(line, point-a) * Mathf.Deg2Rad ) * Vector2.Distance(point, a);

			return point + perp * (dist * 2f) * (Vector2.Dot(point-a, perp) > 0 ? -1f : 1f);
		}

		/**
		 * 	Get the distance between a point and a finite line segment.
		 * http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
		 */
		public static float DistancePointLineSegment(Vector2 p, Vector2 v, Vector2 w)
		{
			// lineStart = v 
			// lineEnd = w
			// point = p

			// Return minimum distance between line segment vw and point p
			float l2 = ((v.x - w.x)*(v.x - w.x)) + ((v.y - w.y)*(v.y - w.y));  // i.e. |w-v|^2 -  avoid a sqrt
			
			if (l2 == 0.0f) return Vector2.Distance(p, v);   // v == w case
			
			// Consider the line extending the segment, parameterized as v + t (w - v).
			// We find projection of point p onto the line. 
			// It falls where t = [(p-v) . (w-v)] / |w-v|^2
			float t = Vector2.Dot(p - v, w - v) / l2;

			if (t < 0.0)
				return Vector2.Distance(p, v);       		// Beyond the 'v' end of the segment
			else if (t > 1.0) 
				return Vector2.Distance(p, w);  			// Beyond the 'w' end of the segment
			
			Vector2 projection = v + t * (w - v);  	// Projection falls on the segment
			
			return Vector2.Distance(p, projection);	
		}

		// http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
		// Returns 1 if the lines intersect, otherwise 0. In addition, if the lines 
		// intersect the intersection point may be stored in the intersect var
		public static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 intersect)
		{
			intersect = Vector2.zero;
			Vector2 s1, s2;
			s1.x = p1.x - p0.x;     s1.y = p1.y - p0.y;
			s2.x = p3.x - p2.x;     s2.y = p3.y - p2.y;

			float s, t;
			s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / (-s2.x * s1.y + s1.x * s2.y);
			t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

			if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
			{
				// Collision detected
				intersect.x = p0.x + (t * s1.x);
				intersect.y = p0.y + (t * s1.y);
				return true;
			}

			return false;
		}		

		/**
		 * True or false lines intersect.
		 */
		public static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			Vector2 s1, s2;
			s1.x = p1.x - p0.x;     s1.y = p1.y - p0.y;
			s2.x = p3.x - p2.x;     s2.y = p3.y - p2.y;

			float s, t;
			s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / (-s2.x * s1.y + s1.x * s2.y);
			t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

			return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
		}

		/**
		 * Returns true if the polygon contains point.  False otherwise.
		 * Casts a ray from outside the bounds to the polygon and checks how 
		 * many edges are hit.
		 * @param polygon A series of individual edges composing a polygon.  polygon length *must* be divisible by 2.
		 */
		public static bool PointInPolygon(Vector2[] polygon, Vector2 point)
		{
			pb_Bounds2D bounds = new pb_Bounds2D(polygon);

			if(bounds.ContainsPoint(point))
			{
				Vector2 rayStart = bounds.center + Vector2.up * (bounds.size.y+2f);
				int collisions = 0;

				for(int i = 0; i < polygon.Length; i += 2)
				{
					if( GetLineSegmentIntersect(rayStart, point, polygon[i], polygon[i+1]) )
						collisions++;
				}
		
				return collisions % 2 != 0;
			}
			else
				return false;
		}

		/**
		 * Returns true if a raycast intersects a triangle.
		 * http://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
		 * http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
		 */
		public static bool RayIntersectsTriangle(Ray InRay, Vector3 InTriangleA,  Vector3 InTriangleB,  Vector3 InTriangleC, out float OutDistance, out Vector3 OutPoint)
		{
			OutDistance = 0f;
			OutPoint = Vector3.zero;
			
			Vector3 e1, e2;  //Edge1, Edge2
			Vector3 P, Q, T;
			float det, inv_det, u, v;
			float t;

			//Find vectors for two edges sharing V1
			e1 = InTriangleB - InTriangleA;
			e2 = InTriangleC - InTriangleA;

			//Begin calculating determinant - also used to calculate `u` parameter
			P = Vector3.Cross(InRay.direction, e2);
			
			//if determinant is near zero, ray lies in plane of triangle
			det = Vector3.Dot(e1, P);

			// Non-culling branch
			// {
				if(det > -Mathf.Epsilon && det < Mathf.Epsilon)
					return false;

				inv_det = 1f / det;

				//calculate distance from V1 to ray origin
				T = InRay.origin - InTriangleA;

				// Calculate u parameter and test bound
				u = Vector3.Dot(T, P) * inv_det;

				//The intersection lies outside of the triangle
				if(u < 0f || u > 1f)
					return false;

				//Prepare to test v parameter
				Q = Vector3.Cross(T, e1);

				//Calculate V parameter and test bound
				v = Vector3.Dot(InRay.direction, Q) * inv_det;

				//The intersection lies outside of the triangle
				if(v < 0f || u + v  > 1f)
					return false;

				t = Vector3.Dot(e2, Q) * inv_det;
			// }

			if(t > Mathf.Epsilon)
			{ 
				//ray intersection
				OutDistance = t;

				OutPoint.x = (u * InTriangleB.x + v * InTriangleC.x + (1-(u+v)) * InTriangleA.x);
				OutPoint.y = (u * InTriangleB.y + v * InTriangleC.y + (1-(u+v)) * InTriangleA.y);
				OutPoint.z = (u * InTriangleB.z + v * InTriangleC.z + (1-(u+v)) * InTriangleA.z);

				return true;
			}

			return false;
		}
#endregion

#region Normal and Tangents

		/**
		 * Calculate the unit vector normal of 3 points:  B-A x C-A
		 */
		public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
			{
				return cross.normalized;
			}
		}

		/**
		 * Finds the normal of each triangle in a face and returns the average.
		 */
		public static Vector3 Normal(pb_Object pb, pb_Face face)
		{
			Vector3 nrm = Vector3.zero;

			for(int i = 0; i < face.indices.Length; i+=3)
				nrm += Normal(	pb.vertices[face.indices[i+0]], 
								pb.vertices[face.indices[i+1]], 
								pb.vertices[face.indices[i+2]]);

			return nrm / (face.indices.Length/3f);
		}

		/**
		 * Returns the first normal, tangent, and bitangent for this face, using the first triangle available for tangent and bitangent.
		 * Does not rely on pb.msh for normal or uv information - uses pb.vertices & pb.uv.
		 */
		public static void NormalTangentBitangent(pb_Object pb, pb_Face face, out Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
		{			
			if(face.indices.Length < 3)
			{
				Debug.LogWarning("Cannot find normal / tangent / bitangent for face with < 3 indices.");
				normal = Vector3.zero;
				tangent = Vector3.zero;
				bitangent = Vector3.zero;
				return;
			}

			normal = pb_Math.Normal(pb, face);

			Vector3 tan1 = Vector3.zero;
			Vector3 tan2 = Vector3.zero;
			Vector4 tan = new Vector4(0f,0f,0f,1f);

			long i1 = face.indices[0];
			long i2 = face.indices[1];
			long i3 = face.indices[2];

			Vector3 v1 = pb.vertices[i1];
			Vector3 v2 = pb.vertices[i2];
			Vector3 v3 = pb.vertices[i3];

			Vector2 w1 = pb.uv[i1];
			Vector2 w2 = pb.uv[i2];
			Vector2 w3 = pb.uv[i3];

			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;

			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;

			float r = 1.0f / (s1 * t2 - s2 * t1);

			Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1 += sdir;
			tan2 += tdir;

			Vector3 n = normal;

			Vector3.OrthoNormalize(ref n, ref tan1);

			tan.x = tan1.x;
			tan.y = tan1.y;
			tan.z = tan1.z;

			tan.w = (Vector3.Dot(Vector3.Cross(n, tan1), tan2) < 0.0f) ? -1.0f : 1.0f;

			tangent = ((Vector3)tan) * tan.w;
			bitangent = Vector3.Cross(normal, tangent);
		}

		/**
		 * If p.Length % 3 == 0, finds the normal of each triangle in a face and returns the average.
		 * Otherwise return the normal of the first three points.
		 */
		public static Vector3 Normal(Vector3[] p)
		{
			if(p.Length % 3 == 0)
			{
				Vector3 nrm = Vector3.zero;

				for(int i = 0; i < p.Length; i+=3)
					nrm += Normal(	p[i+0], 
									p[i+1], 
									p[i+2]);

				return nrm / (p.Length/3f);
			}
			else
			{
				Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
				if (cross.magnitude < Mathf.Epsilon)
					return new Vector3(0f, 0f, 0f); // bad triangle
				else
				{
					return cross.normalized;
				}
			}
		}

		public static Vector3 Normal(List<Vector3> p)
		{
			Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
			{
				return cross.normalized;
			}
		}
#endregion

#region Compare (Max, Min, Average, etc)

		public static T Max<T>(T[] array) where T : System.IComparable<T>
		{
			if(array == null || array.Length < 1)
				return default(T);

			T max = array[0];
			for(int i = 1; i < array.Length; i++)
				if(array[i].CompareTo(max) >= 0)
					max = array[i];
			return max;
		}

		public static T Min<T>(T[] array) where T : System.IComparable<T>
		{
			if(array == null || array.Length < 1)
				return default(T);

			T min = array[0];
			for(int i = 1; i < array.Length; i++)
				if(array[i].CompareTo(min) < 0)
					min = array[i];
			return min;
		}

		/**
		 * Return the largest axis in a Vector3.
		 */
		public static float LargestValue(Vector3 v)
		{
			if(v.x > v.y && v.x > v.z) return v.x;
			if(v.y > v.x && v.y > v.z) return v.y;
			return v.z;
		}
		
		/**
		 * Return the largest axis in a Vector2.
		 */
		public static float LargestValue(Vector2 v)
		{
			return (v.x > v.y) ? v.x :v.y;
		}

		/**
		 * The smallest X and Y value found in an array of Vector2.  May or may not belong to the same Vector2.
		 */
		public static Vector2 SmallestVector2(Vector2[] v)
		{
			Vector2 s = v[0];
			for(int i = 1; i < v.Length; i++)
			{
				if(v[i].x < s.x)
					s.x = v[i].x;
				if(v[i].y < s.y)
					s.y = v[i].y;
			}
			return s;
		}

		/**
		 * The largest X and Y value in an array.  May or may not belong to the same Vector2.
		 */
		public static Vector2 LargestVector2(Vector2[] v)
		{
			Vector2 l = v[0];
			for(int i = 0; i < v.Length; i++)
			{
				if(v[i].x > l.x)
					l.x = v[i].x;
				if(v[i].y > l.y)
					l.y = v[i].y;
			}
			return l;
		}

		/**
		 * Creates an AABB with vertices and returns the Center point.
		 */
		public static Vector3 BoundsCenter(Vector3[] verts)
		{
			if( verts.Length < 1 ) return Vector3.zero;

			Vector3 min = verts[0];
			Vector3 max = min;

			for(int i = 1; i < verts.Length; i++)
			{
				min.x = Mathf.Min(verts[i].x, min.x);
				max.x = Mathf.Max(verts[i].x, max.x);

				min.y = Mathf.Min(verts[i].y, min.y);
				max.y = Mathf.Max(verts[i].y, max.y);

				min.z = Mathf.Min(verts[i].z, min.z);
				max.z = Mathf.Max(verts[i].z, max.z);
			}

			return (min+max) * .5f;
		}

		/**
		 *	\brief Gets the center point of the supplied Vector3[] array.
		 *	\returns Average Vector3 of passed vertex array.
		 */
		public static Vector3 Average(List<Vector3> v)
		{
			Vector3 sum = Vector3.zero;
			for(int i = 0; i < v.Count; i++)
				sum += v[i];
			return sum/(float)v.Count;
		}

		public static Vector3 Average(Vector3[] v)
		{
			Vector3 sum = Vector3.zero;
			for(int i = 0; i < v.Length; i++)
				sum += v[i];
			return sum/(float)v.Length;
		}

		public static Vector2 Average(List<Vector2> v)
		{
			Vector2 sum = Vector2.zero;
			for(int i = 0; i < v.Count; i++)
				sum += v[i];
			return sum/(float)v.Count;
		}

		public static Vector2 Average(Vector2[] v)
		{
			Vector2 sum = Vector2.zero;
			for(int i = 0; i < v.Length; i++)
				sum += v[i];
			return sum/(float)v.Length;
		}

		public static Vector4 Average(List<Vector4> v)
		{
			Vector4 sum = Vector4.zero;
			for(int i = 0; i < v.Count; i++)
				sum += v[i];
			return sum/(float)v.Count;
		}

		public static Vector4 Average(Vector4[] v)
		{
			Vector4 sum = Vector4.zero;
			for(int i = 0; i < v.Length; i++)
				sum += v[i];
			return sum/(float)v.Length;
		}

		public static Color Average(Color[] Array)
		{
			Color sum = Array[0];

			for(int i = 1; i < Array.Length; i++)
				sum += Array[i];

			return sum / (float)Array.Length;
		}

		/**
		 *	\brief Compares 2 vector3 objects, allowing for a margin of error.
		 */
		public static bool Approx(this Vector3 v, Vector3 b, float delta)
		{
			return 
				Mathf.Abs(v.x - b.x) < delta &&
				Mathf.Abs(v.y - b.y) < delta &&
				Mathf.Abs(v.z - b.z) < delta;
		}

		/**
		 *	\brief Compares 2 vector3 objects, allowing for a margin of error.
		 */
		public static bool Approx(this Vector2 v, Vector2 b, float delta)
		{
			return 
				Mathf.Abs(v.x - b.x) < delta &&
				Mathf.Abs(v.y - b.y) < delta;
		}

		/**
		 *	\brief Compares 2 color objects, allowing for a margin of error.
		 */
		public static bool Approx(this Color a, Color b, float delta)
		{
			return 	Mathf.Abs(a.r - b.r) < delta &&
					Mathf.Abs(a.g - b.g) < delta &&
					Mathf.Abs(a.b - b.b) < delta &&
					Mathf.Abs(a.a - b.a) < delta;
		}
#endregion

#region Projection 3d -> 2d

	/**
	 * Maps an array of 3d points to 2d space given the vertices' normal.
	 * Optionally provides an override to set ProjectionAxis.
	 * u = Vector3.Dot( Vector3.Cross(planeNormal, tangentApproximation), verts[i] );
	 * v = Vector3.Dot( Vector3.Cross(uDirection, planeNormal), verts[i] );
	 * \sa GetProjectionAxis()
	 */
	public static Vector2[] PlanarProject(Vector3[] verts, Vector3 planeNormal)
	{
		return PlanarProject(verts, planeNormal, pb_Math.VectorToProjectionAxis(planeNormal));
	}

	public static Vector2[] PlanarProject(Vector3[] verts, Vector3 planeNormal, ProjectionAxis projectionAxis)
	{
		Vector2[] uvs = new Vector2[verts.Length];
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
			
				// vec = -Vector3.forward;
				// break;
			
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
		
		for(int i = 0; i < verts.Length; i++)
		{
			float u, v;

			u = Vector3.Dot(uAxis, verts[i]);
			v = Vector3.Dot(vAxis, verts[i]);

			uvs[i] = new Vector2(u, v);
		}

		return uvs;
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

	public static Vector2 ToMask(this Vector2 vec)
	{
		return new Vector2(
			Mathf.Abs(vec.x) > .0001f ? 1f : 0f,
			Mathf.Abs(vec.y) > .0001f ? 1f : 0f
			);
	}

	public static Vector3 ToMask(this Vector3 vec)
	{
		return new Vector3(
			Mathf.Abs(vec.x) > .0001f ? 1f : 0f,
			Mathf.Abs(vec.y) > .0001f ? 1f : 0f,
			Mathf.Abs(vec.z) > .0001f ? 1f : 0f
			);
	}
#endregion
	}
}