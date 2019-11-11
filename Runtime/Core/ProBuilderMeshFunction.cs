using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnityEngine.ProBuilder
{
#if UNITY_EDITOR
    public sealed partial class ProBuilderMesh : ISerializationCallbackReceiver
#else
    public sealed partial class ProBuilderMesh
#endif
    {
#if UNITY_EDITOR
        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            InvalidateCaches();
        }
#endif

        static HashSet<int> s_CachedHashSet = new HashSet<int>();

        /// <summary>
        /// Reset all the attribute arrays on this object.
        /// </summary>
        public void Clear()
        {
            // various editor tools expect faces & vertices to always be valid.
            // ideally we'd null everything here, but that would break a lot of existing code.
            m_Faces = new Face[0];
            m_Positions = new Vector3[0];
            m_Textures0 = new Vector2[0];
            m_Textures2 = null;
            m_Textures3 = null;
            m_Tangents = null;
            m_SharedVertices = new SharedVertex[0];
            m_SharedTextures = new SharedVertex[0];
            InvalidateSharedVertexLookup();
            InvalidateSharedTextureLookup();
            m_Colors = null;
            ClearSelection();
        }

        void Awake()
        {
            if (vertexCount > 0
                && faceCount > 0
                && meshSyncState == MeshSyncState.Null)
                Rebuild();
        }

        void OnDestroy()
        {
            if (componentWillBeDestroyed != null)
                componentWillBeDestroyed(this);

            // Time.frameCount is zero when loading scenes in the Editor. It's the only way I could figure to
            // differentiate between OnDestroy invoked from user delete & editor scene loading.
            if (!preserveMeshAssetOnDestroy &&
                Application.isEditor &&
                !Application.isPlaying &&
                Time.frameCount > 0)
            {
                if (meshWillBeDestroyed != null)
                    meshWillBeDestroyed(this);
                else
                    DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh, true);
            }
        }

        internal static ProBuilderMesh CreateInstanceWithPoints(Vector3[] positions)
        {
            if (positions.Length % 4 != 0)
            {
                Log.Warning("Invalid Geometry. Make sure vertices in are pairs of 4 (faces).");
                return null;
            }

            GameObject go = new GameObject();
            go.name = "ProBuilder Mesh";
            ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
            pb.m_MeshFormatVersion = k_MeshFormatVersion;
            pb.GeometryWithPoints(positions);

            return pb;
        }

        /// <summary>
        /// Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer. All arrays are
        /// initialized as empty.
        /// </summary>
        /// <returns>A reference to the new ProBuilderMesh component.</returns>
        public static ProBuilderMesh Create()
        {
            var go = new GameObject();
            var pb = go.AddComponent<ProBuilderMesh>();
            pb.m_MeshFormatVersion = k_MeshFormatVersion;
            pb.Clear();
            return pb;
        }

        /// <summary>
        /// Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer, then initializes the ProBuilderMesh with a set of positions and faces.
        /// </summary>
        /// <param name="positions">Vertex positions array.</param>
        /// <param name="faces">Faces array.</param>
        /// <returns>A reference to the new ProBuilderMesh component.</returns>
        public static ProBuilderMesh Create(IEnumerable<Vector3> positions, IEnumerable<Face> faces)
        {
            GameObject go = new GameObject();
            ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
            go.name = "ProBuilder Mesh";
            pb.m_MeshFormatVersion = k_MeshFormatVersion;
            pb.RebuildWithPositionsAndFaces(positions, faces);
            return pb;
        }

        /// <summary>
        /// Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer, then initializes the ProBuilderMesh with a set of positions and faces.
        /// </summary>
        /// <param name="vertices">Vertex positions array.</param>
        /// <param name="faces">Faces array.</param>
        /// <param name="sharedVertices">Optional SharedVertex[] defines coincident vertices.</param>
        /// <param name="sharedTextures">Optional SharedVertex[] defines coincident texture coordinates (UV0).</param>
        /// <param name="materials">Optional array of materials that will be assigned to the MeshRenderer.</param>
        /// <returns></returns>
        public static ProBuilderMesh Create(
            IList<Vertex> vertices,
            IList<Face> faces,
            IList<SharedVertex> sharedVertices = null,
            IList<SharedVertex> sharedTextures = null,
            IList<Material> materials = null)
        {
            var go = new GameObject();
            go.name = "ProBuilder Mesh";
            var mesh = go.AddComponent<ProBuilderMesh>();
            if (materials != null)
                mesh.renderer.sharedMaterials = materials.ToArray();
            mesh.m_MeshFormatVersion = k_MeshFormatVersion;
            mesh.SetVertices(vertices);
            mesh.faces = faces;
            mesh.sharedVertices = sharedVertices;
            mesh.sharedTextures = sharedTextures != null ? sharedTextures.ToArray() : null;
            mesh.ToMesh();
            mesh.Refresh();
            return mesh;
        }

        void GeometryWithPoints(Vector3[] points)
        {
            // Wrap in faces
            Face[] f = new Face[points.Length / 4];

            for (int i = 0; i < points.Length; i += 4)
            {
                f[i / 4] = new Face(new int[6]
                {
                    i + 0, i + 1, i + 2,
                    i + 1, i + 3, i + 2
                },
                        0,
                        AutoUnwrapSettings.tile,
                        0,
                        -1,
                        -1,
                        false);
            }

            Clear();
            positions = points;
            m_Faces = f;
            m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(points);
            InvalidateSharedVertexLookup();
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Clear all mesh attributes and reinitialize with new positions and face collections.
        /// </summary>
        /// <param name="vertices">Vertex positions array.</param>
        /// <param name="faces">Faces array.</param>
        public void RebuildWithPositionsAndFaces(IEnumerable<Vector3> vertices, IEnumerable<Face> faces)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Clear();
            m_Positions = vertices.ToArray();
            m_Faces = faces.ToArray();
            m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(m_Positions);
            InvalidateSharedVertexLookup();
            InvalidateSharedTextureLookup();
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Wraps <see cref="ToMesh"/> and <see cref="Refresh"/>.
        /// </summary>
        internal void Rebuild()
        {
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Rebuild the mesh positions and submeshes. If vertex count matches new positions array the existing attributes are kept, otherwise the mesh is cleared. UV2 is the exception, it is always cleared.
        /// </summary>
        /// <param name="preferredTopology">Triangles and Quads are supported.</param>
        public void ToMesh(MeshTopology preferredTopology = MeshTopology.Triangles)
        {
            Mesh m = mesh;

            // if the mesh vertex count hasn't been modified, we can keep most of the mesh elements around
            if (m != null && m.vertexCount == m_Positions.Length)
                m = mesh;
            else if (m == null)
                m = new Mesh();
            else
                m.Clear();

            m.indexFormat = vertexCount > ushort.MaxValue ? Rendering.IndexFormat.UInt32 : Rendering.IndexFormat.UInt16;
            m.vertices = m_Positions;
            m.uv2 = null;

            if (m_MeshFormatVersion < k_MeshFormatVersion)
            {
                if (m_MeshFormatVersion < k_MeshFormatVersionSubmeshMaterialRefactor)
                    Submesh.MapFaceMaterialsToSubmeshIndex(this);

                m_MeshFormatVersion = k_MeshFormatVersion;
            }

            m_MeshFormatVersion = k_MeshFormatVersion;

            int materialCount = MaterialUtility.GetMaterialCount(renderer);

            Submesh[] submeshes = Submesh.GetSubmeshes(facesInternal, materialCount, preferredTopology);

            m.subMeshCount = materialCount;

            for (int i = 0; i < m.subMeshCount; i++)
            {
#if DEVELOPER_MODE
                if (i >= materialCount)
                    Log.Warning("Submesh index " + i + " is out of bounds of the MeshRenderer materials array.");
                if (submeshes[i] == null)
                    throw new Exception("Attempting to assign a null submesh. " + i + "/" + materialCount);
#endif
                m.SetIndices(submeshes[i].m_Indexes, submeshes[i].m_Topology, i, false);
            }

            m.name = string.Format("pb_Mesh{0}", id);
            filter.sharedMesh = m;
        }

        /// <summary>
        /// Ensure that the UnityEngine.Mesh associated with this object is unique
        /// </summary>
        internal void MakeUnique()
        {
            // set a new UnityEngine.Mesh instance
            mesh = new Mesh();
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Copy mesh data from another mesh to self.
        /// </summary>
        /// <param name="other"></param>
        public void CopyFrom(ProBuilderMesh other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            Clear();
            positions = other.positions;
            sharedVertices = other.sharedVerticesInternal;
            SetSharedTextures(other.sharedTextureLookup);
            facesInternal = other.faces.Select(x => new Face(x)).ToArray();

            List<Vector4> uvs = new List<Vector4>();

            for (var i = 0; i < k_UVChannelCount; i++)
            {
                other.GetUVs(1, uvs);
                SetUVs(1, uvs);
            }

            tangents = other.tangents;
            colors = other.colors;
            userCollisions = other.userCollisions;
            selectable = other.selectable;
            unwrapParameters = new UnwrapParameters(other.unwrapParameters);
        }

        /// <summary>
        /// Recalculates mesh attributes: normals, collisions, UVs, tangents, and colors.
        /// </summary>
        /// <param name="mask">
        /// Optionally pass a mask to define what components are updated (UV and collisions are expensive to rebuild, and can usually be deferred til completion of task).
        /// </param>
        public void Refresh(RefreshMask mask = RefreshMask.All)
        {
            // Mesh
            if ((mask & RefreshMask.UV) > 0)
                RefreshUV(facesInternal);

            if ((mask & RefreshMask.Colors) > 0)
                RefreshColors();

            if ((mask & RefreshMask.Normals) > 0)
                RefreshNormals();

            if ((mask & RefreshMask.Tangents) > 0)
                RefreshTangents();

            if ((mask & RefreshMask.Collisions) > 0)
                RefreshCollisions();
        }

        void RefreshCollisions()
        {
            mesh.RecalculateBounds();

            var meshCollider = GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                gameObject.GetComponent<MeshCollider>().sharedMesh = null;
                gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }

        /// <summary>
        /// Returns a new unused texture group id.
        /// Will be greater than or equal to i.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        internal int GetUnusedTextureGroup(int i = 1)
        {
            while (Array.Exists(facesInternal, element => element.textureGroup == i))
                i++;

            return i;
        }

        static bool IsValidTextureGroup(int group)
        {
            return group > 0;
        }

        /// <summary>
        /// Returns a new unused element group.
        /// Will be greater than or equal to i.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        internal int UnusedElementGroup(int i = 1)
        {
            while (Array.Exists(facesInternal, element => element.elementGroup == i))
                i++;

            return i;
        }

        public void RefreshUV(IEnumerable<Face> facesToRefresh)
        {
            // If the UV array has gone out of sync with the positions array, reset all faces to Auto UV so that we can
            // correct the texture array.
            if (!HasArrays(MeshArrays.Texture0))
            {
                m_Textures0 = new Vector2[vertexCount];
                foreach (Face f in facesInternal)
                    f.manualUV = false;
                facesToRefresh = facesInternal;
            }

            s_CachedHashSet.Clear();

            foreach (var face in facesToRefresh)
            {
                if (face.manualUV)
                    continue;

                int textureGroup = face.textureGroup;

                if (!IsValidTextureGroup(textureGroup))
                    UvUnwrapping.Unwrap(this, face);
                else if (s_CachedHashSet.Add(textureGroup))
                    UvUnwrapping.ProjectTextureGroup(this, textureGroup, face.uv);
            }

            mesh.uv = m_Textures0;

            if (HasArrays(MeshArrays.Texture2))
                mesh.SetUVs(2, m_Textures2);
            if (HasArrays(MeshArrays.Texture3))
                mesh.SetUVs(3, m_Textures3);
        }

        internal void SetGroupUV(AutoUnwrapSettings settings, int group)
        {
            if (!IsValidTextureGroup(group))
                return;

            foreach (var face in facesInternal)
            {
                if (face.textureGroup != group)
                    continue;

                face.uv = settings;
            }
        }

        void RefreshColors()
        {
            Mesh m = filter.sharedMesh;
            m.colors = m_Colors;
        }

        /// <summary>
        /// Set the vertex colors for a @"UnityEngine.ProBuilder.Face".
        /// </summary>
        /// <param name="face">The target face.</param>
        /// <param name="color">The color to set this face's referenced vertices to.</param>
        public void SetFaceColor(Face face, Color color)
        {
            if (face == null)
                throw new ArgumentNullException("face");

            if (!HasArrays(MeshArrays.Color))
                m_Colors = ArrayUtility.Fill(Color.white, vertexCount);

            foreach (int i in face.distinctIndexes)
                m_Colors[i] = color;
        }

        /// <summary>
        /// Set the material for a collection of faces.
        /// </summary>
        /// <remarks>
        /// To apply the changes to the UnityEngine.Mesh, make sure to call ToMesh and Refresh.
        /// </remarks>
        /// <param name="faces">The faces to apply the material to.</param>
        /// <param name="material">The material to apply.</param>
        public void SetMaterial(IEnumerable<Face> faces, Material material)
        {
            var materials = renderer.sharedMaterials;
            var submeshCount = materials.Length;
            var index = -1;

            for (int i = 0; i < submeshCount && index < 0; i++)
            {
                if (materials[i] == material)
                    index = i;
            }

            if (index < 0)
            {
                // Material doesn't exist in MeshRenderer.sharedMaterials, now check if there is an unused
                // submeshIndex that we can replace with this value instead of creating a new entry.
                var submeshIndexes = new bool[submeshCount];

                foreach (var face in m_Faces)
                    submeshIndexes[Math.Clamp(face.submeshIndex, 0, submeshCount - 1)] = true;

                index = Array.IndexOf(submeshIndexes, false);

                // Found an unused submeshIndex, replace it with the material.
                if (index > -1)
                {
                    materials[index] = material;
                    renderer.sharedMaterials = materials;
                }
                else
                {
                    // There were no unused submesh indices, append another submesh and material.
                    index = materials.Length;
                    var copy = new Material[index + 1];
                    Array.Copy(materials, copy, index);
                    copy[index] = material;
                    renderer.sharedMaterials = copy;
                }
            }

            foreach (var face in faces)
                face.submeshIndex = index;
        }

        void RefreshNormals()
        {
            Normals.CalculateNormals(this);
            mesh.normals = m_Normals;
        }

        void RefreshTangents()
        {
            Normals.CalculateTangents(this);
            mesh.tangents = m_Tangents;
        }

        /// <summary>
        /// Find the index of a vertex index (triangle) in an IntArray[]. The index returned is called the common index, or shared index in some cases.
        /// </summary>
        /// <remarks>Aids in removing duplicate vertex indexes.</remarks>
        /// <returns>The common (or shared) index.</returns>
        internal int GetSharedVertexHandle(int vertex)
        {
            int res;

            if (m_SharedVertexLookup.TryGetValue(vertex, out res))
                return res;

            for (int i = 0; i < m_SharedVertices.Length; i++)
            {
                for (int n = 0, c = m_SharedVertices[i].Count; n < c; n++)
                    if (m_SharedVertices[i][n] == vertex)
                        return i;
            }

            throw new ArgumentOutOfRangeException("vertex");
        }

        internal HashSet<int> GetSharedVertexHandles(IEnumerable<int> vertices)
        {
            var lookup = sharedVertexLookup;
            HashSet<int> common = new HashSet<int>();
            foreach (var i in vertices)
                common.Add(lookup[i]);
            return common;
        }

        /// <summary>
        /// Get a list of vertices that are coincident to any of the vertices in the passed vertices parameter.
        /// </summary>
        /// <param name="vertices">A collection of indexes relative to the mesh positions.</param>
        /// <returns>A list of all vertices that share a position with any of the passed vertices.</returns>
        /// <exception cref="ArgumentNullException">The vertices parameter may not be null.</exception>
        public List<int> GetCoincidentVertices(IEnumerable<int> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            List<int> shared = new List<int>();
            GetCoincidentVertices(vertices, shared);
            return shared;
        }

        /// <summary>
        /// Populate a list of vertices that are coincident to any of the vertices in the passed vertices parameter.
        /// </summary>
        /// <param name="faces">A collection of faces to gather vertices from.</param>
        /// <param name="coincident">A list to be cleared and populated with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
        public void GetCoincidentVertices(IEnumerable<Face> faces, List<int> coincident)
        {
            if (faces == null)
                throw new ArgumentNullException("faces");

            if (coincident == null)
                throw new ArgumentNullException("coincident");

            coincident.Clear();
            s_CachedHashSet.Clear();
            var lookup = sharedVertexLookup;

            foreach (var face in faces)
            {
                foreach (var v in face.distinctIndexesInternal)
                {
                    var common = lookup[v];

                    if (s_CachedHashSet.Add(common))
                    {
                        var indices = m_SharedVertices[common];

                        for (int i = 0, c = indices.Count; i < c; i++)
                            coincident.Add(indices[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Populate a list of vertices that are coincident to any of the vertices in the passed vertices parameter.
        /// </summary>
        /// <param name="edges">A collection of edges to gather vertices from.</param>
        /// <param name="coincident">A list to be cleared and populated with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
        public void GetCoincidentVertices(IEnumerable<Edge> edges, List<int> coincident)
        {
            if (faces == null)
                throw new ArgumentNullException("edges");

            if (coincident == null)
                throw new ArgumentNullException("coincident");

            coincident.Clear();
            s_CachedHashSet.Clear();
            var lookup = sharedVertexLookup;

            foreach (var edge in edges)
            {
                var common = lookup[edge.a];

                if (s_CachedHashSet.Add(common))
                {
                    var indices = m_SharedVertices[common];

                    for (int i = 0, c = indices.Count; i < c; i++)
                        coincident.Add(indices[i]);
                }

                common = lookup[edge.b];

                if (s_CachedHashSet.Add(common))
                {
                    var indices = m_SharedVertices[common];

                    for (int i = 0, c = indices.Count; i < c; i++)
                        coincident.Add(indices[i]);
                }
            }
        }

        /// <summary>
        /// Populate a list of vertices that are coincident to any of the vertices in the passed vertices parameter.
        /// </summary>
        /// <param name="vertices">A collection of indexes relative to the mesh positions.</param>
        /// <param name="coincident">A list to be cleared and populated with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
        public void GetCoincidentVertices(IEnumerable<int> vertices, List<int> coincident)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            if (coincident == null)
                throw new ArgumentNullException("coincident");

            coincident.Clear();
            s_CachedHashSet.Clear();
            var lookup = sharedVertexLookup;

            foreach (var v in vertices)
            {
                var common = lookup[v];

                if (s_CachedHashSet.Add(common))
                {
                    var indices = m_SharedVertices[common];

                    for (int i = 0, c = indices.Count; i < c; i++)
                        coincident.Add(indices[i]);
                }
            }
        }

        /// <summary>
        /// Populate a list with all the vertices that are coincident to the requested vertex.
        /// </summary>
        /// <param name="vertex">An index relative to a positions array.</param>
        /// <param name="coincident">A list to be populated with all coincident vertices.</param>
        /// <exception cref="ArgumentNullException">The coincident list may not be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The SharedVertex[] does not contain an entry for the requested vertex.</exception>
        public void GetCoincidentVertices(int vertex, List<int> coincident)
        {
            if (coincident == null)
                throw new ArgumentNullException("coincident");

            int common;

            if (!sharedVertexLookup.TryGetValue(vertex, out common))
                throw new ArgumentOutOfRangeException("vertex");

            var indices = m_SharedVertices[common];

            for (int i = 0, c = indices.Count; i < c; i++)
                coincident.Add(indices[i]);
        }

        /// <summary>
        /// Sets the passed vertices as being considered coincident by the ProBuilderMesh.
        /// </summary>
        /// <remarks>
        /// Note that it is up to the caller to ensure that the passed vertices are indeed sharing a position.
        /// </remarks>
        /// <param name="vertices">Returns a list of vertices to be associated as coincident.</param>
        public void SetVerticesCoincident(IEnumerable<int> vertices)
        {
            var lookup = sharedVertexLookup;
            List<int> coincident = new List<int>();
            GetCoincidentVertices(vertices, coincident);
            SharedVertex.SetCoincident(ref lookup, coincident);
            SetSharedVertices(lookup);
        }

        internal void SetTexturesCoincident(IEnumerable<int> vertices)
        {
            var lookup = sharedTextureLookup;
            SharedVertex.SetCoincident(ref lookup, vertices);
            SetSharedTextures(lookup);
        }

        internal void AddToSharedVertex(int sharedVertexHandle, int vertex)
        {
            if (sharedVertexHandle < 0 || sharedVertexHandle >= m_SharedVertices.Length)
                throw new ArgumentOutOfRangeException("sharedVertexHandle");

            m_SharedVertices[sharedVertexHandle].Add(vertex);
            InvalidateSharedVertexLookup();
        }

        internal void AddSharedVertex(SharedVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            m_SharedVertices = m_SharedVertices.Add(vertex);
            InvalidateSharedVertexLookup();
        }
    }
}
