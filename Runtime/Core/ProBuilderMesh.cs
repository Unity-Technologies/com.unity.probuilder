using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using System;
using System.Collections.ObjectModel;
using UnityEngine.Rendering;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// This component is responsible for storing all the data necessary for editing and compiling UnityEngine.Mesh objects.
    /// </summary>
    // The double "//" sets this component as hidden in the menu, but is used by ObjectNames.cs to get the component name.
    [AddComponentMenu("//ProBuilder MeshFilter")]
    // Don't include MeshFilter in the required components because it gets registered with serialization before we have a
    // chance to mark it with the correct HideFlags.
    [RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent, ExecuteInEditMode, ExcludeFromPreset, ExcludeFromObjectFactory]
//    [MonoBehaviourIcon("Packages/com.unity.probuilder/Content/Icons/Scripts/ProBuilderMesh@64.png")]
    public sealed partial class ProBuilderMesh : MonoBehaviour
    {
#if ENABLE_DRIVEN_PROPERTIES
        internal const HideFlags k_MeshFilterHideFlags = HideFlags.DontSave | HideFlags.HideInInspector | HideFlags.NotEditable;
#else
        internal const HideFlags k_MeshFilterHideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
#endif

        /// <summary>
        /// Max number of UV channels that ProBuilderMesh format supports.
        /// </summary>
        const int k_UVChannelCount = 4;

        /// <summary>
        /// The current mesh format version. This is used to run expensive upgrade functions once in ToMesh().
        /// </summary>
        internal const int k_MeshFormatVersion = k_MeshFormatVersionAutoUVScaleOffset;
        internal const int k_MeshFormatVersionSubmeshMaterialRefactor = 1;
        internal const int k_MeshFormatVersionAutoUVScaleOffset = 2;

        /// <summary>
        /// The maximum number of vertices that a ProBuilderMesh can accomodate.
        /// </summary>
        public const uint maxVertexCount = ushort.MaxValue;

        // MeshFormatVersion is used to deprecate and upgrade serialized data.
        [SerializeField]
        int m_MeshFormatVersion;

        // [SerializeField]
        // [FormerlySerializedAs("_quads")]
        // Face[] m_Faces;

        [SerializeField]
        PMesh m_Mesh;
        
        [SerializeField]
        [FormerlySerializedAs("_sharedIndices")]
        [FormerlySerializedAs("m_SharedVertexes")]
        SharedVertex[] m_SharedVertices;

        [Flags]
        enum CacheValidState : byte
        {
            SharedVertex = 1 << 0,
            SharedTexture = 1 << 1
        }

        [NonSerialized]
        CacheValidState m_CacheValid;

        [NonSerialized]
        Dictionary<int, int> m_SharedVertexLookup;

        [SerializeField]
        [FormerlySerializedAs("_sharedIndicesUV")]
        SharedVertex[] m_SharedTextures;

        [NonSerialized]
        Dictionary<int, int> m_SharedTextureLookup;

        // [SerializeField]
        // [FormerlySerializedAs("_vertices")]
        // Vector3[] m_Positions;

        // [SerializeField]
        // [FormerlySerializedAs("_uv")]
        // Vector2[] m_Textures0;
        //
        // [SerializeField]
        // [FormerlySerializedAs("_uv3")]
        // List<Vector4> m_Textures2;
        //
        // [SerializeField]
        // [FormerlySerializedAs("_uv4")]
        // List<Vector4> m_Textures3;

        // [SerializeField]
        // [FormerlySerializedAs("_tangents")]
        // Vector4[] m_Tangents;

        // [NonSerialized]
        // Vector3[] m_Normals;

        // [SerializeField]
        // [FormerlySerializedAs("_colors")]
        // Color[] m_Colors;

        /// <value>
        /// If false, ProBuilder will automatically create and scale colliders.
        /// </value>
        public bool userCollisions { get; set; }

        [FormerlySerializedAs("unwrapParameters")]
        [SerializeField]
        UnwrapParameters m_UnwrapParameters;

        /// <value>
        /// UV2 generation parameters.
        /// </value>
        public UnwrapParameters unwrapParameters
        {
            get { return m_UnwrapParameters; }
            set { m_UnwrapParameters = value; }
        }

        [FormerlySerializedAs("dontDestroyMeshOnDelete")]
        [SerializeField]
        bool m_PreserveMeshAssetOnDestroy;

        /// <value>
        /// If "Meshes are Assets" feature is enabled, this is used to relate pb_Objects to stored meshes.
        /// </value>
        [SerializeField]
        internal string assetGuid;

        // [SerializeField]
        // Mesh m_Mesh;

        [NonSerialized]
        MeshRenderer m_MeshRenderer;

#pragma warning disable 109
        internal new MeshRenderer renderer
        {
            get
            {
                if (!gameObject.TryGetComponent<MeshRenderer>(out m_MeshRenderer))
                    return null;
                return m_MeshRenderer;
            }
        }
#pragma warning restore 109

        [NonSerialized]
        MeshFilter m_MeshFilter;

#pragma warning disable 109
        internal new MeshFilter filter
        {
            get
            {
                if (m_MeshFilter == null)
                {
                    if (!gameObject.TryGetComponent<MeshFilter>(out m_MeshFilter))
                        return null;
#if UNITY_EDITOR
                    m_MeshFilter.hideFlags = k_MeshFilterHideFlags;
#endif
                }

                return m_MeshFilter;
            }
        }
#pragma warning restore 109

        /// <value>
        /// Simple uint tracking number of time ToMesh() and Refresh() function are called to modify the mesh
        /// Used to check if 2 versions of the ProBuilderMesh are the same or not.
        /// </value>
        [SerializeField]
        ushort m_VersionIndex = 0;
        internal ushort versionIndex => m_VersionIndex;

        internal struct NonVersionedEditScope : IDisposable
        {
            ProBuilderMesh m_Mesh;
            ushort m_VersionIndex;

            public NonVersionedEditScope(ProBuilderMesh mesh)
            {
                m_Mesh = mesh;
                m_VersionIndex = mesh.versionIndex;
            }

            public void Dispose()
            {
                m_Mesh.m_VersionIndex = m_VersionIndex;
            }
        }

        /// <value>
        /// In the editor, when you delete a ProBuilderMesh you usually also want to destroy the mesh asset.
        /// However, there are situations you'd want to keep the mesh around, like when stripping probuilder scripts.
        /// </value>
        public bool preserveMeshAssetOnDestroy
        {
            get { return m_PreserveMeshAssetOnDestroy; }
            set { m_PreserveMeshAssetOnDestroy = value; }
        }

        /// <summary>
        /// Check if the mesh contains the requested arrays.
        /// </summary>
        /// <param name="channels">A flag containing the array types that a ProBuilder mesh stores.</param>
        /// <returns>True if all arrays in the flag are present, false if not.</returns>
        public bool HasArrays(MeshArrays channels)
        {
            bool missing = false;

            int vc = vertexCount;

            missing |= (channels & MeshArrays.Position) == MeshArrays.Position && positions == null;
            missing |= (channels & MeshArrays.Normal) == MeshArrays.Normal && (normals == null || normals.Count != vc);
            missing |= (channels & MeshArrays.Texture0) == MeshArrays.Texture0 && (pmesh.textures0 == null || pmesh.textures0.Count != vc);
            missing |= (channels & MeshArrays.Texture2) == MeshArrays.Texture2 && (pmesh.textures2 == null || pmesh.textures2.Count != vc);
            missing |= (channels & MeshArrays.Texture3) == MeshArrays.Texture3 && (pmesh.textures3 == null || pmesh.textures3.Count != vc);
            missing |= (channels & MeshArrays.Color) == MeshArrays.Color && (pmesh.colors == null || pmesh.colors.Count != vc);
            missing |= (channels & MeshArrays.Tangent) == MeshArrays.Tangent && (pmesh.tangents == null || pmesh.tangents.Count != vc);

            // UV2 is a special case. It is not stored in ProBuilderMesh, does not necessarily match the vertex count,
            // and it has a cost to check.
            if ((channels & MeshArrays.Texture1) == MeshArrays.Texture1 && mesh != null)
            {
#if UNITY_2019_3_OR_NEWER
                missing |= !mesh.HasVertexAttribute(VertexAttribute.TexCoord1);
#else
                var m_Textures1 = pmesh.uv2;
                missing |= (m_Textures1 == null || m_Textures1.Length < 3);
#endif
            }

            return !missing;
        }

        internal Face[] facesInternal
        {
            get { return pmesh.faces as Face[]; }
            set { pmesh.faces = value; }
        }

        /// <summary>
        /// Meshes are composed of vertices and faces. Faces primarily contain triangles and material information. With these components, ProBuilder will compile a mesh.
        /// </summary>
        /// <value>
        /// A collection of the @"UnityEngine.ProBuilder.Face"'s that make up this mesh.
        /// </value>
        public IList<Face> faces
        {
            get => pmesh.faces;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                pmesh.faces = value;
            }
        }

        internal void InvalidateSharedVertexLookup()
        {
            if (m_SharedVertexLookup == null)
                m_SharedVertexLookup = new Dictionary<int, int>();
            m_SharedVertexLookup.Clear();
            m_CacheValid &= ~CacheValidState.SharedVertex;
        }

        internal void InvalidateSharedTextureLookup()
        {
            if (m_SharedTextureLookup == null)
                m_SharedTextureLookup = new Dictionary<int, int>();
            m_SharedTextureLookup.Clear();
            m_CacheValid &= ~CacheValidState.SharedTexture;
        }

        internal void InvalidateFaces()
        {
            if (m_Mesh == null || m_Mesh.faces == null)
                return;

            foreach (var face in faces)
                face.InvalidateCache();
        }

        internal void InvalidateCaches()
        {
            InvalidateSharedVertexLookup();
            InvalidateSharedTextureLookup();
            InvalidateFaces();
            m_SelectedCacheDirty = true;
        }

        internal SharedVertex[] sharedVerticesInternal
        {
            get => m_SharedVertices;

            set
            {
                m_SharedVertices = value;
                InvalidateSharedVertexLookup();
            }
        }

        /// <summary>
        /// ProBuilder makes the assumption that no @"UnityEngine.ProBuilder.Face" references a vertex used by another.
        /// However, we need a way to associate vertices in the editor for many operations. These vertices are usually
        /// called coincident, or shared vertices. ProBuilder manages these associations with the sharedIndexes array.
        /// Each array contains a list of triangles that point to vertices considered to be coincident. When ProBuilder
        /// compiles a UnityEngine.Mesh from the ProBuilderMesh, these vertices will be condensed to a single vertex
        /// where possible.
        /// </summary>
        /// <value>
        /// The shared (or common) index array for this mesh.
        /// </value>
        public IList<SharedVertex> sharedVertices
        {
            get { return new ReadOnlyCollection<SharedVertex>(m_SharedVertices); }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                int len = value.Count;
                m_SharedVertices = new SharedVertex[len];
                for (var i = 0; i < len; i++)
                    m_SharedVertices[i] = new SharedVertex(value[i]);

                InvalidateSharedVertexLookup();
            }
        }

        internal Dictionary<int, int> sharedVertexLookup
        {
            get
            {
                if ((m_CacheValid & CacheValidState.SharedVertex) != CacheValidState.SharedVertex)
                {
                    if (m_SharedVertexLookup == null)
                        m_SharedVertexLookup = new Dictionary<int, int>();
                    SharedVertex.GetSharedVertexLookup(m_SharedVertices, m_SharedVertexLookup);
                    m_CacheValid |= CacheValidState.SharedVertex;
                }

                return m_SharedVertexLookup;
            }
        }

        /// <summary>
        /// Set the sharedIndexes array for this mesh with a lookup dictionary.
        /// </summary>
        /// <param name="indexes">
        /// The new sharedIndexes array.
        /// </param>
        /// <seealso cref="sharedVertices"/>
        internal void SetSharedVertices(IEnumerable<KeyValuePair<int, int>> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes");
            m_SharedVertices = SharedVertex.ToSharedVertices(indexes);
            InvalidateSharedVertexLookup();
        }

        internal SharedVertex[] sharedTextures
        {
            get { return m_SharedTextures; }
            set
            {
                m_SharedTextures = value;
                InvalidateSharedTextureLookup();
            }
        }

        internal Dictionary<int, int> sharedTextureLookup
        {
            get
            {
                if ((m_CacheValid & CacheValidState.SharedTexture) != CacheValidState.SharedTexture)
                {
                    m_CacheValid |= CacheValidState.SharedTexture;
                    if (m_SharedTextureLookup == null)
                        m_SharedTextureLookup = new Dictionary<int, int>();
                    SharedVertex.GetSharedVertexLookup(m_SharedTextures, m_SharedTextureLookup);
                }

                return m_SharedTextureLookup;
            }
        }

        internal void SetSharedTextures(IEnumerable<KeyValuePair<int, int>> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes");
            m_SharedTextures = SharedVertex.ToSharedVertices(indexes);
            InvalidateSharedTextureLookup();
        }

        internal Vector3[] positionsInternal
        {
            get => m_Mesh.positions as Vector3[];
            set => m_Mesh.positions = value;
        }

        /// <value>
        /// The vertex positions that make up this mesh.
        /// </value>
        public IList<Vector3> positions
        {
            get => m_Mesh.positions;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                m_Mesh.positions = value;
            }
        }

        /// <summary>
        /// Creates a new array of vertices with values from a @"UnityEngine.ProBuilder.ProBuilderMesh" component.
        /// </summary>
        /// <param name="indexes">An optional list of indexes pointing to the mesh attribute indexes to include in the returned Vertex array.</param>
        /// <returns>An array of vertices.</returns>
        public Vertex[] GetVertices(IList<int> indexes = null)
        {
            int meshVertexCount = vertexCount;
            int vc = indexes != null ? indexes.Count : vertexCount;

            Vertex[] v = new Vertex[vc];

            Vector3[] positions = positionsInternal;
            Color[] colors = colorsInternal;
            Vector2[] uv0s = texturesInternal;
            Vector4[] tangents = GetTangents();
            Vector3[] normals = GetNormals();
            Vector2[] uv2s = mesh != null ? mesh.uv2 : null;

            List<Vector4> uv3s = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();

            GetUVs(2, uv3s);
            GetUVs(3, uv4s);

            bool _hasPositions = positions != null && positions.Count() == meshVertexCount;
            bool _hasColors = colors != null && colors.Count() == meshVertexCount;
            bool _hasNormals = normals != null && normals.Count() == meshVertexCount;
            bool _hasTangents = tangents != null && tangents.Count() == meshVertexCount;
            bool _hasUv0 = uv0s != null && uv0s.Count() == meshVertexCount;
            bool _hasUv2 = uv2s != null && uv2s.Count() == meshVertexCount;
            bool _hasUv3 = uv3s.Count() == meshVertexCount;
            bool _hasUv4 = uv4s.Count() == meshVertexCount;

            for (int i = 0; i < vc; i++)
            {
                v[i] = new Vertex();

                int ind = indexes == null ? i : indexes[i];

                if (_hasPositions)
                    v[i].position = positions[ind];

                if (_hasColors)
                    v[i].color = colors[ind];

                if (_hasNormals)
                    v[i].normal = normals[ind];

                if (_hasTangents)
                    v[i].tangent = tangents[ind];

                if (_hasUv0)
                    v[i].uv0 = uv0s[ind];

                if (_hasUv2)
                    v[i].uv2 = uv2s[ind];

                if (_hasUv3)
                    v[i].uv3 = uv3s[ind];

                if (_hasUv4)
                    v[i].uv4 = uv4s[ind];
            }

            return v;
        }

        /// <summary>
        /// Get a list of vertices from a @"UnityEngine.ProBuilder.ProBuilderMesh" component.
        /// </summary>
        /// <param name="vertices">The list that will be filled by the method.</param>
        /// <returns></returns>
        internal void GetVerticesInList(IList<Vertex> vertices)
        {
            int vc = vertexCount;

            vertices.Clear();

            Vector3[] positions = positionsInternal;
            Color[] colors = colorsInternal;
            Vector2[] uv0s = texturesInternal;
            Vector4[] tangents = GetTangents();
            Vector3[] normals = GetNormals();
            Vector2[] uv2s = mesh != null ? mesh.uv2 : null;

            List<Vector4> uv3s = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();

            GetUVs(2, uv3s);
            GetUVs(3, uv4s);

            bool _hasPositions = positions != null && positions.Count() == vc;
            bool _hasColors = colors != null && colors.Count() == vc;
            bool _hasNormals = normals != null && normals.Count() == vc;
            bool _hasTangents = tangents != null && tangents.Count() == vc;
            bool _hasUv0 = uv0s != null && uv0s.Count() == vc;
            bool _hasUv2 = uv2s != null && uv2s.Count() == vc;
            bool _hasUv3 = uv3s.Count() == vc;
            bool _hasUv4 = uv4s.Count() == vc;

            for (int i = 0; i < vc; i++)
            {
                vertices.Add(new Vertex());

                if (_hasPositions)
                    vertices[i].position = positions[i];

                if (_hasColors)
                    vertices[i].color = colors[i];

                if (_hasNormals)
                    vertices[i].normal = normals[i];

                if (_hasTangents)
                    vertices[i].tangent = tangents[i];

                if (_hasUv0)
                    vertices[i].uv0 = uv0s[i];

                if (_hasUv2)
                    vertices[i].uv2 = uv2s[i];

                if (_hasUv3)
                    vertices[i].uv3 = uv3s[i];

                if (_hasUv4)
                    vertices[i].uv4 = uv4s[i];
            }
        }

        /// <summary>
        /// Set the vertex element arrays on this mesh.
        /// </summary>
        /// <param name="vertices">The new vertex array.</param>
        /// <param name="applyMesh">An optional parameter that will apply elements to the MeshFilter.sharedMesh. Note that this should only be used when the mesh is in its original state, not optimized (meaning it won't affect triangles which can be modified by Optimize).</param>
        public void SetVertices(IList<Vertex> vertices, bool applyMesh = false)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            var first = vertices.FirstOrDefault();

            if (first == null || !first.HasArrays(MeshArrays.Position))
            {
                Clear();
                return;
            }

            Vector3[] position;
            Color[] color;
            Vector3[] normal;
            Vector4[] tangent;
            Vector2[] uv0;
            Vector2[] uv2;
            List<Vector4> uv3;
            List<Vector4> uv4;

            Vertex.GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4);

            positions = position;
            colors = color;
            // todo
            // normals = normal;
            // m_Tangents = tangent;
            m_Mesh.textures0 = uv0;
            m_Mesh.textures2 = uv3?.Select(x=>(Vector2)x).ToArray();
            m_Mesh.textures3 = uv4?.Select(x=>(Vector2)x).ToArray();

            if (applyMesh)
            {
                Mesh umesh = mesh;

                if (first.HasArrays(MeshArrays.Position))
                    umesh.vertices = position;
                if (first.HasArrays(MeshArrays.Color))
                    umesh.colors = color;
                if (first.HasArrays(MeshArrays.Texture0))
                    umesh.uv = uv0;
                if (first.HasArrays(MeshArrays.Normal))
                    umesh.normals = normal;
                if (first.HasArrays(MeshArrays.Tangent))
                    umesh.tangents = tangent;
                if (first.HasArrays(MeshArrays.Texture1))
                    umesh.uv2 = uv2;
                if (first.HasArrays(MeshArrays.Texture2))
                    umesh.SetUVs(2, uv3);
                if (first.HasArrays(MeshArrays.Texture3))
                    umesh.SetUVs(3, uv4);

                IncrementVersionIndex();
            }
        }

        /// <value>
        /// The mesh normals.
        /// </value>
        /// <see cref="Refresh"/>
        /// <see cref="Normals.CalculateNormals"/>
        public IList<Vector3> normals
        {
            get => m_Mesh.normals;
        }

        internal Vector3[] normalsInternal
        {
            get => m_Mesh.normals as Vector3[];
            set => m_Mesh.normals = value;
        }

        /// <value>
        /// Get the normals array for this mesh.
        /// </value>
        /// <returns>
        /// Returns the normals for this mesh.
        /// </returns>
        public Vector3[] GetNormals()
        {
            if (!HasArrays(MeshArrays.Normal))
                Normals.CalculateNormals(this);

            return normals.ToArray();
        }

        internal Color[] colorsInternal
        {
            get => m_Mesh.colors as Color[];
            set => m_Mesh.colors = value;
        }

        /// <value>
        /// Vertex colors array for this mesh. When setting, the value must match the length of positions.
        /// </value>
        public IList<Color> colors
        {
            get => m_Mesh.colors;
            set => m_Mesh.colors = value;
        }

        /// <summary>
        /// Get an array of Color values from the mesh.
        /// </summary>
        /// <returns>The colors array for this mesh. If mesh does not contain colors, a new array is returned filled with the default value (Color.white).</returns>
        public Color[] GetColors()
        {
            if (HasArrays(MeshArrays.Color))
                return colors.ToArray();
            return ArrayUtility.Fill(Color.white, vertexCount);
        }

        /// <value>
        /// Get the user-set tangents array for this mesh. If tangents have not been explicitly set, this value will be null.
        /// </value>
        /// <remarks>
        /// To get the generated tangents that are applied to the mesh through Refresh(), use GetTangents().
        /// </remarks>
        /// <seealso cref="GetTangents"/>
        public IList<Vector4> tangents
        {
            get => pmesh.tangents;
            set => pmesh.tangents = value;
        }

        internal Vector4[] tangentsInternal
        {
            get => pmesh.tangents as Vector4[];
            set => pmesh.tangents = value;
        }

        /// <summary>
        /// Get the tangents applied to the mesh, or create and cache them if not yet initialized.
        /// </summary>
        /// <returns>The tangents applied to the MeshFilter.sharedMesh. If the tangents array length does not match the vertex count, null is returned.</returns>
        public Vector4[] GetTangents()
        {
            if (!HasArrays(MeshArrays.Tangent))
                Normals.CalculateTangents(this);

            return tangents.ToArray();
        }

        internal Vector2[] texturesInternal
        {
            get => m_Mesh.textures0 as Vector2[];
            set => m_Mesh.textures0 = value;
        }

        internal Vector2[] textures2Internal
        {
            get => m_Mesh.textures2 as Vector2[];
            set => m_Mesh.textures2 = value;
        }

        internal Vector2[] textures3Internal
        {
            get => m_Mesh.textures3 as Vector2[];
            set => m_Mesh.textures3 = value;
        }

        /// <value>
        /// The UV0 channel. Null if not present.
        /// </value>
        /// <seealso cref="GetUVs"/>
        public IList<Vector2> textures
        {
            get => m_Mesh.textures0;
            set => m_Mesh.textures0 = value;
        }

        /// <summary>
        /// Copy values in a UV channel to uvs.
        /// </summary>
        /// <param name="channel">The index of the UV channel to fetch values from. The valid range is `{0, 1, 2, 3}`.</param>
        /// <param name="uvs">A list that will be cleared and populated with the UVs copied from this mesh.</param>
        public void GetUVs(int channel, List<Vector4> uvs)
        {
            if (uvs == null)
                throw new ArgumentNullException("uvs");

            if (channel < 0 || channel > 3)
                throw new ArgumentOutOfRangeException("channel");

            uvs.Clear();

            switch (channel)
            {
                case 0:
                    for (int i = 0; i < vertexCount; i++)
                        uvs.Add(m_Mesh.textures0[i]);
                    break;

                case 1:
                    if (mesh != null && mesh.uv2 != null)
                    {
                        Vector2[] uv2 = mesh.uv2;
                        for (int i = 0; i < uv2.Length; i++)
                            uvs.Add((Vector4)uv2[i]);
                    }
                    break;

                case 2:
                    if (m_Mesh.textures2 != null && m_Mesh.textures2.Count == vertexCount)
                    {
                        for (int i = 0; i < vertexCount; i++)
                            uvs.Add(m_Mesh.textures2[i]);
                    }
                    break;

                case 3:
                    if (m_Mesh.textures3 != null && m_Mesh.textures3.Count == vertexCount)
                    {
                        for (int i = 0; i < vertexCount; i++)
                            uvs.Add(m_Mesh.textures3[i]);
                    }
                    break;
            }
        }

        internal ReadOnlyCollection<Vector2> GetUVs(int channel)
        {
            if (channel == 0)
                return new ReadOnlyCollection<Vector2>(m_Mesh.textures0);

            if (channel == 1)
                return new ReadOnlyCollection<Vector2>(mesh.uv2);

            if (channel == 2)
                return new ReadOnlyCollection<Vector2>(m_Mesh.textures2);

            if (channel == 3)
                return new ReadOnlyCollection<Vector2>(m_Mesh.textures3);

            return null;
        }

        /// <summary>
        /// Set the mesh UVs per-channel. Channels 0 and 1 are cast to Vector2, where channels 2 and 3 are kept Vector4.
        /// </summary>
        /// <remarks>Does not apply to mesh (use Refresh to reflect changes after application).</remarks>
        /// <param name="channel">The index of the UV channel to fetch values from. The valid range is `{0, 1, 2, 3}`.</param>
        /// <param name="uvs">The new UV values.</param>
        public void SetUVs(int channel, List<Vector4> uvs)
        {
            switch (channel)
            {
                case 0:
                    pmesh.textures0 = uvs != null ? uvs.Select(x => (Vector2)x).ToArray() : null;
                    break;

                case 1:
                    // todo maybe we should do something less stupid with uv2s
                    mesh.uv2 = uvs != null ? uvs.Select(x => (Vector2)x).ToArray() : null;
                    break;

                case 2:
                    pmesh.textures2 = uvs != null ? uvs.Select(x => (Vector2)x).ToArray() : null;
                    break;

                case 3:
                    pmesh.textures3 = uvs != null ? uvs.Select(x => (Vector2)x).ToArray() : null;
                    break;
            }
        }

        /// <value>
        /// How many faces does this mesh have?
        /// </value>
        public int faceCount => pmesh.faceCount;

        /// <value>
        /// How many vertices are in the positions array.
        /// </value>
        public int vertexCount => pmesh.vertexCount;

        /// <value>
        /// How many edges compose this mesh.
        /// </value>
        public int edgeCount
        {
            get
            {
                int count = 0;
                for (int i = 0, c = faceCount; i < c; i++)
                    count += facesInternal[i].edgesInternal.Length;
                return count;
            }
        }

        /// <value>
        /// How many vertex indexes make up this mesh.
        /// </value>
        public int indexCount => faces.Sum(x => x.indexesInternal.Length);

        /// <value>
        /// How many triangles make up this mesh.
        /// </value>
        public int triangleCount => faces.Sum(x => x.indexesInternal.Length) / 3;

        /// <summary>
        /// In the editor, when a ProBuilderMesh is destroyed it will also destroy the MeshFilter.sharedMesh that is
        /// found with the parent GameObject. You may override this behaviour by subscribing to meshWillBeDestroyed.
        /// </summary>
        /// <value>
        /// If meshWillBeDestroyed has a subscriber ProBuilder will invoke it instead of cleaning up unused meshes by itself.
        /// </value>
        /// <seealso cref="preserveMeshAssetOnDestroy"/>
        public static event Action<ProBuilderMesh> meshWillBeDestroyed;

        /// <value>
        /// Invoked from ProBuilderMesh.OnDestroy before any cleanup is performed.
        /// </value>
        internal static event Action<ProBuilderMesh> componentWillBeDestroyed;

        /// <value>
        /// Invoked from ProBuilderMesh.Reset after component is rebuilt.
        /// </value>
        internal static event Action<ProBuilderMesh> componentHasBeenReset;

        /// <value>
        /// Invoked when the element selection changes on any ProBuilderMesh.
        /// </value>
        /// <seealso cref="SetSelectedFaces"/>
        /// <seealso cref="SetSelectedVertices"/>
        /// <seealso cref="SetSelectedEdges"/>
        public static event Action<ProBuilderMesh> elementSelectionChanged;

        internal Mesh mesh => m_Mesh == null ? null : m_Mesh.unityMesh;
        
        internal PMesh pmesh
        {
            get
            {
                if (m_Mesh == null)
                {
                    m_Mesh = AssetUtility.CreateSceneAsset<PMesh>();
                    m_Mesh.name = name;
                }

                return m_Mesh;
            }
            set => m_Mesh = value;
        }

        internal int id => gameObject.GetInstanceID();

        /// <summary>
        /// Ensure that the UnityEngine.Mesh is in sync with the ProBuilderMesh.
        /// </summary>
        /// <returns>A flag describing the state of the synchronicity between the MeshFilter.sharedMesh and ProBuilderMesh components.</returns>
        public MeshSyncState meshSyncState
        {
            get
            {
                if (mesh == null)
                    return MeshSyncState.Null;

                int meshNo;

                int.TryParse(mesh.name.Replace("pb_Mesh", ""), out meshNo);

                if (meshNo != id)
                    return MeshSyncState.InstanceIDMismatch;

                return mesh.uv2 == null ? MeshSyncState.Lightmap : MeshSyncState.InSync;
            }
        }

        internal int meshFormatVersion => m_MeshFormatVersion;
    }
}
