using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Provides static methods for working with ProBuilderMesh objects in the Editor.
    /// </summary>
    public static class HandleUtility
    {
        /// <summary>
        /// Convert a screen point (0,0 bottom left, in pixels) to a GUI point (0,0 top left, in points).
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="point"></param>
        /// <param name="pixelsPerPoint"></param>
        /// <returns></returns>
        internal static Vector3 ScreenToGuiPoint(this Camera camera, Vector3 point, float pixelsPerPoint)
        {
            return new Vector3(point.x / pixelsPerPoint, (camera.pixelHeight - point.y) / pixelsPerPoint, point.z);
        }

        /// <summary>
        /// Find a triangle intersected by InRay on InMesh.  InRay is in world space.
        /// Returns the index in mesh.faces of the hit face, or -1.  Optionally can ignore backfaces.
        /// </summary>
        /// <param name="worldRay"></param>
        /// <param name="mesh"></param>
        /// <param name="hit"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        internal static bool FaceRaycast(Ray worldRay, ProBuilderMesh mesh, out RaycastHit hit, HashSet<Face> ignore = null)
        {
            return FaceRaycast(worldRay, mesh, out hit, Mathf.Infinity, CullingMode.Back, ignore);
        }

        /// <summary>
        /// Find the nearest face intersected by InWorldRay on this pb_Object.
        /// </summary>
        /// <param name="worldRay">A ray in world space.</param>
        /// <param name="mesh">The ProBuilder object to raycast against.</param>
        /// <param name="hit">If the mesh was intersected, hit contains information about the intersect point in local coordinate space.</param>
        /// <param name="distance">The distance from the ray origin to the intersection point.</param>
        /// <param name="cullingMode">Which sides of a face are culled when hit testing. Default is back faces are culled.</param>
        /// <param name="ignore">Optional collection of faces to ignore when raycasting.</param>
        /// <returns>True if the ray intersects with the mesh, false if not.</returns>
        internal static bool FaceRaycast(Ray worldRay, ProBuilderMesh mesh, out RaycastHit hit, float distance, CullingMode cullingMode, HashSet<Face> ignore = null)
        {
            // Transform ray into model space
            worldRay.origin -= mesh.transform.position; // Why doesn't worldToLocalMatrix apply translation?
            worldRay.origin = mesh.transform.worldToLocalMatrix * worldRay.origin;
            worldRay.direction = mesh.transform.worldToLocalMatrix * worldRay.direction;

            var positions = mesh.positionsInternal;
            var faces = mesh.facesInternal;

            float OutHitPoint = Mathf.Infinity;
            int OutHitFace = -1;
            Vector3 OutNrm = Vector3.zero;

            // Iterate faces, testing for nearest hit to ray origin. Optionally ignores backfaces.
            for (int i = 0, fc = faces.Length; i < fc; ++i)
            {
                if (ignore != null && ignore.Contains(faces[i]))
                    continue;

                int[] indexes = mesh.facesInternal[i].indexesInternal;

                for (int j = 0, ic = indexes.Length; j < ic; j += 3)
                {
                    Vector3 a = positions[indexes[j + 0]];
                    Vector3 b = positions[indexes[j + 1]];
                    Vector3 c = positions[indexes[j + 2]];

                    Vector3 nrm = Vector3.Cross(b - a, c - a);
                    float dot = Vector3.Dot(worldRay.direction, nrm);

                    bool skip = false;

                    switch (cullingMode)
                    {
                        case CullingMode.Front:
                            if (dot < 0f) skip = true;
                            break;

                        case CullingMode.Back:
                            if (dot > 0f) skip = true;
                            break;
                    }

                    var dist = 0f;

                    Vector3 point;
                    if (!skip && Math.RayIntersectsTriangle(worldRay, a, b, c, out dist, out point))
                    {
                        if (dist > OutHitPoint || dist > distance)
                            continue;

                        OutNrm = nrm;
                        OutHitFace = i;
                        OutHitPoint = dist;
                    }
                }
            }

            hit = new RaycastHit(OutHitPoint,
                    worldRay.GetPoint(OutHitPoint),
                    OutNrm,
                    OutHitFace);

            return OutHitFace > -1;
        }

        internal static bool FaceRaycastBothCullModes(Ray worldRay, ProBuilderMesh mesh, ref SimpleTuple<Face, Vector3> back, ref SimpleTuple<Face, Vector3> front)
        {
            // Transform ray into model space
            worldRay.origin -= mesh.transform.position; // Why doesn't worldToLocalMatrix apply translation?
            worldRay.origin = mesh.transform.worldToLocalMatrix * worldRay.origin;
            worldRay.direction = mesh.transform.worldToLocalMatrix * worldRay.direction;

            var positions = mesh.positionsInternal;
            var faces = mesh.facesInternal;

            back.item1 = null;
            front.item1 = null;

            float backDistance = Mathf.Infinity;
            float frontDistance = Mathf.Infinity;

            // Iterate faces, testing for nearest hit to ray origin. Optionally ignores backfaces.
            for (int i = 0, fc = faces.Length; i < fc; ++i)
            {
                int[] indexes = mesh.facesInternal[i].indexesInternal;

                for (int j = 0, ic = indexes.Length; j < ic; j += 3)
                {
                    Vector3 a = positions[indexes[j + 0]];
                    Vector3 b = positions[indexes[j + 1]];
                    Vector3 c = positions[indexes[j + 2]];

                    float dist;
                    Vector3 point;

                    if (Math.RayIntersectsTriangle(worldRay, a, b, c, out dist, out point))
                    {
                        if (dist < backDistance || dist < frontDistance)
                        {
                            Vector3 nrm = Vector3.Cross(b - a, c - a);
                            float dot = Vector3.Dot(worldRay.direction, nrm);

                            if (dot < 0f)
                            {
                                if (dist < backDistance)
                                {
                                    backDistance = dist;
                                    back.item1 = faces[i];
                                }
                            }
                            else
                            {
                                if (dist < frontDistance)
                                {
                                    frontDistance = dist;
                                    front.item1 = faces[i];
                                }
                            }
                        }
                    }
                }
            }

            if (back.item1 != null)
                back.item2 = worldRay.GetPoint(backDistance);

            if (front.item1 != null)
                front.item2 = worldRay.GetPoint(frontDistance);

            return back.item1 != null || front.item1 != null;
        }

        /// <summary>
        /// Find the all faces intersected by InWorldRay on this pb_Object.
        /// </summary>
        /// <param name="InWorldRay">A ray in world space.</param>
        /// <param name="mesh">The ProBuilder object to raycast against.</param>
        /// <param name="hits">If the mesh was intersected, hits contains all intersection point RaycastHit information.</param>
        /// <param name="cullingMode">What sides of triangles does the ray intersect with.</param>
        /// <param name="ignore">Optional collection of faces to ignore when raycasting.</param>
        /// <returns>True if the ray intersects with the mesh, false if not.</returns>
        internal static bool FaceRaycast(
            Ray InWorldRay,
            ProBuilderMesh mesh,
            out List<RaycastHit> hits,
            CullingMode cullingMode,
            HashSet<Face> ignore = null)
        {
            // Transform ray into model space
            InWorldRay.origin -= mesh.transform.position;  // Why doesn't worldToLocalMatrix apply translation?

            InWorldRay.origin       = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
            InWorldRay.direction    = mesh.transform.worldToLocalMatrix * InWorldRay.direction;

            Vector3[] vertices = mesh.positionsInternal;

            hits = new List<RaycastHit>();

            // Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
            for (int CurFace = 0; CurFace < mesh.facesInternal.Length; ++CurFace)
            {
                if (ignore != null && ignore.Contains(mesh.facesInternal[CurFace]))
                    continue;

                int[] indexes = mesh.facesInternal[CurFace].indexesInternal;

                for (int CurTriangle = 0; CurTriangle < indexes.Length; CurTriangle += 3)
                {
                    Vector3 a = vertices[indexes[CurTriangle + 0]];
                    Vector3 b = vertices[indexes[CurTriangle + 1]];
                    Vector3 c = vertices[indexes[CurTriangle + 2]];

                    var dist = 0f;
                    Vector3 point;

                    if (Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
                    {
                        Vector3 nrm = Vector3.Cross(b - a, c - a);

                        float dot; // vars used in loop
                        switch (cullingMode)
                        {
                            case CullingMode.Front:
                                dot = Vector3.Dot(InWorldRay.direction, nrm);

                                if (dot > 0f)
                                    goto case CullingMode.FrontBack;
                                break;

                            case CullingMode.Back:
                                dot = Vector3.Dot(InWorldRay.direction, nrm);

                                if (dot < 0f)
                                    goto case CullingMode.FrontBack;
                                break;

                            case CullingMode.FrontBack:
                                hits.Add(new RaycastHit(dist,
                                    InWorldRay.GetPoint(dist),
                                    nrm,
                                    CurFace));
                                break;
                        }

                        continue;
                    }
                }
            }

            return hits.Count > 0;
        }

        /// <summary>
        /// Transform a ray from world space to a transform local space.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="InWorldRay"></param>
        /// <returns></returns>
        internal static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
        {
            Vector3 o = InWorldRay.origin;
            o -= transform.position;
            o = transform.worldToLocalMatrix * o;
            Vector3 d = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
            return new Ray(o, d);
        }

        /// <summary>
        /// Find the nearest triangle intersected by InWorldRay on this mesh.
        /// </summary>
        /// <param name="InWorldRay"></param>
        /// <param name="hit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        internal static bool MeshRaycast(Ray InWorldRay, GameObject gameObject, out RaycastHit hit, float distance = Mathf.Infinity)
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var mesh = meshFilter != null ? meshFilter.sharedMesh : null;

            if (!mesh)
            {
                hit = default(RaycastHit);
                return false;
            }

            var transform = gameObject.transform;
            var ray = transform.InverseTransformRay(InWorldRay);
            return MeshRaycast(ray, mesh.vertices, mesh.triangles, out hit, distance);
        }

        /// <summary>
        /// Cast a ray (in model space) against a mesh.
        /// </summary>
        /// <param name="InRay"></param>
        /// <param name="mesh"></param>
        /// <param name="triangles"></param>
        /// <param name="hit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        internal static bool MeshRaycast(Ray InRay, Vector3[] mesh, int[] triangles, out RaycastHit hit, float distance = Mathf.Infinity)
        {
            // float dot;               // vars used in loop
            float hitDistance = Mathf.Infinity;
            Vector3 hitNormal = Vector3.zero;    // vars used in loop
            Vector3 a, b, c, n = Vector3.zero;
            int hitFace = -1;
            Vector3 o = InRay.origin, d = InRay.direction;

            // Iterate faces, testing for nearest hit to ray origin.
            for (int CurTri = 0; CurTri < triangles.Length; CurTri += 3)
            {
                a = mesh[triangles[CurTri + 0]];
                b = mesh[triangles[CurTri + 1]];
                c = mesh[triangles[CurTri + 2]];

                if (Math.RayIntersectsTriangle2(o, d, a, b, c, ref distance, ref n))
                {
                    if(distance < hitDistance)
                    {
                        hitFace = CurTri / 3;
                        hitDistance = distance;
                        hitNormal = n;
                    }
                }
            }

            hit = new RaycastHit(hitDistance,
                    InRay.GetPoint(hitDistance),
                    hitNormal,
                    hitFace);

            return hitFace > -1;
        }

        /// <summary>
        /// Returns true if this point in world space is occluded by a triangle on this object.
        /// </summary>
        /// <remarks>This is very slow, do not use.</remarks>
        /// <param name="cam"></param>
        /// <param name="pb"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        internal static bool PointIsOccluded(Camera cam, ProBuilderMesh pb, Vector3 worldPoint)
        {
            Vector3 dir = (cam.transform.position - worldPoint).normalized;

            // move the point slightly towards the camera to avoid colliding with its own triangle
            Ray ray = new Ray(worldPoint + dir * .0001f, dir);

            RaycastHit hit;

            return FaceRaycast(ray, pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), CullingMode.Front);
        }

        /// <summary>
        /// Collects coincident vertices and returns a rotation calculated from the average normal and bitangent.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="indices">Vertex indices to consider in the rotation calculations.</param>
        /// <returns>A rotation calculated from the average normal of each vertex.</returns>
        public static Quaternion GetRotation(ProBuilderMesh mesh, IEnumerable<int> indices)
        {
            if (!mesh.HasArrays(MeshArrays.Normal))
                Normals.CalculateNormals(mesh);

            if (!mesh.HasArrays(MeshArrays.Tangent))
                Normals.CalculateTangents(mesh);

            var normals = mesh.normalsInternal;
            var tangents = mesh.tangentsInternal;

            var nrm = Vector3.zero;
            var tan = Vector4.zero;
            float count = 0;

            foreach (var index in indices)
            {
                var n = normals[index];
                var t = tangents[index];

                nrm.x += n.x;
                nrm.y += n.y;
                nrm.z += n.z;

                tan.x += t.x;
                tan.y += t.y;
                tan.z += t.z;
                tan.w += t.w;

                count++;
            }

            nrm.x /= count;
            nrm.y /= count;
            nrm.z /= count;

            tan.x /= count;
            tan.y /= count;
            tan.z /= count;
            tan.w /= count;

            if (nrm == Vector3.zero || tan == Vector4.zero)
                return mesh.transform.rotation;

            var bit = Vector3.Cross(nrm, tan * tan.w);

            return mesh.transform.rotation * Quaternion.LookRotation(nrm, bit);
        }

        /// <summary>
        /// Returns a rotation suitable for orienting a handle or gizmo relative to the Face selection.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="orientation">The type of <see cref="HandleOrientation"/> to calculate.</param>
        /// <param name="faces">Which faces to consider in the rotation calculations. This is only used when the
        /// <see cref="HandleOrientation"/> is set to <see cref="HandleOrientation.ActiveElement"/>.</param>
        /// <returns>A rotation appropriate to the orientation and element selection.</returns>
        public static Quaternion GetFaceRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<Face> faces)
        {
            if (mesh == null)
                return Quaternion.identity;

            switch (orientation)
            {
                case HandleOrientation.ActiveElement:
                    // Intentionally not using coincident vertices here. We want the normal of just the face, not an
                    // average of it's neighbors.
                    return GetFaceRotation(mesh, faces.Last());

                case HandleOrientation.ActiveObject:
                    return mesh.transform.rotation;

                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Returns the rotation of a <see cref="Face"/> in world space.
        /// </summary>
        /// <param name="mesh">The mesh that the face belongs to.</param>
        /// <param name="face">The face you want to calculate the rotation for.</param>
        /// <returns>The rotation of the face in world space coordinates.</returns>
        public static Quaternion GetFaceRotation(ProBuilderMesh mesh, Face face)
        {
            if (mesh == null)
                return Quaternion.identity;

            if (face == null)
                return mesh.transform.rotation;

            // Intentionally not using coincident vertices here. We want the normal of just the face, not an
            // average of it's neighbors.
            Normal nrm = Math.NormalTangentBitangent(mesh, face);

            if (nrm.normal == Vector3.zero || nrm.bitangent == Vector3.zero)
                return mesh.transform.rotation;

            return mesh.transform.rotation * Quaternion.LookRotation(nrm.normal, nrm.bitangent);
        }

        /// <summary>
        /// Returns a rotation suitable for orienting a handle or gizmo relative to the Edge selection.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="orientation">The type of <see cref="HandleOrientation"/> to calculate.</param>
        /// <param name="edges">Which edges to consider in the rotation calculations. This is only used when the
        /// <see cref="HandleOrientation"/> is set to <see cref="HandleOrientation.ActiveElement"/>.</param>
        /// <returns>A rotation appropriate to the orientation and element selection.</returns>
        public static Quaternion GetEdgeRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<Edge> edges)
        {
            if (mesh == null)
                return Quaternion.identity;

            switch (orientation)
            {
                case HandleOrientation.ActiveElement:
                    // Getting an average of the edge normals isn't very helpful in real world uses, so we just use the
                    // first selected edge for orientation.
                    // This function accepts an enumerable because in the future we may want to do something more
                    // sophisticated, and it's convenient because selections are stored as collections.
                    return GetEdgeRotation(mesh, edges.Last());

                case HandleOrientation.ActiveObject:
                    return mesh.transform.rotation;

                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Returns the rotation of an <see cref="Edge"/> in world space.
        /// </summary>
        /// <param name="mesh">The mesh that edge belongs to.</param>
        /// <param name="edge">The edge you want to calculate the rotation for.</param>
        /// <returns>The rotation of the edge in world space coordinates.</returns>
        public static Quaternion GetEdgeRotation(ProBuilderMesh mesh, Edge edge)
        {
            if (mesh == null)
                return Quaternion.identity;

            return GetFaceRotation(mesh, EdgeUtility.GetFace(mesh, edge));
        }

        /// <summary>
        /// Returns a rotation suitable for orienting a handle or gizmo relative to the Vertex selection.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="orientation">The type of <see cref="HandleOrientation"/> to calculate.</param>
        /// <param name="vertices">Array of <see cref="Vertex"/> indices pointing to the vertices to consider in the rotation calculations. This is only used when the
        /// <see cref="HandleOrientation"/> is set to <see cref="HandleOrientation.ActiveElement"/>.</param>
        /// <returns>A rotation appropriate to the orientation and element selection.</returns>
        public static Quaternion GetVertexRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<int> vertices)
        {
            if (mesh == null)
                return Quaternion.identity;

            switch (orientation)
            {
                case HandleOrientation.ActiveElement:
                    if (mesh.selectedVertexCount < 1)
                        goto case HandleOrientation.ActiveObject;
                    return GetRotation(mesh, vertices);

                case HandleOrientation.ActiveObject:
                    return mesh.transform.rotation;

                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Get the rotation of a vertex in world space.
        /// </summary>
        /// <param name="mesh">The mesh that the vertex belongs to.</param>
        /// <param name="vertex">The index that points to the vertex to calculate the rotation for.</param>
        /// <returns>The rotation of a vertex normal in world space coordinates.</returns>
        public static Quaternion GetVertexRotation(ProBuilderMesh mesh, int vertex)
        {
            if (mesh == null)
                return Quaternion.identity;

            if (vertex < 0)
                return mesh.transform.rotation;

            return GetRotation(mesh, new int[] { vertex });
        }

        internal static Vector3 GetActiveElementPosition(ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            return mesh.transform.TransformPoint(Math.GetBounds(mesh.positionsInternal, faces.Last().distinctIndexesInternal).center);
        }

        internal static Vector3 GetActiveElementPosition(ProBuilderMesh mesh, IEnumerable<Edge> edges)
        {
            var edge = edges.Last();
            return mesh.transform.TransformPoint(Math.GetBounds(mesh.positionsInternal, new int[] { edge.a, edge.b }).center);
        }

        internal static Vector3 GetActiveElementPosition(ProBuilderMesh mesh, IEnumerable<int> vertices)
        {
            return mesh.transform.TransformPoint(mesh.positionsInternal[vertices.First()]);
        }
    }
}
