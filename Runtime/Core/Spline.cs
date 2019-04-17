using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    static class Spline
    {
        /// <summary>
        /// Create a new pb_Object by extruding along a bezier spline.
        /// </summary>
        /// <param name="points">The points making up the bezier spline.</param>
        /// <param name="radius">The radius of the extruded mesh tube.</param>
        /// <param name="columns">How many columns per segment to create when extruding the mesh.</param>
        /// <param name="rows">How many rows the extruded mesh will be composed of.</param>
        /// <param name="closeLoop">Should the mesh join at the ends or remain unconnected.</param>
        /// <param name="smooth">Are the mesh edges smoothed or hard.</param>
        /// <returns>The resulting pb_Object.</returns>
        internal static ProBuilderMesh Extrude(IList<BezierPoint> points,
            float radius = .5f,
            int columns = 32,
            int rows = 16,
            bool closeLoop = false,
            bool smooth = true)
        {
            ProBuilderMesh pb = null;
            Extrude(points, radius, columns, rows, closeLoop, smooth, ref pb);
            return pb;
        }

        // Update a pb_Object with new geometry from a bezier spline.
        internal static void Extrude(IList<BezierPoint> bezierPoints,
            float radius,
            int columns,
            int rows,
            bool closeLoop,
            bool smooth,
            ref ProBuilderMesh target)
        {
            List<Quaternion> rotations = new List<Quaternion>();
            List<Vector3> positions = GetControlPoints(bezierPoints, columns, closeLoop, rotations);
            Extrude(positions, radius, rows, closeLoop, smooth, ref target, rotations);
        }

        // Extrapolate a bezier curve to it's control points and segments between.
        internal static List<Vector3> GetControlPoints(IList<BezierPoint> bezierPoints, int subdivisionsPerSegment, bool closeLoop, List<Quaternion> rotations)
        {
            int cols = subdivisionsPerSegment;
            int c = bezierPoints.Count;
            List<Vector3> positions = new List<Vector3>(cols * c);

            if (rotations != null)
            {
                rotations.Clear();
                rotations.Capacity = cols * c;
            }

            int keyframes = (closeLoop ? c : c - 1);

            for (int i = 0; i < keyframes; i++)
            {
                int segments_per_keyframe = ((!closeLoop && i >= c - 2) ? cols + 1 : cols);

                for (int n = 0; n < segments_per_keyframe; n++)
                {
                    float s = cols;

                    positions.Add(BezierPoint.CubicPosition(bezierPoints[i], bezierPoints[(i + 1) % c], n / s));

                    if (rotations != null)
                        rotations.Add(Quaternion.Slerp(bezierPoints[i].rotation, bezierPoints[(i + 1) % c].rotation, n / (float)(segments_per_keyframe - 1)));
                }
            }

            return positions;
        }

        // Set mesh geometry by extruding along a set of points.
        internal static void Extrude(IList<Vector3> points,
            float radius,
            int radiusRows,
            bool closeLoop,
            bool smooth,
            ref ProBuilderMesh target,
            IList<Quaternion> pointRotations = null)
        {
            if (points == null || points.Count < 2)
                return;

            int cnt = points.Count;
            int rows = System.Math.Max(3, radiusRows);
            int rowsPlus1 = rows + 1;
            int rowsPlus1Times2 = rows * 2;
            int vertexCount = ((closeLoop ? cnt : cnt - 1) * 2) * rowsPlus1Times2;
            bool vertexCountsMatch = false; // vertexCount == (target == null ? 0 : target.vertexCount);
            bool hasPointRotations = pointRotations != null && pointRotations.Count == points.Count;

            Vector3[] positions = new Vector3[vertexCount];
            Face[] faces = vertexCountsMatch ? null : new Face[(closeLoop ? cnt : cnt - 1) * rows];

            int triangleIndex = 0, faceIndex = 0, vertexIndex = 0;
            int segmentCount = (closeLoop ? cnt : cnt - 1);

            for (int i = 0; i < segmentCount; i++)
            {
                float secant_a, secant_b;

                Quaternion rotation_a = GetRingRotation(points, i, closeLoop, out secant_a);
                Quaternion rotation_b = GetRingRotation(points, (i + 1) % cnt, closeLoop, out secant_b);

                if (hasPointRotations)
                {
                    rotation_a = rotation_a * pointRotations[i];
                    rotation_b = rotation_b * pointRotations[(i + 1) % cnt];
                }

                Vector3[] ringA = VertexRing(rotation_a, points[i], radius, rowsPlus1);
                Vector3[] ringB = VertexRing(rotation_b, points[(i + 1) % cnt], radius, rowsPlus1);

                System.Array.Copy(ringA, 0, positions, vertexIndex, rowsPlus1Times2);
                vertexIndex += rowsPlus1Times2;
                System.Array.Copy(ringB, 0, positions, vertexIndex, rowsPlus1Times2);
                vertexIndex += rowsPlus1Times2;

                if (!vertexCountsMatch)
                {
                    for (int n = 0; n < rowsPlus1Times2; n += 2)
                    {
                        faces[faceIndex] = new Face(new int[6]
                        {
                            triangleIndex, triangleIndex + 1, triangleIndex + rowsPlus1Times2,
                            triangleIndex + rowsPlus1Times2, triangleIndex + 1, triangleIndex + rowsPlus1Times2 + 1
                        });

                        if (smooth)
                            faces[faceIndex].smoothingGroup = 2;

                        faceIndex++;
                        triangleIndex += 2;
                    }

                    triangleIndex += rowsPlus1Times2;
                }
            }

            if (target != null)
            {
                if (faces != null)
                {
                    target.RebuildWithPositionsAndFaces(positions, faces);
                }
                else
                {
                    target.positions = positions;
                    target.ToMesh();
                    target.Refresh(RefreshMask.UV | RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents);
                }
            }
            else
            {
                target = ProBuilderMesh.Create(positions, faces);
            }
        }

        static Quaternion GetRingRotation(IList<Vector3> points, int i, bool closeLoop, out float secant)
        {
            int cnt = points.Count;
            Vector3 dir;

            if (closeLoop || (i > 0 && i < cnt - 1))
            {
                int a = i < 1 ? cnt - 1 : i - 1;
                int b = i;
                int c = (i + 1) % cnt;

                Vector3 coming = (points[b] - points[a]).normalized;
                Vector3 leaving = (points[c] - points[b]).normalized;

                dir = (coming + leaving) * .5f;

                secant = Math.Secant(Vector3.Angle(coming, dir) * Mathf.Deg2Rad);
            }
            else
            {
                if (i < 1)
                    dir = points[i + 1] - points[i];
                else
                    dir = points[i] - points[i - 1];

                secant = 1f;
            }

            dir.Normalize();

            if (Math.Approx3(dir, Vector3.up) || Math.Approx3(dir, Vector3.zero))
                return Quaternion.identity;

            return Quaternion.LookRotation(dir);
        }

        static Vector3[] VertexRing(Quaternion orientation, Vector3 offset, float radius, int segments)
        {
            Vector3[] v = new Vector3[segments * 2];

            for (int i = 0; i < segments; i++)
            {
                float rad0 = (i / (float)(segments - 1)) * 360f * Mathf.Deg2Rad;
                int n = (i + 1) % segments;
                float rad1 = (n / (float)(segments - 1)) * 360f * Mathf.Deg2Rad;

                v[i * 2] = offset + (orientation * new Vector3(Mathf.Cos(rad0) * radius, Mathf.Sin(rad0) * radius, 0f));
                v[i * 2 + 1] = offset + (orientation * new Vector3(Mathf.Cos(rad1) * radius, Mathf.Sin(rad1) * radius, 0f));
            }

            return v;
        }
    }
}
