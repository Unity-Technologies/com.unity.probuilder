using System;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Provides functions to help calculate normals and tangents for a mesh.
    /// </summary>
    public static class Normals
    {
        static Vector3[] s_SmoothAvg = new Vector3[Smoothing.smoothRangeMax];
        static float[] s_SmoothAvgCount = new float[Smoothing.smoothRangeMax];
        static int[] s_CachedIntArray = new int[ushort.MaxValue];

        static void ClearIntArray(int count)
        {
            if (count > s_CachedIntArray.Length)
                Array.Resize(ref s_CachedIntArray, count);

            for (int i = 0; i < count; i++)
                s_CachedIntArray[i] = 0;
        }

        /// <summary>
        /// Calculates the tangents for a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to calculate tangents for.</param>
        public static void CalculateTangents(ProBuilderMesh mesh)
        {
            int vc = mesh.vertexCount;

            if (!mesh.HasArrays(MeshArrays.Tangent))
                mesh.tangentsInternal = new Vector4[vc];

            if (!mesh.HasArrays(MeshArrays.Position) || !mesh.HasArrays(MeshArrays.Texture0))
                return;

            // http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html
            Vector3[] normals = mesh.GetNormals();

            var positions = mesh.positionsInternal;
            var textures = mesh.texturesInternal;

            var tan1 = new Vector3[vc];
            var tan2 = new Vector3[vc];

            var tangents = mesh.tangentsInternal;

            foreach (var face in mesh.facesInternal)
            {
                int[] triangles = face.indexesInternal;

                for (int a = 0, c = triangles.Length; a < c; a += 3)
                {
                    long i1 = triangles[a + 0];
                    long i2 = triangles[a + 1];
                    long i3 = triangles[a + 2];

                    Vector3 v1 = positions[i1];
                    Vector3 v2 = positions[i2];
                    Vector3 v3 = positions[i3];

                    Vector2 w1 = textures[i1];
                    Vector2 w2 = textures[i2];
                    Vector2 w3 = textures[i3];

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

                    tan1[i1] += sdir;
                    tan1[i2] += sdir;
                    tan1[i3] += sdir;

                    tan2[i1] += tdir;
                    tan2[i2] += tdir;
                    tan2[i3] += tdir;
                }
            }

            for (long a = 0; a < vc; ++a)
            {
                Vector3 n = normals[a];
                Vector3 t = Math.EnsureUnitVector(tan1[a]);

                Vector3.OrthoNormalize(ref n, ref t);

                tangents[a].x = t.x;
                tangents[a].y = t.y;
                tangents[a].z = t.z;
                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }
        }

        /// <summary>
        /// Calculate mesh normals without taking into account smoothing groups.
        /// </summary>
        /// <returns>A new array of the vertex normals.</returns>
        /// <seealso cref="CalculateNormals"/>
        /// <summary>
        /// Calculates the normals for a mesh, taking into account smoothing groups.
        /// </summary>
        /// <param name="mesh">The mesh to calculate normals for.</param>
        static void CalculateHardNormals(ProBuilderMesh mesh)
        {
            var vertexCount = mesh.vertexCount;
            var positions = mesh.positionsInternal;
            var faces = mesh.facesInternal;
            // s_CachedIntArray acts as the average count per-normal
            ClearIntArray(vertexCount);

            if (!mesh.HasArrays(MeshArrays.Normal))
                mesh.normalsInternal = new Vector3[vertexCount];

            var normals = mesh.normalsInternal;

            for (int i = 0; i < vertexCount; i++)
            {
                normals[i].x = 0f;
                normals[i].y = 0f;
                normals[i].z = 0f;
            }

            for (int faceIndex = 0, fc = faces.Length; faceIndex < fc; faceIndex++)
            {
                int[] indexes = faces[faceIndex].indexesInternal;

                for (var tri = 0; tri < indexes.Length; tri += 3)
                {
                    int a = indexes[tri], b = indexes[tri + 1], c = indexes[tri + 2];

                    Vector3 cross = Math.Normal(positions[a], positions[b], positions[c]);
                    cross.Normalize();

                    normals[a].x += cross.x;
                    normals[b].x += cross.x;
                    normals[c].x += cross.x;

                    normals[a].y += cross.y;
                    normals[b].y += cross.y;
                    normals[c].y += cross.y;

                    normals[a].z += cross.z;
                    normals[b].z += cross.z;
                    normals[c].z += cross.z;

                    s_CachedIntArray[a]++;
                    s_CachedIntArray[b]++;
                    s_CachedIntArray[c]++;
                }
            }

            for (var i = 0; i < vertexCount; i++)
            {
                normals[i].x = normals[i].x / s_CachedIntArray[i];
                normals[i].y = normals[i].y / s_CachedIntArray[i];
                normals[i].z = normals[i].z / s_CachedIntArray[i];
            }
        }

        /// <summary>
        /// Calculates the normals for a mesh, taking into account smoothing groups.
        /// </summary>
        /// <param name="mesh">The mesh to calculate normals for.</param>
        public static void CalculateNormals(ProBuilderMesh mesh)
        {
            CalculateHardNormals(mesh);

            var sharedVertices = mesh.sharedVerticesInternal;
            var faces = mesh.facesInternal;
            // CalculateHardNormals ensures that normals array is initialized
            var normals = mesh.normalsInternal;
            int smoothGroupMax = 24;
            // s_CachedIntArray acts as the smoothingGroup lookup for each vertex
            ClearIntArray(mesh.vertexCount);

            // Create a lookup of each triangles smoothing group.
            for (int i = 0, c = mesh.faceCount; i < c; i++)
            {
                var face = faces[i];
                var indices = face.distinctIndexesInternal;

                for (int n = 0, d = indices.Length; n < d; n++)
                {
                    s_CachedIntArray[indices[n]] = face.smoothingGroup;

                    if (face.smoothingGroup >= smoothGroupMax)
                        smoothGroupMax = face.smoothingGroup + 1;
                }
            }

            // Increase buffers size if we have more smoothing groups than usual.
            if (smoothGroupMax > s_SmoothAvg.Length)
            {
                Array.Resize(ref s_SmoothAvg, smoothGroupMax);
                Array.Resize(ref s_SmoothAvgCount, smoothGroupMax);
            }

            // For each sharedIndexes group (individual vertex), find vertices that are in the same smoothing
            // group and average their normals.
            for (var i = 0; i < sharedVertices.Length; i++)
            {
                for (var n = 0; n < smoothGroupMax; n++)
                {
                    s_SmoothAvg[n].x = 0f;
                    s_SmoothAvg[n].y = 0f;
                    s_SmoothAvg[n].z = 0f;
                    s_SmoothAvgCount[n] = 0f;
                }

                for (var n = 0; n < sharedVertices[i].Count; n++)
                {
                    int index = sharedVertices[i][n];
                    int group = s_CachedIntArray[index];

                    // Ideally this should only continue on group == NONE, but historically negative values have also
                    // been treated as no smoothing.
                    if (group <= Smoothing.smoothingGroupNone)
                        continue;

                    s_SmoothAvg[group].x += normals[index].x;
                    s_SmoothAvg[group].y += normals[index].y;
                    s_SmoothAvg[group].z += normals[index].z;
                    s_SmoothAvgCount[group] += 1f;
                }

                for (int n = 0; n < sharedVertices[i].Count; n++)
                {
                    int index = sharedVertices[i][n];
                    int group = s_CachedIntArray[index];

                    if (group <= Smoothing.smoothingGroupNone)
                        continue;

                    normals[index].x = s_SmoothAvg[group].x / s_SmoothAvgCount[group];
                    normals[index].y = s_SmoothAvg[group].y / s_SmoothAvgCount[group];
                    normals[index].z = s_SmoothAvg[group].z / s_SmoothAvgCount[group];
                    normals[index] = Math.EnsureUnitVector(normals[index]);
                }
            }
        }
    }
}
