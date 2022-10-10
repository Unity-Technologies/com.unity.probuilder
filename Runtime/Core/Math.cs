using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Defines a set of math functions that are useful for working with 3D meshes.
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// Defines `Pi / 2`.
        /// </summary>
        public const float phi = 1.618033988749895f;

        /// <summary>
        /// Defines ProBuilder's epsilon constant.
        /// </summary>
        const float k_FltEpsilon = float.Epsilon;

        /// <summary>
        /// Defines the epsilon to use when comparing vertex positions for equality.
        /// </summary>
        const float k_FltCompareEpsilon = .0001f;

        /// <summary>
        /// The minimum distance a handle must move on an axis before considering that axis as engaged.
        /// </summary>
        internal const float handleEpsilon = .0001f;

        /// <summary>
        /// Get a point on the circumference of a circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="angleInDegrees">Where along the circle should the point be projected. Angle is in degrees.</param>
        /// <param name="origin"></param>
        /// <returns></returns>
        internal static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
        {
            // Convert from degrees to radians via multiplication by PI/180
            float x = (float)(radius * Mathf.Cos(Mathf.Deg2Rad * angleInDegrees)) + origin.x;
            float y = (float)(radius * Mathf.Sin(Mathf.Deg2Rad * angleInDegrees)) + origin.y;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Get a point on the circumference of an ellipse.
        /// </summary>
        /// <param name="xRadius">The radius of the circle on the x-axis.</param>
        /// <param name="yRadius">The radius of the circle on the y-axis.</param>
        /// <param name="angleInDegrees">Where along the circle should the point be projected. Angle is in degrees.</param>
        /// <param name="origin">The center point of the ellipse</param>
        /// <param name="tangent">Out: the resulting at the computed position</param>
        /// <returns></returns>
        internal static Vector2 PointInEllipseCircumference(float xRadius, float yRadius, float angleInDegrees, Vector2 origin, out Vector2 tangent)
        {
            // Convert from degrees to radians via multiplication by PI/180
            var cosA = Mathf.Cos(Mathf.Deg2Rad * angleInDegrees);
            var sinA = Mathf.Sin(Mathf.Deg2Rad * angleInDegrees);

            float x = (float)(xRadius * cosA) + origin.x;
            float y = (float)(yRadius * sinA) + origin.y;

            tangent = new Vector2( -y / (yRadius * yRadius), x / (xRadius * xRadius));
            tangent.Normalize();

            return new Vector2(x,y);
        }

        /// <summary>
        /// Get a point on the circumference of an ellipse.
        /// </summary>
        /// <param name="xRadius">The radius of the circle on the x-axis.</param>
        /// <param name="yRadius">The radius of the circle on the y-axis.</param>
        /// <param name="angleInDegrees">Where along the circle should the point be projected. Angle is in degrees.</param>
        /// <param name="origin">The center point of the ellipse</param>
        /// <param name="tangent">Out: the resulting at the computed position</param>
        /// <returns></returns>
        internal static Vector2 PointInEllipseCircumferenceWithConstantAngle(float xRadius, float yRadius, float angleInDegrees, Vector2 origin, out Vector2 tangent)
        {
            // Convert from degrees to radians via multiplication by PI/180
            var cosA = Mathf.Cos(Mathf.Deg2Rad * angleInDegrees);
            var sinA = Mathf.Sin(Mathf.Deg2Rad * angleInDegrees);

            float tan = Mathf.Tan(Mathf.Deg2Rad * angleInDegrees);
            float tan2 = tan * tan;
            float newX = ( xRadius * yRadius ) / Mathf.Sqrt(yRadius * yRadius + xRadius * xRadius * tan2);
            if(cosA < 0)
                newX = -newX;
            float newY = ( xRadius * yRadius ) / Mathf.Sqrt(xRadius * xRadius + yRadius * yRadius / tan2);
            if(sinA < 0)
                newY = -newY;

            tangent = new Vector2( -newY / (yRadius * yRadius), newX / (xRadius * xRadius));
            tangent.Normalize();

            return new Vector2(newX, newY);
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
            float x = (radius * Mathf.Cos(Mathf.Deg2Rad * latitudeAngle) * Mathf.Sin(Mathf.Deg2Rad * longitudeAngle));
            float y = (radius * Mathf.Sin(Mathf.Deg2Rad * latitudeAngle) * Mathf.Sin(Mathf.Deg2Rad * longitudeAngle));
            float z = (radius * Mathf.Cos(Mathf.Deg2Rad * longitudeAngle));

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Find the signed angle from direction a to direction b.
        /// </summary>
        /// <param name="a">The direction from which to rotate.</param>
        /// <param name="b">The direction to rotate towards.</param>
        /// <returns>A signed angle in degrees from direction a to direction b.</returns>
        internal static float SignedAngle(Vector2 a, Vector2 b)
        {
            float t = Vector2.Angle(a, b);
            if (b.x - a.x < 0)
                t = 360f - t;
            return t;
        }

        /// <summary>
        /// Returns the squared distance between two points. This is the same as `(b - a).sqrMagnitude`.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>The squared distance between two points.</returns>
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            float dx = b.x - a.x,
                  dy = b.y - a.y,
                  dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Returns the area of a triangle.
        /// </summary>
        /// <remarks>See [area of triangles, the fast way blog](http://www.iquilezles.org/blog/?p=1579).</remarks>
        /// <param name="x">First vertex position of the triangle.</param>
        /// <param name="y">Second vertex position of the triangle.</param>
        /// <param name="z">Third vertex position of the triangle.</param>
        /// <returns>The area of the triangle.</returns>
        public static float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
        {
            float   a = SqrDistance(x, y),
                    b = SqrDistance(y, z),
                    c = SqrDistance(z, x);

            return Mathf.Sqrt((2f * a * b + 2f * b * c + 2f * c * a - a * a - b * b - c * c) / 16f);
        }

        /// <summary>
        /// Returns the Area of a polygon.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        internal static float PolygonArea(Vector3[] vertices, int[] indexes)
        {
            float area = 0f;

            for (int i = 0; i < indexes.Length; i += 3)
                area += TriangleArea(vertices[indexes[i]], vertices[indexes[i + 1]], vertices[indexes[i + 2]]);

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
            float cx = origin.x, cy = origin.y; // origin
            float px = v.x, py = v.y;           // point

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
        /// Scales a Vector2 using the origin as the pivot point.
        /// </summary>
        /// <param name="v">The Vector2 to scale.</param>
        /// <param name="origin">The center point to use as the pivot point.</param>
        /// <param name="scale">Unit(s) to scale by.</param>
        /// <returns>The scaled Vector2</returns>
        public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
        {
            Vector2 tp = v - origin;
            tp = Vector2.Scale(tp, scale);
            tp += origin;

            return tp;
        }

        internal static Vector2 Perpendicular(Vector2 value)
        {
            return new Vector2(-value.y, value.x);
        }

        /// <summary>
        /// Reflects a point across a line segment.
        /// </summary>
        /// <param name="point">The point to reflect.</param>
        /// <param name="lineStart">First point of the line segment.</param>
        /// <param name="lineEnd">Second point of the line segment.</param>
        /// <returns>The reflected point.</returns>
        public static Vector2 ReflectPoint(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            Vector2 perp = new Vector2(-line.y, line.x);    // skip normalize

            float dist = Mathf.Sin(Vector2.Angle(line, point - lineStart) * Mathf.Deg2Rad) * Vector2.Distance(point, lineStart);

            return point + (dist * 2f) * (Vector2.Dot(point - lineStart, perp) > 0 ? -1f : 1f) * perp;
        }

        internal static float SqrDistanceRayPoint(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
        }

        /// <summary>
        /// Returns the distance between a point and a finite line segment using Vector2s.
        /// </summary>
        /// <remarks>See [Shortest distance between a point and a line segment](http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment).</remarks>
        /// <param name="point">The point.</param>
        /// <param name="lineStart">Where the line starts.</param>
        /// <param name="lineEnd">Where the line ends.</param>
        /// <returns>The distance from the point to the nearest point on a line segment.</returns>
        public static float DistancePointLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            // Return minimum distance between line segment vw and point p
            float l2 = ((lineStart.x - lineEnd.x) * (lineStart.x - lineEnd.x)) + ((lineStart.y - lineEnd.y) * (lineStart.y - lineEnd.y));  // i.e. |w-v|^2 -  avoid a sqrt

            if (l2 == 0.0f) return Vector2.Distance(point, lineStart);   // v == w case

            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line.
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            float t = Vector2.Dot(point - lineStart, lineEnd - lineStart) / l2;

            if (t < 0.0)
                return Vector2.Distance(point, lineStart);              // Beyond the 'v' end of the segment
            else if (t > 1.0)
                return Vector2.Distance(point, lineEnd);            // Beyond the 'w' end of the segment

            Vector2 projection = lineStart + t * (lineEnd - lineStart);     // Projection falls on the segment

            return Vector2.Distance(point, projection);
        }

        /// <summary>
        /// Returns the distance between a point and a finite line segment using Vector3s.
        /// </summary>
        /// <remarks>See [Shortest distance between a point and a line segment](http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment).</remarks>
        /// <param name="point">The point.</param>
        /// <param name="lineStart">Line start.</param>
        /// <param name="lineEnd">Line end.</param>
        /// <returns>The distance from point to the nearest point on a line segment.</returns>
        public static float DistancePointLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            // Return minimum distance between line segment vw and point p
            float l2 = ((lineStart.x - lineEnd.x) * (lineStart.x - lineEnd.x)) + ((lineStart.y - lineEnd.y) * (lineStart.y - lineEnd.y)) + ((lineStart.z - lineEnd.z) * (lineStart.z - lineEnd.z));  // i.e. |w-v|^2 -  avoid a sqrt

            if (l2 == 0.0f) return Vector3.Distance(point, lineStart);   // v == w case

            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line.
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            float t = Vector3.Dot(point - lineStart, lineEnd - lineStart) / l2;

            if (t < 0.0)
                return Vector3.Distance(point, lineStart);              // Beyond the 'v' end of the segment
            else if (t > 1.0)
                return Vector3.Distance(point, lineEnd);            // Beyond the 'w' end of the segment

            Vector3 projection = lineStart + t * (lineEnd - lineStart);     // Projection falls on the segment

            return Vector3.Distance(point, projection);
        }

        /// <summary>
        /// Calculates and returns the nearest point between two rays.
        /// </summary>
        /// <param name="a">First ray.</param>
        /// <param name="b">Second ray.</param>
        /// <returns>The nearest point between the two rays</returns>
        public static Vector3 GetNearestPointRayRay(Ray a, Ray b)
        {
            return GetNearestPointRayRay(a.origin, a.direction, b.origin, b.direction);
        }

        internal static Vector3 GetNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
        {
            float dot = Vector3.Dot(ad, bd);
            float abs = Mathf.Abs(dot);

            // ray is parallel (or garbage)
            if ((abs - 1f) > Mathf.Epsilon || abs < Mathf.Epsilon)
                return ao;

            Vector3 c = bo - ao;

            float n = -dot * Vector3.Dot(bd, c) + Vector3.Dot(ad, c) * Vector3.Dot(bd, bd);
            float d = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - dot * dot;

            return ao + ad * (n / d);
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
            t = (s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

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
            t = (s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

            return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
        }

        /// <summary>
        /// Casts a ray from outside the bounds to the polygon and checks how many edges are hit.
        /// </summary>
        /// <param name="polygon">A series of individual edges composing a polygon.  polygon length *must* be divisible by 2.</param>
        /// <param name="point"></param>
        /// <param name="indexes">If present these indexes make up the border of polygon. If not, polygon is assumed to be in correct order.</param>
        /// <returns>True if the polygon contains point. False otherwise.</returns>
        internal static bool PointInPolygon(Vector2[] polygon, Vector2 point, int[] indexes = null)
        {
            int len = indexes != null ? indexes.Length : polygon.Length;

            if (len % 2 != 0)
            {
                Debug.LogError("PointInPolygon requires polygon indexes be divisible by 2!");
                return false;
            }

            Bounds2D bounds = new Bounds2D(polygon, indexes);

            if (bounds.ContainsPoint(point))
            {
                //Get the direction toward the first edge of the polygon
                Vector2 p1 = polygon[indexes != null ? indexes[0] : 0];
                Vector2 p2 = polygon[indexes != null ? indexes[1] : 1];
                Vector2 center = p1 + (p2 - p1) * 0.5f;
                Vector2 dir = center - bounds.center;

                Vector2 rayStart = bounds.center + dir * (bounds.size.y + bounds.size.x + 2f);
                int collisions = 0;

                for (int i = 0; i < len; i += 2)
                {
                    int a = indexes != null ? indexes[i] : i;
                    int b = indexes != null ? indexes[i + 1] : i + 1;

                    if (GetLineSegmentIntersect(rayStart, point, polygon[a], polygon[b]))
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
        internal static bool PointInPolygon(Vector2[] positions, Bounds2D polyBounds, Edge[] edges, Vector2 point)
        {
            int len = edges.Length * 2;

            Vector2 rayStart = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);

            int collisions = 0;

            for (int i = 0; i < len; i += 2)
            {
                if (GetLineSegmentIntersect(rayStart, point, positions[i], positions[i + 1]))
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
        internal static bool PointInPolygon(Vector3[] positions, Bounds2D polyBounds, Edge[] edges, Vector2 point)
        {
            int len = edges.Length * 2;

            Vector2 rayStart = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);

            int collisions = 0;

            for (int i = 0; i < len; i += 2)
            {
                if (GetLineSegmentIntersect(rayStart, point, positions[i], positions[i + 1]))
                    collisions++;
            }

            return collisions % 2 != 0;
        }

        internal static bool RectIntersectsLineSegment(Rect rect, Vector2 a, Vector2 b)
        {
            return Clipping.RectContainsLineSegment(rect, a.x, a.y, b.x, b.y);
        }

        internal static bool RectIntersectsLineSegment(Rect rect, Vector3 a, Vector3 b)
        {
            return Clipping.RectContainsLineSegment(rect, a.x, a.y, b.x, b.y);
        }

        /// <summary>
        /// Tests whether a raycast intersects a triangle. Does not test for culling.
        /// </summary>
        /// <remarks>
        /// See [Möller–Trumbore intersection algorithm](http://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm).
        /// </remarks>
        /// <param name="InRay">The ray to test.</param>
        /// <param name="InTriangleA">First vertex position in the triangle.</param>
        /// <param name="InTriangleB">Second vertex position in the triangle.</param>
        /// <param name="InTriangleC">Third vertex position in the triangle.</param>
        /// <param name="OutDistance">The distance of the intersection point from the ray origin if the ray intersects the triangle; otherwise, 0.</param>
        /// <param name="OutPoint">The point of collision if the ray intersects the triangle; otherwise, 0.</param>
        /// <returns>True if the ray intersects the triangle; otherwise false.</returns>
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
            if (det > -Mathf.Epsilon && det < Mathf.Epsilon)
                return false;

            float inv_det = 1f / det;

            //calculate distance from V1 to ray origin
            Vector3 T = InRay.origin - InTriangleA;

            // Calculate u parameter and test bound
            float u = Vector3.Dot(T, P) * inv_det;

            //The intersection lies outside of the triangle
            if (u < 0f || u > 1f)
                return false;

            //Prepare to test v parameter
            Vector3 Q = Vector3.Cross(T, e1);

            //Calculate V parameter and test bound
            float v = Vector3.Dot(InRay.direction, Q) * inv_det;

            //The intersection lies outside of the triangle
            if (v < 0f || u + v  > 1f)
                return false;

            float t = Vector3.Dot(e2, Q) * inv_det;
            // }

            if (t > Mathf.Epsilon)
            {
                //ray intersection
                OutDistance = t;

                OutPoint.x = (u * InTriangleB.x + v * InTriangleC.x + (1 - (u + v)) * InTriangleA.x);
                OutPoint.y = (u * InTriangleB.y + v * InTriangleC.y + (1 - (u + v)) * InTriangleA.y);
                OutPoint.z = (u * InTriangleB.z + v * InTriangleC.z + (1 - (u + v)) * InTriangleA.z);

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
        internal static bool RayIntersectsTriangle2(Vector3 origin,
            Vector3 dir,
            Vector3 vert0,
            Vector3 vert1,
            Vector3 vert2,
            ref float distance,
            ref Vector3 normal)
        {
            Math.Subtract(vert0, vert1, ref tv1);
            Math.Subtract(vert0, vert2, ref tv2);

            Math.Cross(dir, tv2, ref tv4);
            float det = Vector3.Dot(tv1, tv4);

            if (det < Mathf.Epsilon)
                return false;

            Math.Subtract(vert0, origin, ref tv3);

            float u = Vector3.Dot(tv3, tv4);

            if (u < 0f || u > det)
                return false;

            Math.Cross(tv3, tv1, ref tv4);

            float v = Vector3.Dot(dir, tv4);

            if (v < 0f || u + v > det)
                return false;

            distance = Vector3.Dot(tv2, tv4) * (1f / det);
            Math.Cross(tv1, tv2, ref normal);

            return true;
        }

        /// <summary>
        /// Returns the secant of a radian. This is equivalent to: `1f / cos(x)`.
        /// </summary>
        /// <param name="x">The radian to calculate the secant of.</param>
        /// <returns>The secant of radian `x`.</returns>
        public static float Secant(float x)
        {
            return 1f / Mathf.Cos(x);
        }

        /// <summary>
        /// Calculates and returns the unit vector normal of 3 points in a triangle.
        ///
        /// This is equivalent to: `B-A x C-A`.
        /// </summary>
        /// <param name="p0">First point of the triangle.</param>
        /// <param name="p1">Second point of the triangle.</param>
        /// <param name="p2">Third point of the triangle.</param>
        /// <returns>The unit vector normal of the points</returns>
        public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float   ax = p1.x - p0.x,
                    ay = p1.y - p0.y,
                    az = p1.z - p0.z,
                    bx = p2.x - p0.x,
                    by = p2.y - p0.y,
                    bz = p2.z - p0.z;

            Vector3 cross = new Vector3(
                ay * bz - az * by,
                az * bx - ax * bz,
                ax * by - ay * bx);

            if (cross.magnitude < Mathf.Epsilon)
                return new Vector3(0f, 0f, 0f); // bad triangle

            cross.Normalize();
            return cross;
        }

        /// <summary>
        /// Calculate the normal of a set of vertices. If indexes is null or not divisible by 3, the first 3 positions are used.  If indexes is valid, an average of each set of 3 is taken.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        internal static Vector3 Normal(IList<Vertex> vertices, IList<int> indexes = null)
        {
            if (indexes == null || indexes.Count % 3 != 0)
            {
                Vector3 cross = Vector3.Cross(vertices[1].position - vertices[0].position, vertices[2].position - vertices[0].position);
                cross.Normalize();
                return cross;
            }
            else
            {
                int len = indexes.Count;
                Vector3 nrm = Vector3.zero;

                for (int i = 0; i < len; i += 3)
                    nrm += Normal(vertices[indexes[i]].position, vertices[indexes[i + 1]].position, vertices[indexes[i + 2]].position);

                nrm /= (len / 3f);
                nrm.Normalize();

                return nrm;
            }
        }

        /// <summary>
        /// Finds and returns the best normal for a face.
        /// </summary>
        /// <param name="mesh">The mesh that the target face belongs to.</param>
        /// <param name="face">The face to calculate a normal for.</param>
        /// <returns>A normal that most closely matches the face orientation in model coordinates.</returns>
        public static Vector3 Normal(ProBuilderMesh mesh, Face face)
        {
            if (mesh == null || face == null)
                throw new ArgumentNullException("mesh");

            var positions = mesh.positionsInternal;

            // if the face is just a quad, use the first triangle normal. otherwise it's not safe to assume that the
            // face has even generally uniform normals
            Vector3 nrm = Normal(
                    positions[face.indexesInternal[0]],
                    positions[face.indexesInternal[1]],
                    positions[face.indexesInternal[2]]);

            if (face.indexesInternal.Length > 6)
            {
                Vector3 prj = Projection.FindBestPlane(positions, face.distinctIndexesInternal).normal;

                if (Vector3.Dot(nrm, prj) < 0f)
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
        /// Returns the first normal, tangent, and bitangent for this face using the first triangle available for tangent and bitangent.
        /// </summary>
        /// <param name="mesh">The mesh that the target face belongs to.</param>
        /// <param name="face">The face to calculate normal information for.</param>
        /// <returns>The normal, bitangent, and tangent for the face.</returns>
        public static Normal NormalTangentBitangent(ProBuilderMesh mesh, Face face)
        {
            if (mesh == null || face == null || face.indexesInternal.Length < 3)
                throw new System.ArgumentNullException("mesh", "Cannot find normal, tangent, and bitangent for null object, or faces with < 3 indexes.");

            if (mesh.texturesInternal == null || mesh.texturesInternal.Length != mesh.vertexCount)
                throw new ArgumentException("Mesh textures[0] channel is not present, cannot calculate tangents.");

            var nrm = Math.Normal(mesh, face);

            Vector3 tan1 = Vector3.zero;
            Vector3 tan2 = Vector3.zero;
            Vector4 tan = new Vector4(0f, 0f, 0f, 1f);

            long i1 = face.indexesInternal[0];
            long i2 = face.indexesInternal[1];
            long i3 = face.indexesInternal[2];

            Vector3 v1 = mesh.positionsInternal[i1];
            Vector3 v2 = mesh.positionsInternal[i2];
            Vector3 v3 = mesh.positionsInternal[i3];

            Vector2 w1 = mesh.texturesInternal[i1];
            Vector2 w2 = mesh.texturesInternal[i2];
            Vector2 w3 = mesh.texturesInternal[i3];

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

            Vector3 n = nrm;
            Vector3.OrthoNormalize(ref n, ref tan1);

            tan.x = tan1.x;
            tan.y = tan1.y;
            tan.z = tan1.z;

            tan.w = (Vector3.Dot(Vector3.Cross(n, tan1), tan2) < 0.0f) ? -1.0f : 1.0f;

            return new Normal()
            {
                normal = nrm,
                tangent = tan,
                bitangent = Vector3.Cross(nrm, ((Vector3)tan) * tan.w)
            };
        }

        /// <summary>
        /// Is the direction within epsilon of Up, Down, Left, Right, Forward, or Backwards?
        /// </summary>
        /// <param name="v"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        internal static bool IsCardinalAxis(Vector3 v, float epsilon = k_FltEpsilon)
        {
            if (v == Vector3.zero)
                return false;

            v.Normalize();

            return (1f - Mathf.Abs(Vector3.Dot(Vector3.up, v))) < epsilon ||
                (1f - Mathf.Abs(Vector3.Dot(Vector3.forward, v))) < epsilon ||
                (1f - Mathf.Abs(Vector3.Dot(Vector3.right, v))) < epsilon;
        }

        /// <summary>
        /// Component-wise division.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        internal static Vector2 DivideBy(this Vector2 v, Vector2 o)
        {
            return new Vector2(v.x / o.x, v.y / o.y);
        }

        /// <summary>
        /// Component-wise division.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        internal static Vector3 DivideBy(this Vector3 v, Vector3 o)
        {
            return new Vector3(v.x / o.x, v.y / o.y, v.z / o.z);
        }

        /// <summary>
        /// Find the largest value in an array.
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T Max<T>(T[] array) where T : System.IComparable<T>
        {
            if (array == null || array.Length < 1)
                return default(T);

            T max = array[0];
            for (int i = 1; i < array.Length; i++)
                if (array[i].CompareTo(max) >= 0)
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
            if (array == null || array.Length < 1)
                return default(T);

            T min = array[0];
            for (int i = 1; i < array.Length; i++)
                if (array[i].CompareTo(min) < 0)
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
            if (v.x > v.y && v.x > v.z) return v.x;
            if (v.y > v.x && v.y > v.z) return v.y;
            return v.z;
        }

        /// <summary>
        /// Return the largest axis in a Vector2.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal static float LargestValue(Vector2 v)
        {
            return (v.x > v.y) ? v.x : v.y;
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
            for (int i = 0; i < len; i++)
            {
                if (v[i].x < l.x) l.x = v[i].x;
                if (v[i].y < l.y) l.y = v[i].y;
            }
            return l;
        }

        /// <summary>
        /// The smallest X and Y value found in an array of Vector2. May or may not belong to the same Vector2.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="indexes">Indexes of v array to test.</param>
        /// <returns></returns>
        internal static Vector2 SmallestVector2(Vector2[] v, IList<int> indexes)
        {
            int len = indexes.Count;
            Vector2 l = v[indexes[0]];
            for (int i = 0; i < len; i++)
            {
                if (v[indexes[i]].x < l.x) l.x = v[indexes[i]].x;
                if (v[indexes[i]].y < l.y) l.y = v[indexes[i]].y;
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
            for (int i = 0; i < len; i++)
            {
                if (v[i].x > l.x) l.x = v[i].x;
                if (v[i].y > l.y) l.y = v[i].y;
            }
            return l;
        }

        internal static Vector2 LargestVector2(Vector2[] v, IList<int> indexes)
        {
            int len = indexes.Count;
            Vector2 l = v[indexes[0]];
            for (int i = 0; i < len; i++)
            {
                if (v[indexes[i]].x > l.x) l.x = v[indexes[i]].x;
                if (v[indexes[i]].y > l.y) l.y = v[indexes[i]].y;
            }
            return l;
        }

        /// <summary>
        /// Creates an AABB with a set of vertices.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        internal static Bounds GetBounds(Vector3[] positions, IList<int> indices = null)
        {
            bool hasIndices = indices != null;

            if ((hasIndices && indices.Count < 1) || positions.Length < 1)
                return default(Bounds);

            Vector3 min = positions[hasIndices ? indices[0] : 0];
            Vector3 max = min;

            if (hasIndices)
            {
                for (int i = 1, c = indices.Count; i < c; i++)
                {
                    min.x = Mathf.Min(positions[indices[i]].x, min.x);
                    max.x = Mathf.Max(positions[indices[i]].x, max.x);

                    min.y = Mathf.Min(positions[indices[i]].y, min.y);
                    max.y = Mathf.Max(positions[indices[i]].y, max.y);

                    min.z = Mathf.Min(positions[indices[i]].z, min.z);
                    max.z = Mathf.Max(positions[indices[i]].z, max.z);
                }
            }
            else
            {
                for (int i = 1, c = positions.Length; i < c; i++)
                {
                    min.x = Mathf.Min(positions[i].x, min.x);
                    max.x = Mathf.Max(positions[i].x, max.x);

                    min.y = Mathf.Min(positions[i].y, min.y);
                    max.y = Mathf.Max(positions[i].y, max.y);

                    min.z = Mathf.Min(positions[i].z, min.z);
                    max.z = Mathf.Max(positions[i].z, max.z);
                }
            }

            return new Bounds((min + max) * .5f, max - min);
        }

        /// <summary>
        /// Calculates and returns the average of a Vector2 array.
        /// </summary>
        /// <param name="array">The array to get the average for.</param>
        /// <param name="indexes">
        /// Specify a list of points in the vector array to calculate the average from.
        /// If not specified, it uses the entire `array` instead.
        /// </param>
        /// <returns>Average of the whole vertex array or the portion specified in the `indexes` list.</returns>
        public static Vector2 Average(IList<Vector2> array, IList<int> indexes = null)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            Vector2 sum = Vector2.zero;
            float len = indexes == null ? array.Count : indexes.Count;

            if (indexes == null)
                for (int i = 0; i < len; i++) sum += array[i];
            else
                for (int i = 0; i < len; i++) sum += array[indexes[i]];

            return sum / len;
        }

        /// <summary>
        /// Calculates and returns the average of the specified Vector3 array.
        /// </summary>
        /// <param name="array">The array to get the average for.</param>
        /// <param name="indexes">
        /// Specify a list of points in the vector array to calculate the average from.
        /// If not specified, it uses the entire `array` instead.
        /// </param>
        /// <returns>Average of the whole vertex array or the portion specified in the `indexes` list.</returns>
        public static Vector3 Average(IList<Vector3> array, IList<int> indexes = null)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            Vector3 sum = Vector3.zero;

            float len = indexes == null ? array.Count : indexes.Count;

            if (indexes == null)
            {
                for (int i = 0; i < len; i++)
                {
                    sum.x += array[i].x;
                    sum.y += array[i].y;
                    sum.z += array[i].z;
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    sum.x += array[indexes[i]].x;
                    sum.y += array[indexes[i]].y;
                    sum.z += array[indexes[i]].z;
                }
            }

            return sum / len;
        }

        /// <summary>
        /// Calculates and returns the average of a Vector4 array.
        /// </summary>
        /// <param name="array">The array to get the average for.</param>
        /// <param name="indexes">
        /// Specify a list of points in the vector array to calculate the average from.
        /// If not specified, it uses the entire `array` instead.
        /// </param>
        /// <returns>Average of the whole vertex array or the portion specified in the `indexes` list.</returns>
        public static Vector4 Average(IList<Vector4> array, IList<int> indexes = null)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            Vector4 sum = Vector3.zero;

            float len = indexes == null ? array.Count : indexes.Count;

            if (indexes == null)
            {
                for (int i = 0; i < len; i++)
                {
                    sum.x += array[i].x;
                    sum.y += array[i].y;
                    sum.z += array[i].z;
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    sum.x += array[indexes[i]].x;
                    sum.y += array[indexes[i]].y;
                    sum.z += array[indexes[i]].z;
                }
            }

            return sum / len;
        }

        internal static Vector3 InvertScaleVector(Vector3 scaleVector)
        {
            for (int axis = 0; axis < 3; ++axis)
                scaleVector[axis] = scaleVector[axis] == 0f ? 0f : 1f / scaleVector[axis];
            return scaleVector;
        }

        /// <summary>
        /// Compares two Vector2 values component-wise, allowing for a margin of error.
        /// </summary>
        /// <param name="a">First Vector2 value.</param>
        /// <param name="b">Second Vector2 value.</param>
        /// <param name="delta">The maximum difference between components allowed.</param>
        /// <returns>True if a and b components are respectively within delta distance of one another.</returns>
        internal static bool Approx2(this Vector2 a, Vector2 b, float delta = k_FltCompareEpsilon)
        {
            return
                Mathf.Abs(a.x - b.x) < delta &&
                Mathf.Abs(a.y - b.y) < delta;
        }

        /// <summary>
        /// Compares two Vector3 values component-wise, allowing for a margin of error.
        /// </summary>
        /// <param name="a">First Vector3 value.</param>
        /// <param name="b">Second Vector3 value.</param>
        /// <param name="delta">The maximum difference between components allowed.</param>
        /// <returns>True if a and b components are respectively within delta distance of one another.</returns>
        internal static bool Approx3(this Vector3 a, Vector3 b, float delta = k_FltCompareEpsilon)
        {
            return
                Mathf.Abs(a.x - b.x) < delta &&
                Mathf.Abs(a.y - b.y) < delta &&
                Mathf.Abs(a.z - b.z) < delta;
        }

        /// <summary>
        /// Compares two Vector4 values component-wise, allowing for a margin of error.
        /// </summary>
        /// <param name="a">First Vector4 value.</param>
        /// <param name="b">Second Vector4 value.</param>
        /// <param name="delta">The maximum difference between components allowed.</param>
        /// <returns>True if a and b components are respectively within delta distance of one another.</returns>

        internal static bool Approx4(this Vector4 a, Vector4 b, float delta = k_FltCompareEpsilon)
        {
            return
                Mathf.Abs(a.x - b.x) < delta &&
                Mathf.Abs(a.y - b.y) < delta &&
                Mathf.Abs(a.z - b.z) < delta &&
                Mathf.Abs(a.w - b.w) < delta;
        }

        /// <summary>
        /// Compares two Color values component-wise, allowing for a margin of error.
        /// </summary>
        /// <param name="a">First Color value.</param>
        /// <param name="b">Second Color value.</param>
        /// <param name="delta">The maximum difference between components allowed.</param>
        /// <returns>True if a and b components are respectively within delta distance of one another.</returns>
        internal static bool ApproxC(this Color a, Color b, float delta = k_FltCompareEpsilon)
        {
            return Mathf.Abs(a.r - b.r) < delta &&
                Mathf.Abs(a.g - b.g) < delta &&
                Mathf.Abs(a.b - b.b) < delta &&
                Mathf.Abs(a.a - b.a) < delta;
        }

        /// <summary>
        /// Compares two float values component-wise, allowing for a margin of error.
        /// </summary>
        /// <param name="a">First float value.</param>
        /// <param name="b">Second float value.</param>
        /// <param name="delta">The maximum difference between components allowed.</param>
        /// <returns>True if a and b components are respectively within delta distance of one another.</returns>
        internal static bool Approx(this float a, float b, float delta = k_FltCompareEpsilon)
        {
            return Mathf.Abs(b - a) < Mathf.Abs(delta);
        }

        /// <summary>
        /// Clamps an integer value to the specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="lowerBound">The lowest value that the clamped value can be.</param>
        /// <param name="upperBound">The highest value that the clamped value can be.</param>
        /// <returns>A value clamped inside the range of `lowerBound` and `upperBound`.</returns>
        public static int Clamp(int value, int lowerBound, int upperBound)
        {
            return value < lowerBound
                ? lowerBound
                : value > upperBound
                    ? upperBound
                    : value;
        }

        internal static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        internal static Vector3 Sign(this Vector3 v)
        {
            return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
        }

        internal static float Sum(this Vector3 v)
        {
            return Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
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

        internal static bool IsNumber(float value)
        {
            return !(float.IsInfinity(value) || float.IsNaN(value));
        }

        internal static bool IsNumber(Vector2 value)
        {
            return IsNumber(value.x) && IsNumber(value.y);
        }

        internal static bool IsNumber(Vector3 value)
        {
            return IsNumber(value.x) && IsNumber(value.y) && IsNumber(value.z);
        }

        internal static bool IsNumber(Vector4 value)
        {
            return IsNumber(value.x) && IsNumber(value.y) && IsNumber(value.z) && IsNumber(value.w);
        }

        internal static float MakeNonZero(float value, float min = .0001f)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || Mathf.Abs(value) < min)
                return min * Mathf.Sign(value);
            return value;
        }

        // Ensure that a vector does not contain NaN or INF values. This should only be used in repair functions to clean
        // up imported or otherwise broken geometry. Mesh operations should not by default produce NaN or INF values.
        internal static Vector4 FixNaN(Vector4 value)
        {
            value.x = IsNumber(value.x) ? value.x : 0f;
            value.y = IsNumber(value.y) ? value.y : 0f;
            value.z = IsNumber(value.z) ? value.z : 0f;
            value.w = IsNumber(value.w) ? value.w : 0f;
            return value;
        }

        internal static Vector2 EnsureUnitVector(Vector2 value)
        {
            return Mathf.Abs(value.sqrMagnitude) < float.Epsilon ? Vector2.right : value.normalized;
        }
        
        internal static Vector3 EnsureUnitVector(Vector3 value)
        {
            return Mathf.Abs(value.sqrMagnitude) < float.Epsilon ? Vector3.up : value.normalized;
        }

        internal static Vector4 EnsureUnitVector(Vector4 value)
        {
            var dir = EnsureUnitVector((Vector3)value);
            return new Vector4(dir.x, dir.y, dir.z, MakeNonZero(value.w, 1f));
        }
    }
}
