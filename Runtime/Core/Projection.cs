using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Contains functions for projecting 3D points to 2D space.
    /// </summary>
    public static class Projection
    {
        /// <summary>
        /// Projects a collection of 3D positions to a 2D plane. This method uses <see cref="FindBestPlane"/> to calculate
        /// the direction from which the vertices are projected.
        /// </summary>
        /// <param name="positions">A collection of positions to project based on a direction.</param>
        /// <param name="indexes">A collection of indices to project. The returned array matches the length of this collection.</param>
        /// <returns>The positions array projected into 2D coordinates.</returns>
        public static Vector2[] PlanarProject(IList<Vector3> positions, IList<int> indexes = null)
        {
            return PlanarProject(positions, indexes, FindBestPlane(positions, indexes).normal);
        }

        /// <summary>
        /// Projects a collection of 3D positions to a 2D plane in the specified direction.
        /// </summary>
        /// <param name="positions">A collection of positions to project based on a direction.</param>
        /// <param name="indexes">A collection of indices to project. The returned array matches the length of this collection.</param>
        /// <param name="direction">The direction from which vertex positions are projected into 2D space.</param>
        /// <returns>The positions array projected into 2D coordinates.</returns>
        public static Vector2[] PlanarProject(IList<Vector3> positions, IList<int> indexes, Vector3 direction)
        {
            List<Vector2> results = new List<Vector2>(indexes != null ? indexes.Count : positions.Count);
            PlanarProject(positions, indexes, direction, results);
            return results.ToArray();
        }

        internal static void PlanarProject(IList<Vector3> positions, IList<int> indexes, Vector3 direction, List<Vector2> results)
        {
            if(positions == null)
                throw new ArgumentNullException("positions");

            if(results == null)
                throw new ArgumentNullException("results");

            var nrm = Math.EnsureUnitVector(direction);
            var axis = VectorToProjectionAxis(nrm);
            var prj = GetTangentToAxis(axis);
            var len = indexes == null ? positions.Count : indexes.Count;
            results.Clear();

            var u = Vector3.Cross(nrm, prj);
            var v = Vector3.Cross(u, nrm);

            u.Normalize();
            v.Normalize();

            if (indexes != null)
            {
                for (int i = 0, ic = len; i < ic; ++i)
                    results.Add(new Vector2(Vector3.Dot(u, positions[indexes[i]]), Vector3.Dot(v, positions[indexes[i]])));
            }
            else
            {
                for (int i = 0, ic = len; i < ic; ++i)
                    results.Add(new Vector2(Vector3.Dot(u, positions[i]), Vector3.Dot(v, positions[i])));
            }
        }

        internal static void PlanarProject(ProBuilderMesh mesh, int textureGroup, AutoUnwrapSettings unwrapSettings)
        {
            var worldSpace = unwrapSettings.useWorldSpace;
            var trs = (Transform)null;
            var faces = mesh.facesInternal;

            // Get a projection direction by averaging the normals of all selected faces
            var projectionDirection = Vector3.zero;

            for (int f = 0, fc = faces.Length; f < fc; ++f)
            {
                if (faces[f].textureGroup != textureGroup)
                    continue;

                var nrm = Math.Normal(mesh, faces[f]);
                projectionDirection += nrm;
            }

            if (worldSpace)
            {
                trs = mesh.transform;
                projectionDirection = trs.TransformDirection(projectionDirection);
            }

            var axis = VectorToProjectionAxis(projectionDirection);
            var prj = GetTangentToAxis(axis);

            var u = Vector3.Cross(projectionDirection, prj);
            var v = Vector3.Cross(u, projectionDirection);

            u.Normalize();
            v.Normalize();

            var positions = mesh.positionsInternal;
            var textures = mesh.texturesInternal;

            for (int f = 0, fc = faces.Length; f < fc; ++f)
            {
                if (faces[f].textureGroup != textureGroup)
                    continue;

                var indexes = faces[f].distinctIndexesInternal;

                for (int i = 0, ic = indexes.Length; i < ic; ++i)
                {
                    var p = worldSpace ? trs.TransformPoint(positions[indexes[i]]) : positions[indexes[i]];

                    textures[indexes[i]].x = Vector3.Dot(u, p);
                    textures[indexes[i]].y = Vector3.Dot(v, p);
                }
            }
        }

        internal static void PlanarProject(ProBuilderMesh mesh, Face face, Vector3 projection = default)
        {
            var nrm = Math.EnsureUnitVector(Math.Normal(mesh, face));
            var trs = (Transform)null;
            var worldSpace = face.uv.useWorldSpace;

            if (worldSpace)
            {
                trs = mesh.transform;
                nrm = trs.TransformDirection(nrm);
            }

            Vector3 prj = projection;
            if (prj == Vector3.zero)
            {
                var axis = VectorToProjectionAxis(nrm);
                prj = GetTangentToAxis(axis);
            }

            var uAxis = Vector3.Cross(nrm, prj);
            var vAxis = Vector3.Cross(uAxis, nrm);

            uAxis.Normalize();
            vAxis.Normalize();

            var positions = mesh.positionsInternal;
            var textures = mesh.texturesInternal;

            int[] indexes = face.distinctIndexesInternal;

            for (int i = 0, ic = indexes.Length; i < ic; ++i)
            {
                var p = worldSpace ? trs.TransformPoint(positions[indexes[i]]) : positions[indexes[i]];

                textures[indexes[i]].x = Vector3.Dot(uAxis, p);
                textures[indexes[i]].y = Vector3.Dot(vAxis, p);
            }
        }

        internal static Vector2[] SphericalProject(IList<Vector3> vertices, IList<int> indexes = null)
        {
            int len = indexes == null ? vertices.Count : indexes.Count;
            Vector2[] uv = new Vector2[len];
            Vector3 cen = Math.Average(vertices, indexes);

            for (int i = 0; i < len; i++)
            {
                int indx = indexes == null ? i : indexes[i];
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

            for (int i = 0; i < count; i++)
                angles.Add(new SimpleTuple<float, Vector2>(Math.SignedAngle(up, verts[i] - cen), verts[i]));

            angles.Sort((a, b) => { return a.item1 < b.item1 ? -1 : 1; });

            IList<Vector2> values = angles.Select(x => x.item2).ToList();

            if (method == SortMethod.Clockwise)
                values = values.Reverse().ToList();

            return values;
        }

        internal static Vector3 GetTangentToAxis(ProjectionAxis axis)
        {
            // old probuilder didn't respect project axis settings properly, and changing it to the correct version
            // (ProjectionAxisToVector) would break existing models.
            switch (axis)
            {
                case ProjectionAxis.X:
                case ProjectionAxis.XNegative:
                    return Vector3.up;

                case ProjectionAxis.Y:
                case ProjectionAxis.YNegative:
                    return Vector3.forward;

                case ProjectionAxis.Z:
                case ProjectionAxis.ZNegative:
                    return Vector3.up;

                default:
                    return Vector3.up;
            }
        }

        /// <summary>
        /// Given a ProjectionAxis, return  the appropriate Vector3 conversion.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        internal static Vector3 ProjectionAxisToVector(ProjectionAxis axis)
        {
            switch (axis)
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
                    return Vector3.forward;
            }
        }

        /// <summary>
        /// Returns a projection axis based on which axis is the largest
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        internal static ProjectionAxis VectorToProjectionAxis(Vector3 direction)
        {
            float x = System.Math.Abs(direction.x);
            float y = System.Math.Abs(direction.y);
            float z = System.Math.Abs(direction.z);

            // Prior to 2019.1 Vector3.Normalize did not use System.Math for Magnitude calculations. There is a slight
            // difference between 2018.3 and 2019.1 in accuracy, with 2019.1 being more correct. This inaccuracy results
            // in float comparison problems (ex, .00700000001 > .007). To account for that we add an additional equality
            // check here.
            // In most other cases this doesn't matter, but since we need this function to be deterministic regardless
            // of floating point inaccuracy it is necessary here.
            if (!Math.Approx(x, y) && x > y && !Math.Approx(x, z) && x > z)
                return direction.x > 0 ? ProjectionAxis.X : ProjectionAxis.XNegative;

            if (!Math.Approx(y, z) && y > z)
                return direction.y > 0 ? ProjectionAxis.Y : ProjectionAxis.YNegative;

            return direction.z > 0 ? ProjectionAxis.Z : ProjectionAxis.ZNegative;
        }

        /// <summary>
        /// Finds the plane that best fits the specified set of 3D points.
        /// </summary>
        /// <remarks>http://www.ilikebigbits.com/blog/2015/3/2/plane-from-points</remarks>
        /// <param name="points">The points to find a plane for. Order does not matter.</param>
        /// <param name="indexes">If provided, this method considers only the vertices referenced by this array.</param>
        /// <returns>The plane that best matches the layout of the points array.</returns>
        public static Plane FindBestPlane(IList<Vector3> points, IList<int> indexes = null)
        {
            float   xx = 0f, xy = 0f, xz = 0f,
                    yy = 0f, yz = 0f, zz = 0f;

            if (points == null)
                throw new System.ArgumentNullException("points");

            bool ind = indexes != null && indexes.Count > 0;
            int len = ind ? indexes.Count : points.Count;

            Vector3 c = Vector3.zero, n = Vector3.zero;

            for (int i = 0; i < len; i++)
            {
                c.x += points[ind ? indexes[i] : i].x;
                c.y += points[ind ? indexes[i] : i].y;
                c.z += points[ind ? indexes[i] : i].z;
            }

            c.x /= (float)len;
            c.y /= (float)len;
            c.z /= (float)len;

            for (int i = 0; i < len; i++)
            {
                Vector3 r = points[ind ? indexes[i] : i] - c;

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

            if (det_x > det_y && det_x > det_z)
            {
                n.x = 1f;
                n.y = (xz * yz - xy * zz) / det_x;
                n.z = (xy * yz - xz * yy) / det_x;
            }
            else if (det_y > det_z)
            {
                n.x = (yz * xz - xy * zz) / det_y;
                n.y = 1f;
                n.z = (xy * xz - yz * xx) / det_y;
            }
            else
            {
                n.x = (yz * xy - xz * yy) / det_z;
                n.y = (xz * xy - yz * xx) / det_z;
                n.z = 1f;
            }

            n.Normalize();

            return new Plane(n, c);
        }

        /// <summary>
        /// Find a plane that best fits a set of faces within a texture group.
        /// </summary>
        /// <returns>A plane that best matches the layout of the points array.</returns>
        internal static Plane FindBestPlane(ProBuilderMesh mesh, int textureGroup)
        {
            float   xx = 0f, xy = 0f, xz = 0f,
                    yy = 0f, yz = 0f, zz = 0f;

            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            Vector3 c = Vector3.zero;
            int len = 0;
            Vector3[] positions = mesh.positionsInternal;
            int faceCount = mesh.faceCount;
            Face[] faces = mesh.facesInternal;

            for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
            {
                if (faces[faceIndex].textureGroup != textureGroup)
                    continue;

                int[] indexes = faces[faceIndex].indexesInternal;

                for (int index = 0, indexCount = indexes.Length; index < indexCount; index++)
                {
                    c.x += positions[indexes[index]].x;
                    c.y += positions[indexes[index]].y;
                    c.z += positions[indexes[index]].z;

                    len++;
                }
            }

            c.x /= len;
            c.y /= len;
            c.z /= len;

            for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
            {
                if (faces[faceIndex].textureGroup != textureGroup)
                    continue;

                int[] indexes = faces[faceIndex].indexesInternal;

                for (int index = 0, indexCount = indexes.Length; index < indexCount; index++)
                {
                    Vector3 r = positions[indexes[index]] - c;

                    xx += r.x * r.x;
                    xy += r.x * r.y;
                    xz += r.x * r.z;
                    yy += r.y * r.y;
                    yz += r.y * r.z;
                    zz += r.z * r.z;
                }
            }

            float det_x = yy * zz - yz * yz;
            float det_y = xx * zz - xz * xz;
            float det_z = xx * yy - xy * xy;
            Vector3 n = Vector3.zero;

            if (det_x > det_y && det_x > det_z)
            {
                n.x = 1f;
                n.y = (xz * yz - xy * zz) / det_x;
                n.z = (xy * yz - xz * yy) / det_x;
            }
            else if (det_y > det_z)
            {
                n.x = (yz * xz - xy * zz) / det_y;
                n.y = 1f;
                n.z = (xy * xz - yz * xx) / det_y;
            }
            else
            {
                n.x = (yz * xy - xz * yy) / det_z;
                n.y = (xz * xy - yz * xx) / det_z;
                n.z = 1f;
            }

            n.Normalize();

            return new Plane(n, c);
        }
    }
}
