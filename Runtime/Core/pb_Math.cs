using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder.Core;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// Additional commonly used math when working with 3d geometry.
	/// </summary>
	public static class pb_Math
	{
		/// <summary>
		/// Pi / 2.
		/// </summary>
		public const float PHI = 1.618033988749895f;

		/// <summary>
		/// ProBuilder epsilon constant.
		/// </summary>
		public const float FLT_EPSILON = float.Epsilon;

		/// <summary>
		/// Epsilon to use when comparing vertex positions for equality.
		/// </summary>
		public const float FLT_COMPARE_EPSILON = .0001f;

		/// <summary>
		/// The minimum distance a handle must move on an axis before considering that axis as engaged.
		/// </summary>
		public const float HANDLE_EPSILON = .0001f;

		/// <summary>
		/// Get a point on the circumference of a circle.
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="angleInDegrees"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
		{
			// Convert from degrees to radians via multiplication by PI/180
			float x = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * angleInDegrees)) + origin.x;
			float y = (float)(radius * Mathf.Sin( Mathf.Deg2Rad * angleInDegrees)) + origin.y;

			return new Vector2(x, y);
		}

		/// <summary>
		/// Provided a radius, latitudinal and longitudinal angle, return a position.
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="latitudeAngle"></param>
		/// <param name="longitudeAngle"></param>
		/// <returns></returns>
		internal static Vector3 PointInSphere(float radius, float latitudeAngle, float longitudeAngle)
		{
			float x = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * latitudeAngle) * Mathf.Sin( Mathf.Deg2Rad * longitudeAngle));
			float y = (float)(radius * Mathf.Sin( Mathf.Deg2Rad * latitudeAngle) * Mathf.Sin( Mathf.Deg2Rad * longitudeAngle));
			float z = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * longitudeAngle));

			return new Vector3(x, y, z);
		}

		/// <summary>
		/// Returns the signed angle from a to b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float SignedAngle(Vector2 a, Vector2 b)
		{
			float t = Vector2.Angle(a, b);
			if( b.x - a.x < 0 )
				t = 360f - t;
			return t;
		}

		/// <summary>
		/// Squared distance between two points. (b - a).sqrMagnitude.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float SqrDistance(Vector3 a, Vector3 b)
		{
			float dx = b.x - a.x,
				  dy = b.y - a.y,
				  dz = b.z - a.z;
			return dx * dx + dy * dy + dz * dz;
		}

		/// <summary>
		/// Get the area of a triangle.
		/// </summary>
		/// <remarks>http://www.iquilezles.org/blog/?p=1579</remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
		{
			float 	a = SqrDistance(x, y),
					b = SqrDistance(y, z),
					c = SqrDistance(z, x);

			return Mathf.Sqrt((2f*a*b + 2f*b*c + 2f*c*a - a*a - b*b - c*c) / 16f);
		}

		/// <summary>
		/// Returns the Area of a polygon.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static float PolygonArea(Vector3[] vertices, int[] indices)
		{
			float area = 0f;

			for(int i = 0; i < indices.Length; i += 3)
				area += TriangleArea(vertices[indices[i]], vertices[indices[i+1]], vertices[indices[i+2]]);

			return area;
		}

		/// <summary>
		/// Returns a new point by rotating the Vector2 around an origin point.
		/// </summary>
		/// <param name="v">Vector2 original point.</param>
		/// <param name="origin">The pivot to rotate around.</param>
		/// <param name="theta">How far to rotate in degrees.</param>
		/// <returns></returns>
		internal static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
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

		/// <summary>
		/// Scales a Vector2 using origin as the pivot point.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="origin"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
		{
			Vector2 tp = v-origin;
			tp = Vector2.Scale(tp, scale);
			tp += origin;

			return tp;
		}

		/// <summary>
		/// Return the perpindicular direction to a 2d line `Perpendicular(b - y)`
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>A normalized perpindicular direction.</returns>
		internal static Vector2 Perpendicular(Vector2 a, Vector2 b)
		{
			float x = a.x;
			float y = a.y;

			float x2 = b.x;
			float y2 = b.y;

			return new Vector2( -(y2-y), x2-x ).normalized;
		}

		/// <summary>
		/// Return the perpindicular direction to a unit vector.
		/// </summary>
		/// <param name="a"></param>
		/// <returns>Normalized perpindular direction.</returns>
		public static Vector2 Perpendicular(Vector2 a)
		{
			return new Vector2(-a.y, a.x).normalized;
		}

		/// <summary>
		/// Reflects a point @point across line @a -> @b
		/// </summary>
		/// <param name="point"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector2 ReflectPoint(Vector2 point, Vector2 a, Vector2 b)
		{
			Vector2 line = b-a;
			Vector2 perp = new Vector2(-line.y, line.x);	// skip normalize

			float dist = Mathf.Sin( Vector2.Angle(line, point-a) * Mathf.Deg2Rad ) * Vector2.Distance(point, a);

			return point + perp * (dist * 2f) * (Vector2.Dot(point-a, perp) > 0 ? -1f : 1f);
		}

		/// <summary>
		/// Get the distance between a point and a finite line segment.
		/// </summary>
		/// <remarks>http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment</remarks>
		/// <param name="p">The point.</param>
		/// <param name="v">Line start.</param>
		/// <param name="w">Line end.</param>
		/// <returns></returns>
		public static float DistancePointLineSegment(Vector2 p, Vector2 v, Vector2 w)
		{
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

		/// <summary>
		/// Get the distance between a point and a finite line segment.
		/// </summary>
		/// <param name="p">Point.</param>
		/// <param name="v">Line start.</param>
		/// <param name="w">Line end.</param>
		/// <returns></returns>
		public static float DistancePointLineSegment(Vector3 p, Vector3 v, Vector3 w)
		{
			// Return minimum distance between line segment vw and point p
			float l2 = ((v.x - w.x)*(v.x - w.x)) + ((v.y - w.y)*(v.y - w.y)) + ((v.z - w.z)*(v.z - w.z));  // i.e. |w-v|^2 -  avoid a sqrt

			if (l2 == 0.0f) return Vector3.Distance(p, v);   // v == w case

			// Consider the line extending the segment, parameterized as v + t (w - v).
			// We find projection of point p onto the line.
			// It falls where t = [(p-v) . (w-v)] / |w-v|^2
			float t = Vector3.Dot(p - v, w - v) / l2;

			if (t < 0.0)
				return Vector3.Distance(p, v);       		// Beyond the 'v' end of the segment
			else if (t > 1.0)
				return Vector3.Distance(p, w);  			// Beyond the 'w' end of the segment

			Vector3 projection = v + t * (w - v);  	// Projection falls on the segment

			return Vector3.Distance(p, projection);
		}

		/// <summary>
		/// Calculate the nearest point on ray A to ray B.
		/// </summary>
		/// <param name="ao">First ray origin.</param>
		/// <param name="ad">First ray direction.</param>
		/// <param name="bo">Second ray origin.</param>
		/// <param name="bd">Second ray direction.</param>
		/// <returns></returns>
		public static Vector3 GetNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
		{
			// ray doesn't do parallel
			if(ad == bd)
				return ao;

			Vector3 c = bo - ao;

			float n = -Vector3.Dot(ad, bd) * Vector3.Dot(bd, c) + Vector3.Dot(ad, c) * Vector3.Dot(bd, bd);
			float d = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - Vector3.Dot(ad, bd) * Vector3.Dot(ad, bd);

			return ao + ad * (n/d);
		}

		// http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
		// Returns 1 if the lines intersect, otherwise 0. In addition, if the lines
		// intersect the intersection point may be stored in the intersect var
		internal static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 intersect)
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

		/// <summary>
		/// True or false lines, do lines intersect.
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="p3"></param>
		/// <returns></returns>
		internal static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			Vector2 s1, s2;
			s1.x = p1.x - p0.x;     s1.y = p1.y - p0.y;
			s2.x = p3.x - p2.x;     s2.y = p3.y - p2.y;

			float s, t;
			s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / (-s2.x * s1.y + s1.x * s2.y);
			t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

			return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
		}

		/// <summary>
		/// Casts a ray from outside the bounds to the polygon and checks how many edges are hit.
		/// </summary>
		/// <param name="polygon">A series of individual edges composing a polygon.  polygon length *must* be divisible by 2.</param>
		/// <param name="point"></param>
		/// <param name="indices">If present these indices make up the border of polygon. If not, polygon is assumed to be in correct order.</param>
		/// <returns>True if the polygon contains point. False otherwise.</returns>
		internal static bool PointInPolygon(Vector2[] polygon, Vector2 point, int[] indices = null)
		{
			int len = indices != null ? indices.Length : polygon.Length;

			if(len % 2 != 0)
			{
				Debug.LogError("PointInPolygon requires polygon indices be divisible by 2!");
				return false;
			}

			pb_Bounds2D bounds = new pb_Bounds2D(polygon, indices);

			if(bounds.ContainsPoint(point))
			{
				Vector2 rayStart = bounds.center + Vector2.up * (bounds.size.y+2f);
				int collisions = 0;

				for(int i = 0; i < len; i += 2)
				{
					int a = indices != null ? indices[i] : i;
					int b = indices != null ? indices[i + 1] : i + 1;

					if( GetLineSegmentIntersect(rayStart, point, polygon[a], polygon[b]) )
						collisions++;
				}

				return collisions % 2 != 0;
			}
			else
				return false;
		}

		/// <summary>
		/// Is the point within a polygon?
		/// </summary>
		/// <remarks>
		/// Assumes polygon has already been tested with AABB
		/// </remarks>
		/// <param name="positions"></param>
		/// <param name="polyBounds"></param>
		/// <param name="edges"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		internal static bool PointInPolygon(Vector2[] positions, pb_Bounds2D polyBounds, pb_Edge[] edges, Vector2 point)
		{
			int len = edges.Length * 2;

			Vector2 rayStart = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);

			int collisions = 0;

			for(int i = 0; i < len; i += 2)
			{
				if( GetLineSegmentIntersect(rayStart, point, positions[i], positions[i+1]) )
					collisions++;
			}

			return collisions % 2 != 0;
		}

		/// <summary>
		/// Is the 2d point within a 2d polygon? This overload is provided as a convenience for 2d arrays coming from cam.WorldToScreenPoint (which includes a Z value).
		/// </summary>
		/// <remarks>
		/// Assumes polygon has already been tested with AABB
		/// </remarks>
		/// <param name="positions"></param>
		/// <param name="polyBounds"></param>
		/// <param name="edges"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		internal static bool PointInPolygon(Vector3[] positions, pb_Bounds2D polyBounds, pb_Edge[] edges, Vector2 point)
		{
			int len = edges.Length * 2;

			Vector2 rayStart = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);

			int collisions = 0;

			for(int i = 0; i < len; i += 2)
			{
				if( GetLineSegmentIntersect(rayStart, point, positions[i], positions[i+1]) )
					collisions++;
			}

			return collisions % 2 != 0;
		}

		internal static bool RectIntersectsLineSegment(Rect rect, Vector2 a, Vector2 b)
		{
			return pb_Clipping.RectContainsLineSegment(rect, a.x, a.y, b.x, b.y);
		}

		internal static bool RectIntersectsLineSegment(Rect rect, Vector3 a, Vector3 b)
		{
			return pb_Clipping.RectContainsLineSegment(rect, a.x, a.y, b.x, b.y);
		}

		/// <summary>
		/// Test if a raycast intersects a triangle.
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
		/// http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
		/// </remarks>
		/// <param name="InRay"></param>
		/// <param name="InTriangleA">First vertex position in the triangle.</param>
		/// <param name="InTriangleB">Second vertex position in the triangle.</param>
		/// <param name="InTriangleC">Third vertex position in the triangle.</param>
		/// <param name="OutDistance">If triangle is intersected, this is the distance of intersection point from ray origin. Zero if not intersected.</param>
		/// <param name="OutPoint">If triangle is intersected, this is the point of collision. Zero if not intersected.</param>
		/// <returns>True if ray intersects, false if not.</returns>
		public static bool RayIntersectsTriangle(Ray InRay, Vector3 InTriangleA, Vector3 InTriangleB, Vector3 InTriangleC,
			out float OutDistance, out Vector3 OutPoint)
		{
			OutDistance = 0f;
			OutPoint = Vector3.zero;

			//Find vectors for two edges sharing V1
			Vector3 e1 = InTriangleB - InTriangleA;
			Vector3 e2 = InTriangleC - InTriangleA;

			//Begin calculating determinant - also used to calculate `u` parameter
			Vector3 P = Vector3.Cross(InRay.direction, e2);

			//if determinant is near zero, ray lies in plane of triangle
			float det = Vector3.Dot(e1, P);

			// Non-culling branch
			// {
				if(det > -Mathf.Epsilon && det < Mathf.Epsilon)
					return false;

				float inv_det = 1f / det;

				//calculate distance from V1 to ray origin
				Vector3 T = InRay.origin - InTriangleA;

				// Calculate u parameter and test bound
				float u = Vector3.Dot(T, P) * inv_det;

				//The intersection lies outside of the triangle
				if(u < 0f || u > 1f)
					return false;

				//Prepare to test v parameter
				Vector3 Q = Vector3.Cross(T, e1);

				//Calculate V parameter and test bound
				float v = Vector3.Dot(InRay.direction, Q) * inv_det;

				//The intersection lies outside of the triangle
				if(v < 0f || u + v  > 1f)
					return false;

				float t = Vector3.Dot(e2, Q) * inv_det;
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

		// Temporary vector3 values
		static Vector3 tv1, tv2, tv3, tv4;

		/// <summary>
		/// Non-allocating version of Ray / Triangle intersection.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="dir"></param>
		/// <param name="vert0"></param>
		/// <param name="vert1"></param>
		/// <param name="vert2"></param>
		/// <param name="distance"></param>
		/// <param name="normal"></param>
		/// <returns></returns>
		internal static bool RayIntersectsTriangle2(	Vector3 origin,
													Vector3 dir,
													Vector3 vert0,
													Vector3 vert1,
													Vector3 vert2,
		 											ref float distance,
		 											ref Vector3 normal)
		{
			pb_Math.Subtract(vert0, vert1, ref tv1);
			pb_Math.Subtract(vert0, vert2, ref tv2);

			pb_Math.Cross(dir, tv2, ref tv4);
			float det = Vector3.Dot(tv1, tv4);

			if(det < Mathf.Epsilon)
				return false;

			pb_Math.Subtract(vert0, origin, ref tv3);

			float u = Vector3.Dot(tv3, tv4);

			if(u < 0f || u > det)
				return false;

			pb_Math.Cross(tv3, tv1, ref tv4);

			float v = Vector3.Dot(dir, tv4);

			if(v < 0f || u + v > det)
				return false;

			distance = Vector3.Dot(tv2, tv4) * (1f / det);
			pb_Math.Cross(tv1, tv2, ref normal);

			return true;
		}

		/// <summary>
		/// Return the secant of radian `x` ( `1f / cos(x)` ).
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float Secant(float x)
		{
			return 1f / Mathf.Cos(x);
		}

		/// <summary>
		/// Calculate the unit vector normal of 3 points:  B-A x C-A
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			float 	ax = p1.x - p0.x,
					ay = p1.y - p0.y,
					az = p1.z - p0.z,
					bx = p2.x - p0.x,
					by = p2.y - p0.y,
					bz = p2.z - p0.z;

			Vector3 cross = Vector3.zero;

			Cross(ax, ay, az, bx, by, bz, ref cross.x, ref cross.y, ref cross.z);

			if (cross.magnitude < Mathf.Epsilon)
			{
				return new Vector3(0f, 0f, 0f); // bad triangle
			}
			else
			{
				cross.Normalize();
				return cross;
			}
		}

		/// <summary>
		/// Calculate the normal of a set of vertices. If indices is null or not divisible by 3, the first 3 positions are used.  If indices is valid, an average of each set of 3 is taken.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static Vector3 Normal(IList<pb_Vertex> vertices, IList<int> indices = null)
		{
			if(indices == null || indices.Count % 3 != 0)
			{
				Vector3 cross = Vector3.Cross(vertices[1].position - vertices[0].position, vertices[2].position - vertices[0].position);
				cross.Normalize();
				return cross;
			}
			else
			{
				int len = indices.Count;
				Vector3 nrm = Vector3.zero;

				for(int i = 0; i < len; i += 3)
					nrm += Normal(vertices[indices[i]].position, vertices[indices[i+1]].position, vertices[indices[i+2]].position);

				nrm /= (len/3f);
				nrm.Normalize();

				return nrm;
			}
		}

		/// <summary>
		/// Finds the "best" normal of a face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static Vector3 Normal(pb_Object pb, pb_Face face)
		{
			Vector3[] _vertices = pb.vertices;

			// if the face is just a quad, use the first
			// triangle normal.
			// otherwise it's not safe to assume that the face
			// has even generally uniform normals
			Vector3 nrm = pb_Math.Normal(
				_vertices[face.indices[0]],
				_vertices[face.indices[1]],
				_vertices[face.indices[2]] );

			if(face.indices.Length > 7)
			{
				Vector3 prj = pb_Projection.FindBestPlane(_vertices, face.distinctIndices).normal;

				if(Vector3.Dot(nrm, prj) < 0f)
				{
					nrm.x = -prj.x;
					nrm.y = -prj.y;
					nrm.z = -prj.z;
				}
				else
				{
					nrm.x = prj.x;
					nrm.y = prj.y;
					nrm.z = prj.z;
				}
			}

			return nrm;
		}

		/// <summary>
		/// Get the average normal of a set of individual triangles.
		/// If p.Length % 3 == 0, finds the normal of each triangle in a face and returns the average. Otherwise return the normal of the first three points.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Vector3 Normal(IList<Vector3> p)
		{
			if(p == null || p.Count < 3)
				return Vector3.zero;

			int c = p.Count;

			if(c % 3 == 0)
			{
				Vector3 nrm = Vector3.zero;
				for(int i = 0; i < c; i+=3)
					nrm += Normal(p[i+0], p[i+1], p[i+2]);
				nrm /= (c/3f);
				nrm.Normalize();
				return nrm;
			}
			else
			{
				Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
				if (cross.magnitude < Mathf.Epsilon)
					return new Vector3(0f, 0f, 0f); // bad triangle
				else
					return cross.normalized;
			}
		}

		/// <summary>
		/// Returns the first normal, tangent, and bitangent for this face using the first triangle available for tangent and bitangent.
		/// </summary>
		/// <remarks>
		/// Does not rely on pb.msh for normal or uv information - uses pb.vertices & pb.uv.
		/// </remarks>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <param name="normal"></param>
		/// <param name="tangent"></param>
		/// <param name="bitangent"></param>
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

		/// <summary>
		/// Is the direction within epsilon of Up, Down, Left, Right, Forward, or Backwards?
		/// </summary>
		/// <param name="v"></param>
		/// <param name="epsilon"></param>
		/// <returns></returns>
		internal static bool IsCardinalAxis(Vector3 v, float epsilon = .00001f)
		{
			v.Normalize();

			return 	(1f - Mathf.Abs(Vector3.Dot(Vector3.up, v))) < epsilon ||
					(1f - Mathf.Abs(Vector3.Dot(Vector3.forward, v))) < epsilon ||
					(1f - Mathf.Abs(Vector3.Dot(Vector3.right, v))) < epsilon;
		}


		/// <summary>
		/// Find the largest value in an array.
		/// </summary>
		/// <param name="array"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static T Max<T>(T[] array) where T : System.IComparable<T>
		{
			if(array == null || array.Length < 1)
				return default(T);

			T max = array[0];
			for(int i = 1; i < array.Length; i++)
				if(array[i].CompareTo(max) >= 0)
					max = array[i];
			return max;
		}

		/// <summary>
		/// Find the smallest value in an array.
		/// </summary>
		/// <param name="array"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static T Min<T>(T[] array) where T : System.IComparable<T>
		{
			if(array == null || array.Length < 1)
				return default(T);

			T min = array[0];
			for(int i = 1; i < array.Length; i++)
				if(array[i].CompareTo(min) < 0)
					min = array[i];
			return min;
		}

		/// <summary>
		/// Return the largest axis in a Vector3.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		internal static float LargestValue(Vector3 v)
		{
			if(v.x > v.y && v.x > v.z) return v.x;
			if(v.y > v.x && v.y > v.z) return v.y;
			return v.z;
		}

		/// <summary>
		/// Return the largest axis in a Vector2.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		internal static float LargestValue(Vector2 v)
		{
			return (v.x > v.y) ? v.x :v.y;
		}

		/// <summary>
		/// The smallest X and Y value found in an array of Vector2. May or may not belong to the same Vector2.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		internal static Vector2 SmallestVector2(Vector2[] v)
		{
			int len = v.Length;
			Vector2 l = v[0];
			for(int i = 0; i < len; i++)
			{
				if(v[i].x < l.x) l.x = v[i].x;
				if(v[i].y < l.y) l.y = v[i].y;
			}
			return l;
		}

		/// <summary>
		/// The smallest X and Y value found in an array of Vector2.  May or may not belong to the same Vector2.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="indices">Indices of v array to test.</param>
		/// <returns></returns>
		internal static Vector2 SmallestVector2(Vector2[] v, int[] indices)
		{
			int len = indices.Length;
			Vector2 l = v[indices[0]];
			for(int i = 0; i < len; i++)
			{
				if(v[indices[i]].x < l.x) l.x = v[indices[i]].x;
				if(v[indices[i]].y < l.y) l.y = v[indices[i]].y;
			}
			return l;
		}

		/// <summary>
		/// The largest X and Y value in an array.  May or may not belong to the same Vector2.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		internal static Vector2 LargestVector2(Vector2[] v)
		{
			int len = v.Length;
			Vector2 l = v[0];
			for(int i = 0; i < len; i++)
			{
				if(v[i].x > l.x) l.x = v[i].x;
				if(v[i].y > l.y) l.y = v[i].y;
			}
			return l;
		}

		internal static Vector2 LargestVector2(Vector2[] v, int[] indices)
		{
			int len = indices.Length;
			Vector2 l = v[indices[0]];
			for(int i = 0; i < len; i++)
			{
				if(v[indices[i]].x > l.x) l.x = v[indices[i]].x;
				if(v[indices[i]].y > l.y) l.y = v[indices[i]].y;
			}
			return l;
		}

		/// <summary>
		/// Creates an AABB with vertices and returns the Center point.
		/// </summary>
		/// <param name="verts"></param>
		/// <returns></returns>
		internal static Vector3 BoundsCenter(Vector3[] verts)
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

		/// <summary>
		/// Gets the average of a vector array.
		/// </summary>
		/// <param name="v">The array</param>
		/// <param name="indices">If provided the average is the sum of all points contained in the indices array. If not, the entire v array is used.</param>
		/// <returns>Average Vector3 of passed vertex array.</returns>
		public static Vector2 Average(IList<Vector2> v, IList<int> indices = null)
		{
			Vector2 sum = Vector2.zero;
			float len = indices == null ? v.Count : indices.Count;

			if( indices == null )
				for(int i = 0; i < len; i++) sum += v[i];
			else
				for(int i = 0; i < len; i++) sum += v[indices[i]];
			return sum/len;
		}

		/// <summary>
		/// Gets the average of a vector array.
		/// </summary>
		/// <param name="v">The array</param>
		/// <param name="indices">If provided the average is the sum of all points contained in the indices array. If not, the entire v array is used.</param>
		/// <returns>Average Vector3 of passed vertex array.</returns>
		public static Vector3 Average(IList<Vector3> v, IList<int> indices = null)
		{
			Vector3 sum = Vector3.zero;
			float len = indices == null ? v.Count : indices.Count;

			if( indices == null )
			{
				for(int i = 0; i < len; i++)
				{
					sum.x += v[i].x;
					sum.y += v[i].y;
					sum.z += v[i].z;
				}
			}
			else
			{
				for(int i = 0; i < len; i++)
				{
					sum.x += v[indices[i]].x;
					sum.y += v[indices[i]].y;
					sum.z += v[indices[i]].z;
				}
			}

			return sum / len;
		}

		/// <summary>
		/// Average a set of vertices.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="selector"></param>
		/// <param name="indices"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Vector3 Average<T>(this IList<T> v, System.Func<T, Vector3> selector, IList<int> indices = null)
		{
			Vector3 sum = Vector3.zero;
			float len = indices == null ? v.Count : indices.Count;
			if( indices == null )
				for(int i = 0; i < len; i++) sum += selector(v[i]);
			else
				for(int i = 0; i < len; i++) sum += selector(v[indices[i]]);
			return sum/len;
		}

		internal static Vector4 Average(IList<Vector4> v, IList<int> indices = null)
		{
			Vector4 sum = Vector4.zero;
			float len = indices == null ? v.Count : indices.Count;
			if( indices == null )
				for(int i = 0; i < len; i++) sum += v[i];
			else
				for(int i = 0; i < len; i++) sum += v[indices[i]];
			return sum/len;
		}

		internal static Color Average(IList<Color> c, IList<int> indices = null)
		{
			Color sum = c[0];
			float len = indices == null ? c.Count : indices.Count;
			if( indices == null )
				for(int i = 1; i < len; i++) sum += c[i];
			else
				for(int i = 1; i < len; i++) sum += c[indices[i]];
			return sum/len;
		}

		/// <summary>
		/// Compares 2 vector2 objects, allowing for a margin of error.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="b"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		public static bool Approx2(this Vector2 v, Vector2 b, float delta = FLT_COMPARE_EPSILON)
		{
			return
				Mathf.Abs(v.x - b.x) < delta &&
				Mathf.Abs(v.y - b.y) < delta;
		}

		/// <summary>
		/// Compares 2 vector3 objects, allowing for a margin of error.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="b"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		public static bool Approx3(this Vector3 v, Vector3 b, float delta = FLT_COMPARE_EPSILON)
		{
			return
				Mathf.Abs(v.x - b.x) < delta &&
				Mathf.Abs(v.y - b.y) < delta &&
				Mathf.Abs(v.z - b.z) < delta;
		}

		/// <summary>
		/// Compares 2 vector4 objects, allowing for a margin of error.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="b"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		public static bool Approx4(this Vector4 v, Vector4 b, float delta = FLT_COMPARE_EPSILON)
		{
			return
				Mathf.Abs(v.x - b.x) < delta &&
				Mathf.Abs(v.y - b.y) < delta &&
				Mathf.Abs(v.z - b.z) < delta &&
				Mathf.Abs(v.w - b.w) < delta;
		}

		/// <summary>
		/// Compares 2 color objects, allowing for a margin of error.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		internal static bool ApproxC(this Color a, Color b, float delta = FLT_COMPARE_EPSILON)
		{
			return 	Mathf.Abs(a.r - b.r) < delta &&
					Mathf.Abs(a.g - b.g) < delta &&
					Mathf.Abs(a.b - b.b) < delta &&
					Mathf.Abs(a.a - b.a) < delta;
		}

		/// <summary>
		/// Compares float values, allowing for a margin of error.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		internal static bool Approx(this float a, float b, float delta = FLT_COMPARE_EPSILON)
		{
			return Mathf.Abs(b - a) < Mathf.Abs(delta);
		}

		/// <summary>
		/// Wrap value to range.
		/// </summary>
		/// <remarks>
		/// http://stackoverflow.com/questions/707370/clean-efficient-algorithm-for-wrapping-integers-in-c
		/// </remarks>
		/// <param name="value"></param>
		/// <param name="lowerBound"></param>
		/// <param name="upperBound"></param>
		/// <returns></returns>
		internal static int Wrap(int value, int lowerBound, int upperBound)
		{
			int range_size = upperBound - lowerBound + 1;

			if (value < lowerBound)
				value += range_size * ((lowerBound - value) / range_size + 1);

			return lowerBound + (value - lowerBound) % range_size;
		}

		/// <summary>
		/// Clamp int to range.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="lowerBound"></param>
		/// <param name="upperBound"></param>
		/// <returns></returns>
		public static int Clamp(int value, int lowerBound, int upperBound)
		{
			return value < lowerBound ? lowerBound : value > upperBound ? upperBound : value;
		}

		internal static Vector2 ToMask(this Vector2 vec, float delta = FLT_EPSILON)
		{
			return new Vector2(
				Mathf.Abs(vec.x) > delta ? 1f : 0f,
				Mathf.Abs(vec.y) > delta ? 1f : 0f
				);
		}

		internal static Vector3 ToMask(this Vector3 vec, float delta = FLT_EPSILON)
		{
			return new Vector3(
				Mathf.Abs(vec.x) > delta ? 1f : 0f,
				Mathf.Abs(vec.y) > delta ? 1f : 0f,
				Mathf.Abs(vec.z) > delta ? 1f : 0f
				);
		}

		internal static Vector3 ToSignedMask(this Vector3 vec, float delta = FLT_EPSILON)
		{
			return new Vector3(
				Mathf.Abs(vec.x) > delta ? vec.x/Mathf.Abs(vec.x) : 0f,
				Mathf.Abs(vec.y) > delta ? vec.y/Mathf.Abs(vec.y) : 0f,
				Mathf.Abs(vec.z) > delta ? vec.z/Mathf.Abs(vec.z) : 0f
				);
		}

		internal static Vector3 Abs(this Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		internal static int IntSum(this Vector3 mask)
		{
			return (int)Mathf.Abs(mask.x) + (int)Mathf.Abs(mask.y) + (int)Mathf.Abs(mask.z);
		}

		/// <summary>
		/// Non-allocating cross product.
		/// </summary>
		/// <remarks>
		/// `ref` does not box with primitive types (https://msdn.microsoft.com/en-us/library/14akc2c7.aspx)
		/// </remarks>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		internal static void Cross(Vector3 a, Vector3 b, ref float x, ref float y, ref float z)
		{
			x = a.y * b.z - a.z * b.y;
			y = a.z * b.x - a.x * b.z;
			z = a.x * b.y - a.y * b.x;
		}

		/// <summary>
		/// Non-allocating cross product.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="res"></param>
		internal static void Cross(Vector3 a, Vector3 b, ref Vector3 res)
		{
			res.x = a.y * b.z - a.z * b.y;
			res.y = a.z * b.x - a.x * b.z;
			res.z = a.x * b.y - a.y * b.x;
		}

		/// <summary>
		/// Non-allocating cross product.
		/// </summary>
		/// <param name="ax"></param>
		/// <param name="ay"></param>
		/// <param name="az"></param>
		/// <param name="bx"></param>
		/// <param name="by"></param>
		/// <param name="bz"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		internal static void Cross(float ax, float ay, float az, float bx, float by, float bz, ref float x, ref float y, ref float z)
		{
			x = ay * bz - az * by;
			y = az * bx - ax * bz;
			z = ax * by - ay * bx;
		}

		/// <summary>
		/// Vector subtraction without allocating a new vector.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="res"></param>
		internal static void Subtract(Vector3 a, Vector3 b, ref Vector3 res)
		{
			res.x = b.x - a.x;
			res.y = b.y - a.y;
			res.z = b.z - a.z;
		}
	}
}
