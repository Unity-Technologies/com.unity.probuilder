using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Provides functions for generating mesh attributes and utilities.
    /// </summary>
    public static class MeshUtility
    {
        /// <summary>
        /// Create an array of @"UnityEngine.ProBuilder.Vertex" values that are ordered as individual triangles. This modifies the source mesh to match the new individual triangles format.
        /// </summary>
        /// <param name="mesh">The mesh to extract vertices from, and apply per-triangle topology to.</param>
        /// <returns>A @"UnityEngine.ProBuilder.Vertex" array of the per-triangle vertices.</returns>
        internal static Vertex[] GeneratePerTriangleMesh(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Vertex[] vertices = mesh.GetVertices();
            int smc = mesh.subMeshCount;
            Vertex[] tv = new Vertex[mesh.triangles.Length];
            int[][] triangles = new int[smc][];
            int triIndex = 0;

            for (int s = 0; s < smc; s++)
            {
                triangles[s] = mesh.GetTriangles(s);
                int tl = triangles[s].Length;

                for (int i = 0; i < tl; i++)
                {
                    tv[triIndex++] = new Vertex(vertices[triangles[s][i]]);
                    triangles[s][i] = triIndex - 1;
                }
            }

            Vertex.SetMesh(mesh, tv);

            mesh.subMeshCount = smc;

            for (int s = 0; s < smc; s++)
                mesh.SetTriangles(triangles[s], s);

            return tv;
        }

        /// <summary>
        /// Generates tangents and applies them on the specified mesh.
        /// </summary>
        /// <param name="mesh">The <see cref="UnityEngine.Mesh"/> mesh target.</param>
        public static void GenerateTangent(Mesh mesh)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            // http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

            // speed up math by copying the mesh arrays
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = mesh.uv;
            Vector3[] normals = mesh.normals;

            //variable definitions
            int triangleCount = triangles.Length;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Vector4[] tangents = new Vector4[vertexCount];

            for (long a = 0; a < triangleCount; a += 3)
            {
                long i1 = triangles[a + 0];
                long i2 = triangles[a + 1];
                long i3 = triangles[a + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

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


            for (long a = 0; a < vertexCount; ++a)
            {
                Vector3 n = normals[a];
                Vector3 t = tan1[a];

                Vector3.OrthoNormalize(ref n, ref t);
                tangents[a].x = t.x;
                tangents[a].y = t.y;
                tangents[a].z = t.z;

                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }

        /// <summary>
        /// Returns a new mesh containing all attributes and values copied from the specified source mesh.
        /// </summary>
        /// <param name="source">The mesh to copy from.</param>
        /// <returns>A new <see cref="UnityEngine.Mesh"/> object with the same values as the source mesh.</returns>
        public static Mesh DeepCopy(Mesh source)
        {
            Mesh m = new Mesh();
            CopyTo(source, m);
            return m;
        }

        /// <summary>
        /// Copies mesh attribute values from one mesh to another.
        /// </summary>
        /// <param name="source">The mesh from which to copy attribute values.</param>
        /// <param name="destination">The destination mesh to copy attribute values to.</param>
        /// <exception cref="ArgumentNullException">Throws if source or destination is null.</exception>
        public static void CopyTo(Mesh source, Mesh destination)
        {
            if (source == null)
                throw new System.ArgumentNullException("source");

            if (destination == null)
                throw new System.ArgumentNullException("destination");

            Vector3[] v = new Vector3[source.vertices.Length];
            int[][] t = new int[source.subMeshCount][];
            Vector2[] u = new Vector2[source.uv.Length];
            Vector2[] u2 = new Vector2[source.uv2.Length];
            Vector4[] tan = new Vector4[source.tangents.Length];
            Vector3[] n = new Vector3[source.normals.Length];
            Color32[] c = new Color32[source.colors32.Length];

            System.Array.Copy(source.vertices, v, v.Length);

            for (int i = 0; i < t.Length; i++)
                t[i] = source.GetTriangles(i);

            System.Array.Copy(source.uv, u, u.Length);
            System.Array.Copy(source.uv2, u2, u2.Length);
            System.Array.Copy(source.normals, n, n.Length);
            System.Array.Copy(source.tangents, tan, tan.Length);
            System.Array.Copy(source.colors32, c, c.Length);

            destination.Clear();
            destination.name = source.name;

            destination.vertices = v;

            destination.subMeshCount = t.Length;

            for (int i = 0; i < t.Length; i++)
                destination.SetTriangles(t[i], i);

            destination.uv = u;
            destination.uv2 = u2;
            destination.tangents = tan;
            destination.normals = n;
            destination.colors32 = c;
        }

        /// <summary>
        /// Get a mesh attribute from either the MeshFilter.sharedMesh or the MeshRenderer.additionalVertexStreams mesh. The additional vertex stream mesh has priority.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to fetch.</typeparam>
        /// <param name="gameObject">The GameObject with the MeshFilter and (optional) MeshRenderer to search for mesh attributes.</param>
        /// <param name="attributeGetter">The function used to extract mesh attribute.</param>
        /// <returns>A List of the mesh attribute values from the Additional Vertex Streams mesh if it exists and contains the attribute, or the MeshFilter.sharedMesh attribute values.</returns>
        internal static T GetMeshChannel<T>(GameObject gameObject, Func<Mesh, T> attributeGetter) where T : IList
        {
            if (gameObject == null)
                throw new System.ArgumentNullException("gameObject");

            if (attributeGetter == null)
                throw new System.ArgumentNullException("attributeGetter");

            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = mf != null ? mf.sharedMesh : null;
            T res = default(T);

            if (mesh == null)
                return res;

            int vertexCount = mesh.vertexCount;

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            Mesh vertexStream = renderer != null ? renderer.additionalVertexStreams : null;

            if (vertexStream != null)
            {
                res = attributeGetter(vertexStream);

                if (res != null && res.Count == vertexCount)
                    return res;
            }
            res = attributeGetter(mesh);

            return res != null && res.Count == vertexCount ? res : default(T);
        }

        static void PrintAttribute<T>(StringBuilder sb, string title, IEnumerable<T> attrib, string fmt)
        {
            sb.AppendLine($"  - {title}");
            if (attrib != null && attrib.Any())
            {
                foreach (var value in attrib)
                    sb.AppendLine(string.Format($"    {fmt}", value));
            }
            else
            {
                sb.AppendLine("\tnull");
            }
        }

        /// <summary>
        /// Prints a detailed string summary of the mesh attributes.
        /// </summary>
        /// <param name="mesh">The mesh to print information for.</param>
        /// <returns>A tab-delimited string (positions, normals, colors, tangents, and UV coordinates).</returns>
        public static string Print(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            StringBuilder sb = new StringBuilder();

            Vector3[] positions = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Color[] colors = mesh.colors;
            Vector4[] tangents = mesh.tangents;
            List<Vector4> uv0 = new List<Vector4>();
            Vector2[] uv2 = mesh.uv2;
            List<Vector4> uv3 = new List<Vector4>();
            List<Vector4> uv4 = new List<Vector4>();

            mesh.GetUVs(0, uv0);
            mesh.GetUVs(2, uv3);
            mesh.GetUVs(3, uv4);

            sb.AppendLine($"# Sanity Check");
            sb.AppendLine(MeshUtility.SanityCheck(mesh));

            sb.AppendLine($"# Attributes ({mesh.vertexCount})");

            PrintAttribute(sb, $"positions ({positions.Length})", positions, "pos: {0:F2}");
            PrintAttribute(sb, $"normals ({normals.Length})", normals, "nrm: {0:F2}");
            PrintAttribute(sb, $"colors ({colors.Length})", colors, "col: {0:F2}");
            PrintAttribute(sb, $"tangents ({tangents.Length})", tangents, "tan: {0:F2}");
            PrintAttribute(sb, $"uv0 ({uv0.Count})", uv0, "uv0: {0:F2}");
            PrintAttribute(sb, $"uv2 ({uv2.Length})", uv2, "uv2: {0:F2}");
            PrintAttribute(sb, $"uv3 ({uv3.Count})", uv3, "uv3: {0:F2}");
            PrintAttribute(sb, $"uv4 ({uv4.Count})", uv4, "uv4: {0:F2}");

            sb.AppendLine("# Topology");

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var topo = mesh.GetTopology(i);
                var submesh = mesh.GetIndices(i);
                sb.AppendLine($"  Submesh[{i}] ({topo})");

                switch (topo)
                {
                    case MeshTopology.Points:
                        for (int n = 0; n < submesh.Length; n += 1)
                            sb.AppendLine(string.Format("\t{0}", submesh[n]));
                        break;
                    case MeshTopology.Lines:
                        for (int n = 0; n < submesh.Length; n += 2)
                            sb.AppendLine(string.Format("\t{0}, {1}", submesh[n], submesh[n + 1]));
                        break;
                    case MeshTopology.Triangles:
                        for (int n = 0; n < submesh.Length; n += 3)
                            sb.AppendLine(string.Format("\t{0}, {1}, {2}", submesh[n], submesh[n + 1], submesh[n + 2]));
                        break;
                    case MeshTopology.Quads:
                        for (int n = 0; n < submesh.Length; n += 4)
                            sb.AppendLine(string.Format("\t{0}, {1}, {2}, {3}", submesh[n], submesh[n + 1], submesh[n + 2], submesh[n + 3]));
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the number of indices this mesh contains.
        /// </summary>
        /// <param name="mesh">The source mesh to sum submesh index counts from.</param>
        /// <returns>The count of all indices contained within this mesh's submeshes.</returns>
        public static uint GetIndexCount(Mesh mesh)
        {
            uint sum = 0;

            if (mesh == null)
                return sum;

            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
                sum += mesh.GetIndexCount(i);

            return sum;
        }

        /// <summary>
        /// Returns the number of triangles or quads this mesh contains. No other mesh topologies are considered.
        /// </summary>
        /// <param name="mesh">The source mesh to sum submesh primitive counts from.</param>
        /// <returns>The count of all triangles or quads contained within this mesh's submeshes.</returns>
        public static uint GetPrimitiveCount(Mesh mesh)
        {
            uint sum = 0;

            if (mesh == null)
                return sum;

            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
            {
                if (mesh.GetTopology(i) == MeshTopology.Triangles)
                    sum += mesh.GetIndexCount(i) / 3;
                else if (mesh.GetTopology(i) == MeshTopology.Quads)
                    sum += mesh.GetIndexCount(i) / 4;
            }

            return sum;
        }

        /// <summary>
        /// Compiles a <see cref="UnityEngine.Mesh"/> from a ProBuilderMesh.
        /// </summary>
        /// <param name="probuilderMesh">The source mesh.</param>
        /// <param name="targetMesh">The destination `UnityEngine.Mesh`.</param>
        /// <param name="preferredTopology">True to try to create topology that matches the requested format. If the method can't create topology using the requested format, it uses triangles where necessary.</param>
        public static void Compile(ProBuilderMesh probuilderMesh, Mesh targetMesh, MeshTopology preferredTopology = MeshTopology.Triangles)
        {
            if (probuilderMesh == null)
                throw new ArgumentNullException("probuilderMesh");

            if (targetMesh == null)
                throw new ArgumentNullException("targetMesh");

            targetMesh.Clear();

            targetMesh.vertices = probuilderMesh.positionsInternal;
            targetMesh.uv = probuilderMesh.texturesInternal;

            if (probuilderMesh.HasArrays(MeshArrays.Texture2))
            {
                List<Vector4> uvChannel = new List<Vector4>();
                probuilderMesh.GetUVs(2, uvChannel);
                targetMesh.SetUVs(2, uvChannel);
            }

            if (probuilderMesh.HasArrays(MeshArrays.Texture3))
            {
                List<Vector4> uvChannel = new List<Vector4>();
                probuilderMesh.GetUVs(3, uvChannel);
                targetMesh.SetUVs(3, uvChannel);
            }

            targetMesh.normals = probuilderMesh.GetNormals();
            targetMesh.tangents = probuilderMesh.GetTangents();

            if (probuilderMesh.HasArrays(MeshArrays.Color))
                targetMesh.colors = probuilderMesh.colorsInternal;

            var materialCount = probuilderMesh.GetComponent<Renderer>().sharedMaterials.Length;
            var submeshes = Submesh.GetSubmeshes(probuilderMesh.facesInternal, materialCount, preferredTopology);
            targetMesh.subMeshCount = submeshes.Length;

            for (int i = 0; i < targetMesh.subMeshCount; i++)
                targetMesh.SetIndices(submeshes[i].m_Indexes, submeshes[i].m_Topology, i, false);
        }

        /// <summary>
        /// Creates a new array of vertices with values from a <see cref="UnityEngine.Mesh"/> object.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <returns>An array of vertices.</returns>
        public static Vertex[] GetVertices(this Mesh mesh)
        {
            if (mesh == null)
                return null;

            int vertexCount = mesh.vertexCount;
            Vertex[] v = new Vertex[vertexCount];

            Vector3[] positions = mesh.vertices;
            Color[] colors = mesh.colors;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Vector2[] uv0s = mesh.uv;
            Vector2[] uv2s = mesh.uv2;
            List<Vector4> uv3s = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();
            mesh.GetUVs(2, uv3s);
            mesh.GetUVs(3, uv4s);

            bool _hasPositions = positions != null && positions.Length == vertexCount;
            bool _hasColors = colors != null && colors.Length == vertexCount;
            bool _hasNormals = normals != null && normals.Length == vertexCount;
            bool _hasTangents = tangents != null && tangents.Length == vertexCount;
            bool _hasUv0 = uv0s != null && uv0s.Length == vertexCount;
            bool _hasUv2 = uv2s != null && uv2s.Length == vertexCount;
            bool _hasUv3 = uv3s.Count == vertexCount;
            bool _hasUv4 = uv4s.Count == vertexCount;

            for (int i = 0; i < vertexCount; i++)
            {
                v[i] = new Vertex();

                if (_hasPositions)
                    v[i].position = positions[i];

                if (_hasColors)
                    v[i].color = colors[i];

                if (_hasNormals)
                    v[i].normal = normals[i];

                if (_hasTangents)
                    v[i].tangent = tangents[i];

                if (_hasUv0)
                    v[i].uv0 = uv0s[i];

                if (_hasUv2)
                    v[i].uv2 = uv2s[i];

                if (_hasUv3)
                    v[i].uv3 = uv3s[i];

                if (_hasUv4)
                    v[i].uv4 = uv4s[i];
            }

            return v;
        }

        /// <summary>
        /// Merges coincident vertices where possible, optimizing the vertex count of a <see cref="UnityEngine.Mesh"/> object.
        /// </summary>
        /// <param name="mesh">The mesh to optimize.</param>
        /// <param name="vertices">
        /// Specify an array of <see cref="Vertex"/> objects to use instead of extracting attributes from the mesh.
        ///
        /// This is a performance optimization for when this array already exists. If not specified, ProBuilder generates this array automatically.
        /// </param>
        public static void CollapseSharedVertices(Mesh mesh, Vertex[] vertices = null)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            bool hasCollapsedVertices = vertices != null;

            if(vertices == null)
                vertices = mesh.GetVertices();

            int smc = mesh.subMeshCount;
            List<Dictionary<Vertex, int>> subVertices = new List<Dictionary<Vertex, int>>();
            int[][] tris = new int[smc][];
            int subIndex = 0;

            for (int i = 0; i < smc; ++i)
            {
                tris[i] = mesh.GetTriangles(i);
                Dictionary<Vertex, int> newVertices = new Dictionary<Vertex, int>();

                for (int n = 0; n < tris[i].Length; n++)
                {
                    Vertex v = vertices[tris[i][n]];
                    int index;

                    if (newVertices.TryGetValue(v, out index))
                    {
                        tris[i][n] = index;
                    }
                    else
                    {
                        tris[i][n] = subIndex;
                        newVertices.Add(v, subIndex);
                        subIndex++;
                    }
                }

                subVertices.Add(newVertices);
            }

            Vertex[] collapsed = subVertices.SelectMany(x => x.Keys).ToArray();
            //Check if new vertices have been collapsed
            hasCollapsedVertices |= (collapsed.Length != vertices.Length);
            if(hasCollapsedVertices)
            {
                Vertex.SetMesh(mesh, collapsed);
                mesh.subMeshCount = smc;
                for(int i = 0; i < smc; i++)
                    mesh.SetTriangles(tris[i], i);
            }
        }

        /// <summary>
        /// Scales mesh vertices to fit within a bounding box.
        /// </summary>
        /// <param name="mesh">The mesh to scale.</param>
        /// <param name="currentSize">The bounding box that defines the mesh's shape. </param>
        /// <param name="sizeToFit">The new size to fit mesh contents within.</param>
        public static void FitToSize(ProBuilderMesh mesh, Bounds currentSize, Vector3 sizeToFit)
        {
            if (mesh.vertexCount < 1)
                return;

            var scale = Math.Abs(sizeToFit).DivideBy(currentSize.size);
            if (scale == Vector3.one || scale == Vector3.zero)
                return;

            var positions = mesh.positionsInternal;

            if (System.Math.Abs(currentSize.size.x) < 0.001f)
                scale.x = 0;
            if (System.Math.Abs(currentSize.size.y) < 0.001f)
                scale.y = 0;
            if (System.Math.Abs(currentSize.size.z) < 0.001f)
                scale.z = 0;

            for (int i = 0, c = mesh.vertexCount; i < c; i++)
            {
                positions[i] -= currentSize.center;
                positions[i].Scale(scale);
                positions[i] += currentSize.center;
            }

            mesh.Rebuild();
        }

        internal static string SanityCheck(ProBuilderMesh mesh)
        {
            return SanityCheck(mesh.GetVertices());
        }

        /// <summary>
        /// Check mesh for invalid properties.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns>Returns true if mesh is valid, false if a problem was found.</returns>
        internal static string SanityCheck(Mesh mesh)
        {
            return SanityCheck(mesh.GetVertices());
        }

        /// <summary>
        /// Check mesh for invalid properties.
        /// </summary>
        /// <returns>Returns true if mesh is valid, false if a problem was found.</returns>
        internal static string SanityCheck(IList<Vertex> vertices)
        {
            var sb = new StringBuilder();

            for (int i = 0, c = vertices.Count; i < c; i++)
            {
                var vertex = vertices[i];

                if (Math.IsNumber(vertex.position)
                    && Math.IsNumber(vertex.color)
                    && Math.IsNumber(vertex.uv0)
                    && Math.IsNumber(vertex.normal)
                    && Math.IsNumber(vertex.tangent)
                    && Math.IsNumber(vertex.uv2)
                    && Math.IsNumber(vertex.uv3)
                    && Math.IsNumber(vertex.uv4))
                    continue;

                sb.AppendFormat("vertex {0} contains invalid values:\n{1}\n\n", i, vertex.ToString());
            }
            return sb.ToString();
        }

        internal static bool IsUsedInParticleSystem(ProBuilderMesh pbmesh)
        {
#if USING_PARTICLE_SYSTEM
            ParticleSystem pSys;
            if(pbmesh.TryGetComponent(out pSys))
            {
                var shapeModule = pSys.shape;
                if(shapeModule.meshRenderer == pbmesh.renderer)
                {
                    shapeModule.meshRenderer = null;
                    return true;
                }
            }
#endif
            return false;
        }

        internal static void RestoreParticleSystem(ProBuilderMesh pbmesh)
        {
#if USING_PARTICLE_SYSTEM
            ParticleSystem pSys;
            if(pbmesh.TryGetComponent(out pSys))
            {
                var shapeModule = pSys.shape;
                shapeModule.meshRenderer = pbmesh.renderer;
            }
#endif
        }

        internal static Bounds GetBounds(this ProBuilderMesh mesh)
        {
            if (mesh.mesh != null)
                return mesh.mesh.bounds;
            return Math.GetBounds(mesh.positionsInternal);
        }
    }
}
