using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Snapping functions and ProGrids compatibility.
    /// </summary>
    static class Snapping
    {
        const float k_MaxRaySnapDistance = Mathf.Infinity;

        /// <summary>
        /// Round value to nearest snpVal increment.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="snpVal"></param>
        /// <returns></returns>
        public static Vector3 SnapValue(Vector3 vertex, float snpVal)
        {
            // snapValue is a global setting that comes from ProGrids
            return new Vector3(
                snpVal * Mathf.Round(vertex.x / snpVal),
                snpVal * Mathf.Round(vertex.y / snpVal),
                snpVal * Mathf.Round(vertex.z / snpVal));
        }

        /// <summary>
        /// Round value to nearest snpVal increment.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="snpVal"></param>
        /// <returns></returns>
        public static float SnapValue(float val, float snpVal)
        {
            if (snpVal < Mathf.Epsilon)
                return val;
            return snpVal * Mathf.Round(val / snpVal);
        }

        /// <summary>
        /// An override that accepts a vector3 to use as a mask for which values to snap.  Ex;
        /// Snap((.3f, 3f, 41f), (0f, 1f, .4f)) only snaps Y and Z values (to 1 & .4 unit increments).
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static Vector3 SnapValue(Vector3 vertex, Vector3 snap)
        {
            float x = vertex.x, y = vertex.y, z = vertex.z;
            Vector3 v = new Vector3(
                    (Mathf.Abs(snap.x) < 0.0001f ? x : snap.x * Mathf.Round(x / snap.x)),
                    (Mathf.Abs(snap.y) < 0.0001f ? y : snap.y * Mathf.Round(y / snap.y)),
                    (Mathf.Abs(snap.z) < 0.0001f ? z : snap.z * Mathf.Round(z / snap.z))
                    );
            return v;
        }

        public static Vector3 Floor(Vector3 vertex, Vector3 snap)
        {
            float x = vertex.x, y = vertex.y, z = vertex.z;
            Vector3 v = new Vector3(
                    (Mathf.Abs(snap.x) < 0.0001f ? x : snap.x * Mathf.Floor(x / snap.x)),
                    (Mathf.Abs(snap.y) < 0.0001f ? y : snap.y * Mathf.Floor(y / snap.y)),
                    (Mathf.Abs(snap.z) < 0.0001f ? z : snap.z * Mathf.Floor(z / snap.z))
                    );
            return v;
        }

        public static Vector3 Ceil(Vector3 vertex, Vector3 snap)
        {
            float x = vertex.x, y = vertex.y, z = vertex.z;
            Vector3 v = new Vector3(
                    (Mathf.Abs(snap.x) < 0.0001f ? x : snap.x * Mathf.Ceil(x / snap.x)),
                    (Mathf.Abs(snap.y) < 0.0001f ? y : snap.y * Mathf.Ceil(y / snap.y)),
                    (Mathf.Abs(snap.z) < 0.0001f ? z : snap.z * Mathf.Ceil(z / snap.z))
                    );
            return v;
        }

        public static Vector3 Ceil(Vector3 vertex, float snpVal)
        {
            return new Vector3(
                snpVal * Mathf.Ceil(vertex.x / snpVal),
                snpVal * Mathf.Ceil(vertex.y / snpVal),
                snpVal * Mathf.Ceil(vertex.z / snpVal));
        }

        public static Vector3 Floor(Vector3 vertex, float snpVal)
        {
            // snapValue is a global setting that comes from ProGrids
            return new Vector3(
                snpVal * Mathf.Floor(vertex.x / snpVal),
                snpVal * Mathf.Floor(vertex.y / snpVal),
                snpVal * Mathf.Floor(vertex.z / snpVal));
        }

        /// <summary>
        /// Snap all vertices to an increment of @snapValue in world space.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        /// <param name="snap"></param>
        public static void SnapVertices(ProBuilderMesh mesh, IEnumerable<int> indexes, Vector3 snap)
        {
            Vector3[] verts = mesh.positionsInternal;

            foreach (var v in indexes)
                verts[v] = mesh.transform.InverseTransformPoint(SnapValue(mesh.transform.TransformPoint(verts[v]), snap));
        }

        internal static Vector3 GetSnappingMaskBasedOnNormalVector(Vector3 normal)
        {
            return new Vector3(
                (Mathf.Approximately(Mathf.Abs(normal.x), 1f)) ? 0f : 1f,
                (Mathf.Approximately(Mathf.Abs(normal.y), 1f)) ? 0f : 1f,
                (Mathf.Approximately(Mathf.Abs(normal.z), 1f)) ? 0f : 1f);
        }

        internal static Vector3 SnapValueOnRay(Ray ray, float distance, float snap, Vector3Mask mask)
        {
            var nearest = k_MaxRaySnapDistance;

            var forwardRay = new Ray(ray.origin, ray.direction);
            var backwardsRay = new Ray(ray.origin, -ray.direction);

            for (int i = 0; i < 3; i++)
            {
                if (mask[i] > 0f)
                {
                    var dir = new Vector3Mask(new Vector3Mask((byte) (1 << i)));

                    var prj = Vector3.Project(
                        ray.direction * Math.MakeNonZero(distance),
                        dir * Mathf.Sign(ray.direction[i]));

                    var pnt = ray.origin + prj;
                    var plane = new Plane(dir, SnapValue(pnt, dir * snap));

                    if(Mathf.Abs(plane.GetDistanceToPoint(ray.origin)) < .0001f)
                    {
                        nearest = 0f;
                        continue;
                    }

                    float d;

                    if (plane.Raycast(forwardRay, out d) && Mathf.Abs(d) < Mathf.Abs(nearest))
                        nearest = d;
                    if (plane.Raycast(backwardsRay, out d) && Mathf.Abs(d) < Mathf.Abs(nearest))
                        nearest = -d;
                }
            }

            return ray.origin + ray.direction * (Mathf.Abs(nearest) >= k_MaxRaySnapDistance ? distance : nearest);
        }
    }
}
