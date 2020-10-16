using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Snapping functions (didn't exist in UnityEngine prior to 2019.3)
    /// </summary>
    static class ProBuilderSnapping
    {
        const float k_MaxRaySnapDistance = Mathf.Infinity;

        internal static bool IsCardinalDirection(Vector3 direction)
        {
            return
                Mathf.Abs(direction.x) > 0f && Mathf.Approximately(direction.y, 0f) && Mathf.Approximately(direction.z, 0f)
                || Mathf.Abs(direction.y) > 0f && Mathf.Approximately(direction.x, 0f) && Mathf.Approximately(direction.z, 0f)
                || Mathf.Abs(direction.z) > 0f && Mathf.Approximately(direction.x, 0f) && Mathf.Approximately(direction.y, 0f);
        }

        public static float Snap(float val, float snap)
        {
            if (snap == 0)
                return val;

            return snap * Mathf.Round(val / snap);
        }

        public static Vector3 Snap(Vector3 val, Vector3 snap)
        {
            return new Vector3(
                (Mathf.Abs(snap.x) > 0.0001f) ? Snap(val.x, snap.x) : val.x,
                (Mathf.Abs(snap.y) > 0.0001f) ? Snap(val.y, snap.y) : val.y,
                (Mathf.Abs(snap.z) > 0.0001f) ? Snap(val.z, snap.z) : val.z
            );
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
                verts[v] = mesh.transform.InverseTransformPoint(Snap(mesh.transform.TransformPoint(verts[v]), snap));
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
                    var plane = new Plane(dir, Snap(pnt, dir * snap));

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
