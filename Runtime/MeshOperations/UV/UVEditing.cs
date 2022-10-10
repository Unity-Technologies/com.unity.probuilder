using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// UV actions.
    /// </summary>
    static partial class UVEditing
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
                        lookup.Add(indexes[i], a = lookup.Count);

                    if (!lookup.TryGetValue(indexes[n], out b))
                        lookup.Add(indexes[n], b = lookup.Count);

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
        /// Creates separate entries in shared indexes cache for all passed indexes.
        /// </summary>
        internal static void SplitUVs(ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            var lookup = mesh.sharedTextureLookup;
            var index = lookup.Count;

            foreach(var face in faces)
            {
                foreach (var vertex in face.distinctIndexesInternal)
                {
                    int a;

                    if (lookup.TryGetValue(vertex, out a))
                        lookup[vertex] = index++;
                }
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
            if (faces.Length < 1)
                return;

            int[] ind = faces.SelectMany(x => x.distinctIndexesInternal).ToArray();

            // Get a projection direction by averaging the normals of all selected faces
            var projectionDirection = Vector3.zero;

            foreach (var face in faces)
            {
                var nrm = Math.Normal(mesh, face);
                projectionDirection += nrm;
            }

            projectionDirection /= (float) faces.Length;

            // project uv coordinates
            Vector2[] uvs = Projection.PlanarProject(mesh.positionsInternal, ind, projectionDirection);

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
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        /// <param name="channel"></param>
        public static void ProjectFacesBox(ProBuilderMesh mesh, Face[] faces, int channel = 0)
        {
            Vector2[] uv = GetUVs(mesh, channel);

            Dictionary<ProjectionAxis, List<Face>> sorted = new Dictionary<ProjectionAxis, List<Face>>();

            for (int i = 0; i < faces.Length; i++)
            {
                Vector3 nrm = Math.Normal(mesh, faces[i]);
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

                Vector2[] uvs = Projection.PlanarProject(mesh.positionsInternal, distinct, Projection.ProjectionAxisToVector(kvp.Key));

                for (int n = 0; n < distinct.Length; n++)
                    uv[distinct[n]] = uvs[n];

                SplitUVs(mesh, distinct);
            }

            /* and set the msh uv array using the new coordintaes */
            ApplyUVs(mesh, uv, channel);
        }

        /// <summary>
        /// Finds the minimal U and V coordinate of a set of an array of UVs
        /// </summary>
        internal static Vector2 FindMinimalUV(Vector2[] uvs, int[] indices = null, float xMin = 0f, float yMin = 0f)
        {
            int nbElements = (indices == null ? uvs.Length : indices.Length);
            bool first = (xMin == 0f && yMin == 0f);
            for (int i = 0; i < nbElements; ++i)
            {
                int currentIndex = (indices == null ? i : indices[i]);
                if (first)
                {
                    xMin = uvs[currentIndex].x;
                    yMin = uvs[currentIndex].y;
                    first = false;
                }
                else
                {

                    if (uvs[currentIndex].x < xMin)
                    {
                        xMin = uvs[currentIndex].x;
                    }

                    if (uvs[currentIndex].y < yMin)
                    {
                        yMin = uvs[currentIndex].y;
                    }
                }
            }

            return new Vector2(xMin, yMin);
        }

        /// <summary>
        /// Projects UVs for each face using the closest normal on a box and then place the lower left coordinate at the anchor position.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        /// <param name="lowerLeftAnchor"></param>
        /// <param name="channel"></param>
        public static void ProjectFacesBox(ProBuilderMesh mesh, Face[] faces, Vector2 lowerLeftAnchor, int channel = 0)
        {
            Vector2[] uv = GetUVs(mesh, channel);

            Dictionary<ProjectionAxis, List<Face>> sorted = new Dictionary<ProjectionAxis, List<Face>>();

            for (int i = 0; i < faces.Length; i++)
            {
                Vector3 nrm = Math.Normal(mesh, faces[i]);
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

                Vector2[] uvs = Projection.PlanarProject(mesh.positionsInternal, distinct, Projection.ProjectionAxisToVector(kvp.Key));


                Vector2 minimalUV = FindMinimalUV(uvs);

                for (int n = 0; n < distinct.Length; n++)
                    uv[distinct[n]] = uvs[n] - minimalUV;

                SplitUVs(mesh, distinct);
            }

            /* and set the msh uv array using the new coordintaes */
            ApplyUVs(mesh, uv, channel);
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
                uvs[i] -= smallestVector2;

            float scale = Math.MakeNonZero(Math.LargestValue(Math.LargestVector2(uvs)));

            for (i = 0; i < uvs.Length; i++)
                uvs[i] /= scale;

            return uvs;
        }
    }
}
