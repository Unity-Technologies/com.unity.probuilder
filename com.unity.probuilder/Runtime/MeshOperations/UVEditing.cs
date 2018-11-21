using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// UV actions.
    /// </summary>
    static class UVEditing
    {
        /// <summary>
        /// Get a reference to the mesh UV array at index.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="channel">The zero-indexed UV channel.</param>
        /// <returns></returns>
        internal static Vector2[] GetUVs(ProBuilderMesh mesh, int channel)
        {
            switch (channel)
            {
                case 1:
                {
                    Mesh m = mesh.mesh;
                    if (m == null)
                        return null;
                    return mesh.mesh.uv2;
                }

                case 2:
                case 3:
                {
                    if (channel == 2 ? mesh.HasArrays(MeshArrays.Texture2) : mesh.HasArrays(MeshArrays.Texture3))
                    {
                        List<Vector4> uvs = new List<Vector4>();
                        mesh.GetUVs(channel, uvs);
                        return uvs.Select(x => (Vector2)x).ToArray();
                    }

                    return null;
                }

                default:
                    return mesh.texturesInternal;
            }
        }

        /// <summary>
        /// Sets an array to the appropriate UV channel, but don't refresh the Mesh.
        /// </summary>
        internal static void ApplyUVs(ProBuilderMesh mesh, Vector2[] uvs, int channel, bool applyToMesh = true)
        {
            switch (channel)
            {
                case 0:
                    mesh.texturesInternal = uvs;
                    if (applyToMesh && mesh.mesh != null)
                        mesh.mesh.uv = uvs;
                    break;

                case 1:
                    if (applyToMesh && mesh.mesh != null)
                        mesh.mesh.uv2 = uvs;
                    break;

                case 2:
                case 3:
                    int vc = mesh.vertexCount;
                    if (vc != uvs.Length)
                        throw new IndexOutOfRangeException("uvs");
                    List<Vector4> list = new List<Vector4>(vc);
                    for (int i = 0; i < vc; i++)
                        list.Add(uvs[i]);
                    mesh.SetUVs(channel, list);
                    if (applyToMesh && mesh.mesh != null)
                        mesh.mesh.SetUVs(channel, list);
                    break;
            }
        }

        /// <summary>
        /// Sews (welds) a UV seam using delta to determine which UVs are close enough to be merged.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static void SewUVs(this ProBuilderMesh mesh, int[] indexes, float delta)
        {
            Vector2[] uvs = mesh.texturesInternal;

            if (uvs == null || uvs.Length != mesh.vertexCount)
                uvs = new Vector2[mesh.vertexCount];

            var lookup = mesh.sharedTextureLookup;

            for (int i = 0; i < indexes.Length - 1; i++)
            {
                for (int n = i + 1; n < indexes.Length; n++)
                {
                    int a, b;

                    if (!lookup.TryGetValue(indexes[i], out a))
                        lookup.Add(indexes[i], a = lookup.Count());

                    if (!lookup.TryGetValue(indexes[n], out b))
                        lookup.Add(indexes[n], b = lookup.Count());

                    if (a == b)
                        continue;

                    if (Vector2.Distance(uvs[indexes[i]], uvs[indexes[n]]) < delta)
                    {
                        Vector3 cen = (uvs[indexes[i]] + uvs[indexes[n]]) / 2f;

                        uvs[indexes[i]] = cen;
                        uvs[indexes[n]] = cen;

                        // ToArray prevents delayed execution of linq actions, which cause trouble when modifying the
                        // dictionary values
                        var merge = lookup.Where(x => x.Value == b).Select(y => y.Key).ToArray();

                        foreach (var key in merge)
                            lookup[key] = a;
                    }
                }
            }

            mesh.SetSharedTextures(lookup);
        }

        /// <summary>
        /// Similar to Sew, except Collapse just flattens all UVs to the center point no matter the distance.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        public static void CollapseUVs(this ProBuilderMesh mesh, int[] indexes)
        {
            Vector2[] uvs = mesh.texturesInternal;

            // set the shared indexes cache to a unique non-used index
            Vector2 cen = Math.Average(ArrayUtility.ValuesWithIndexes(uvs, indexes));

            foreach (int i in indexes)
                uvs[i] = cen;

            mesh.SetTexturesCoincident(indexes);
        }

        /// <summary>
        /// Creates separate entries in shared indexes cache for all passed indexes. If indexes are not present in pb_IntArray[], don't do anything with them.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexes"></param>
        public static void SplitUVs(this ProBuilderMesh mesh, IEnumerable<int> indexes)
        {
            var lookup = mesh.sharedTextureLookup;
            var index = lookup.Count;

            foreach (var vertex in indexes)
            {
                int a;

                if (lookup.TryGetValue(vertex, out a))
                    lookup[vertex] = index++;
            }

            mesh.SetSharedTextures(lookup);
        }

        /// <summary>
        /// Projects UVs on all passed faces, automatically updating the sharedIndexesUV table as required (only associates
        /// vertices that share a seam).
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        /// <param name="channel"></param>
        internal static void ProjectFacesAuto(ProBuilderMesh mesh, Face[] faces, int channel)
        {
            int[] ind = faces.SelectMany(x => x.distinctIndexesInternal).ToArray();

            // project uv coordinates
            Vector2[] uvs = Projection.PlanarProject(mesh.positionsInternal, ind);

            // re-assign new projected coords back into full uv array
            Vector2[] rebuiltUVs = GetUVs(mesh, channel);

            for (int i = 0; i < ind.Length; i++)
                rebuiltUVs[ind[i]] = uvs[i];

            // and set the msh uv array using the new coordintaes
            ApplyUVs(mesh, rebuiltUVs, channel);

            // now go trhough and set all adjacent face groups to use matching element groups
            foreach (Face f in faces)
            {
                f.elementGroup = -1;
                SplitUVs(mesh, f.distinctIndexesInternal);
            }

            mesh.SewUVs(faces.SelectMany(x => x.distinctIndexesInternal).ToArray(), .001f);
        }

        /// <summary>
        /// Projects UVs for each face using the closest normal on a box.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="faces"></param>
        /// <param name="channel"></param>
        public static void ProjectFacesBox(ProBuilderMesh pb, Face[] faces, int channel = 0)
        {
            Vector2[] uv = GetUVs(pb, channel);

            Dictionary<ProjectionAxis, List<Face>> sorted = new Dictionary<ProjectionAxis, List<Face>>();

            for (int i = 0; i < faces.Length; i++)
            {
                Vector3 nrm = Math.Normal(pb, faces[i]);
                ProjectionAxis axis = Projection.VectorToProjectionAxis(nrm);

                if (sorted.ContainsKey(axis))
                    sorted[axis].Add(faces[i]);
                else
                    sorted.Add(axis, new List<Face>() { faces[i] });

                // clean up UV stuff - no shared UV indexes and remove element group
                faces[i].elementGroup = -1;
                faces[i].manualUV = true;
            }

            foreach (KeyValuePair<ProjectionAxis, List<Face>> kvp in sorted)
            {
                int[] distinct = kvp.Value.SelectMany(x => x.distinctIndexesInternal).ToArray();

                Vector2[] uvs = Projection.PlanarProject(pb.positionsInternal, distinct);

                for (int n = 0; n < distinct.Length; n++)
                    uv[distinct[n]] = uvs[n];

                SplitUVs(pb, distinct);
            }

            /* and set the msh uv array using the new coordintaes */
            ApplyUVs(pb, uv, channel);
        }

        /// <summary>
        /// Projects UVs for each face using the closest normal on a sphere.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="indexes"></param>
        /// <param name="channel"></param>
        public static void ProjectFacesSphere(ProBuilderMesh pb, int[] indexes, int channel = 0)
        {
            foreach (Face f in pb.facesInternal)
            {
                if (ArrayUtility.ContainsMatch<int>(f.distinctIndexesInternal, indexes))
                {
                    f.elementGroup = -1;
                    f.manualUV = true;
                }
            }

            SplitUVs(pb, indexes);

            Vector2[] projected = Projection.SphericalProject(pb.positionsInternal, indexes);
            Vector2[] uv = GetUVs(pb, channel);

            for (int i = 0; i < indexes.Length; i++)
                uv[indexes[i]] = projected[i];

            /* and set the msh uv array using the new coordintaes */
            ApplyUVs(pb, uv, channel);
        }

        /*
         *  Returns normalized UV values for a mesh uvs (0,0) - (1,1)
         */
        public static Vector2[] FitUVs(Vector2[] uvs)
        {
            // shift UVs to zeroed coordinates
            Vector2 smallestVector2 = Math.SmallestVector2(uvs);

            int i;
            for (i = 0; i < uvs.Length; i++)
            {
                uvs[i] -= smallestVector2;
            }

            float scale = Math.LargestValue(Math.LargestVector2(uvs));

            for (i = 0; i < uvs.Length; i++)
            {
                uvs[i] /= scale;
            }

            return uvs;
        }

        /// <summary>
        /// Provided two faces, this method will attempt to project @f2 and align its size, rotation, and position to match
        /// the shared edge on f1.  Returns true on success, false otherwise.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool AutoStitch(ProBuilderMesh mesh, Face f1, Face f2, int channel)
        {
            for (int i = 0; i < f1.edgesInternal.Length; i++)
            {
                // find a matching edge
                int ind = mesh.IndexOf(f2.edgesInternal, f1.edgesInternal[i]);
                if (ind > -1)
                {
                    // First, project the second face
                    ProjectFacesAuto(mesh, new Face[] { f2 }, channel);

                    // Use the first first projected as the starting point
                    // and match the vertices
                    f1.manualUV = true;
                    f2.manualUV = true;

                    f1.textureGroup = -1;
                    f2.textureGroup = -1;

                    AlignEdges(mesh, f2, f1.edgesInternal[i], f2.edgesInternal[ind], channel);
                    return true;
                }
            }

            // no matching edge found
            return false;
        }

        /// <summary>
        /// move the UVs to where the edges passed meet
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faceToMove"></param>
        /// <param name="edgeToAlignTo"></param>
        /// <param name="edgeToBeAligned"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        static bool AlignEdges(ProBuilderMesh mesh, Face faceToMove, Edge edgeToAlignTo, Edge edgeToBeAligned, int channel)
        {
            Vector2[] uvs = GetUVs(mesh, channel);
            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
            SharedVertex[] sharedIndexesUV = mesh.sharedTextures;

            // Match each edge vertex to the other
            int[] matchX = new int[2] { edgeToAlignTo.a, -1 };
            int[] matchY = new int[2] { edgeToAlignTo.b, -1 };

            int siIndex = mesh.GetSharedVertexHandle(edgeToAlignTo.a);

            if (siIndex < 0)
                return false;

            if (sharedIndexes[siIndex].Contains(edgeToBeAligned.a))
            {
                matchX[1] = edgeToBeAligned.a;
                matchY[1] = edgeToBeAligned.b;
            }
            else
            {
                matchX[1] = edgeToBeAligned.b;
                matchY[1] = edgeToBeAligned.a;
            }

            // scale face 2 to match the edge size of f1
            float dist_e1 = Vector2.Distance(uvs[edgeToAlignTo.a], uvs[edgeToAlignTo.b]);
            float dist_e2 = Vector2.Distance(uvs[edgeToBeAligned.a], uvs[edgeToBeAligned.b]);

            float scale = dist_e1 / dist_e2;

            // doesn't matter what point we scale around because we'll move it in the next step anyways
            foreach (int i in faceToMove.distinctIndexesInternal)
                uvs[i] = uvs[i].ScaleAroundPoint(Vector2.zero, Vector2.one * scale);

            /**
             * Figure out where the center of each edge is so that we can move the f2 edge to match f1's origin
             */
            Vector2 f1_center = (uvs[edgeToAlignTo.a] + uvs[edgeToAlignTo.b]) / 2f;
            Vector2 f2_center = (uvs[edgeToBeAligned.a] + uvs[edgeToBeAligned.b]) / 2f;

            Vector2 diff = f1_center - f2_center;

            /**
             * Move f2 face to where it's matching edge center is on top of f1's center
             */
            foreach (int i in faceToMove.distinctIndexesInternal)
                uvs[i] += diff;

            /**
             * Now that the edge's centers are matching, rotate f2 to match f1's angle
             */
            Vector2 angle1 = uvs[matchY[0]] - uvs[matchX[0]];
            Vector2 angle2 = uvs[matchY[1]] - uvs[matchX[1]];

            float angle = Vector2.Angle(angle1, angle2);
            if (Vector3.Cross(angle1, angle2).z < 0)
                angle = 360f - angle;

            foreach (int i in faceToMove.distinctIndexesInternal)
                uvs[i] = Math.RotateAroundPoint(uvs[i], f1_center, angle);

            float error = Mathf.Abs(Vector2.Distance(uvs[matchX[0]], uvs[matchX[1]])) + Mathf.Abs(Vector2.Distance(uvs[matchY[0]], uvs[matchY[1]]));

            // now check that the matched UVs are on top of one another if the error allowance is greater than some small value
            if (error > .02f)
            {
                // first try rotating 180 degrees
                foreach (int i in faceToMove.distinctIndexesInternal)
                    uvs[i] = Math.RotateAroundPoint(uvs[i], f1_center, 180f);

                float e2 = Mathf.Abs(Vector2.Distance(uvs[matchX[0]], uvs[matchX[1]])) + Mathf.Abs(Vector2.Distance(uvs[matchY[0]], uvs[matchY[1]]));
                if (e2 < error)
                    error = e2;
                else
                {
                    // flip 'em back around
                    foreach (int i in faceToMove.distinctIndexesInternal)
                        uvs[i] = Math.RotateAroundPoint(uvs[i], f1_center, 180f);
                }
            }

            // If successfully aligned, merge the sharedIndexesUV
            SplitUVs(mesh, faceToMove.distinctIndexesInternal);

            mesh.SetTexturesCoincident(matchX);
            mesh.SetTexturesCoincident(matchY);
            ApplyUVs(mesh, uvs, channel);

            return true;
        }

        /**
         * Attempts to translate, rotate, and scale @points to match @target as closely as possible.
         * Only points[0, target.Length] coordinates are used in the matching process - points[target.Length, points.Length]
         * are just along for the ride.
         */
        public static Transform2D MatchCoordinates(Vector2[] points, Vector2[] target)
        {
            int length = points.Length < target.Length ? points.Length : target.Length;

            Bounds2D t_bounds = new Bounds2D(target, length); // only match the bounds of known matching points

            // move points to the center of target
            Vector2 translation = t_bounds.center - Bounds2D.Center(points, length);

            Vector2[] transformed = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
                transformed[i] = points[i] + translation;

            // rotate to match target points
            Vector2 target_angle = target[1] - target[0], transform_angle = transformed[1] - transformed[0];

            float angle = Vector2.Angle(target_angle, transform_angle);
            float dot = Vector2.Dot(Vector2.Perpendicular(target_angle), transform_angle);

            if (dot < 0) angle = 360f - angle;

            for (int i = 0; i < points.Length; i++)
                transformed[i] = transformed[i].RotateAroundPoint(t_bounds.center, angle);

            // and lastly scale
            Bounds2D p_bounds = new Bounds2D(transformed, length);
            Vector2 scale = t_bounds.size.DivideBy(p_bounds.size);

            // for(int i = 0; i < points.Length; i++)
            //  transformed[i] = transformed[i].ScaleAroundPoint(t_bounds.center, scale);

            return new Transform2D(translation, angle, scale);
        }

        /**
         * Sets the passed faces to use Auto or Manual UVs, and (if previously manual) splits any vertex connections.
         */
        public static void SetAutoUV(ProBuilderMesh pb, Face[] faces, bool auto)
        {
            if (auto)
            {
                faces = System.Array.FindAll(faces, x => x.manualUV).ToArray(); // only operate on faces that were previously manual

                pb.SplitUVs(faces.SelectMany(x => x.indexes));

                Vector2[][] uv_origins = new Vector2[faces.Length][];
                for (int i = 0; i < faces.Length; i++)
                    uv_origins[i] = pb.texturesInternal.ValuesWithIndexes(faces[i].distinctIndexesInternal);

                for (int f = 0; f < faces.Length; f++)
                {
                    faces[f].uv.Reset();
                    faces[f].manualUV = false;
                    faces[f].elementGroup = -1;
                }

                pb.Refresh(RefreshMask.UV);

                for (int i = 0; i < faces.Length; i++)
                {
                    Transform2D transform = MatchCoordinates(pb.texturesInternal.ValuesWithIndexes(faces[i].distinctIndexesInternal), uv_origins[i]);

                    var uv = faces[i].uv;
                    uv.offset = -transform.position;
                    uv.rotation = transform.rotation;

                    if (Mathf.Abs(transform.scale.sqrMagnitude - 2f) > .1f)
                        uv.scale = transform.scale;

                    faces[i].uv = uv;
                }
            }
            else
            {
                foreach (Face f in faces)
                {
                    f.textureGroup = -1;
                    f.manualUV = true;
                }
            }
        }
    }
}
