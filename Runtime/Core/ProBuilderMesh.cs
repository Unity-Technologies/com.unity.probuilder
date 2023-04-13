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
    /// Represents the ProBuilder MeshFilter component, which is responsible for storing all
    /// the data necessary for editing and compiling <see cref="UnityEngine.Mesh" /> objects.
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
        /// Stores the maximum number of UV channels that the ProBuilderMesh format supports.
        /// </summary>
        const int k_UVChannelCount = 4;

        /// <summary>
        /// Represents the current mesh format version. This is used to run expensive upgrade functions once in ToMesh().
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

        [SerializeField]
        [FormerlySerializedAs("_quads")]
        Face[] m_Faces;

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

        [SerializeField]
        [FormerlySerializedAs("_vertices")]
        Vector3[] m_Positions;

        [SerializeField]
        [FormerlySerializedAs("_uv")]
        Vector2[] m_Textures0;

        [SerializeField]
        [FormerlySerializedAs("_uv3")]
        List<Vector4> m_Textures2;

        [SerializeField]
        [FormerlySerializedAs("_uv4")]
        List<Vector4> m_Textures3;

        [SerializeField]
        [FormerlySerializedAs("_tangents")]
        Vector4[] m_Tangents;

        [NonSerialized]
        Vector3[] m_Normals;

        [SerializeField]
        [FormerlySerializedAs("_colors")]
        Color[] m_Colors;

        /// <summary>
        /// If false, ProBuilder will automatically create and scale colliders.
        /// </summary>
        public bool userCollisions { get; set; }

        [FormerlySerializedAs("unwrapParameters")]
        [SerializeField]
        UnwrapParameters m_UnwrapParameters;

        /// <summary>
        /// UV2 generation parameters.
        /// </summary>
        public UnwrapParameters unwrapParameters
        {
            get { return m_UnwrapParameters; }
            set { m_UnwrapParameters = value; }
        }

        [FormerlySerializedAs("dontDestroyMeshOnDelete")]
        [SerializeField]
        bool m_PreserveMeshAssetOnDestroy;

        /// <summary>
        /// If "Meshes are Assets" feature is enabled, this is used to relate pb_Objects to stored meshes.
        /// </summary>
        [SerializeField]
        internal string assetGuid;

        [SerializeField]
        Mesh m_Mesh;

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

        /// <summary>
        /// Tracks each time the ToMesh() and Refresh() functions are called to modify the mesh.
        /// This is a simple uint number used to check whether two versions of the ProBuilderMesh are the same or not.
        /// </summary>
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

        /// <summary>
        /// Gets or sets whether to destroy the mesh asset if this ProBuilderMesh is deleted.
        ///
        /// In the Editor, when you delete a ProBuilderMesh you usually also want to destroy the mesh asset.
        /// However, there are situations you'd want to keep the mesh around, like when stripping ProBuilder scripts.
        /// </summary>
        /// <returns>True to keep the mesh asset; false to destroy it.</returns>
        public bool preserveMeshAssetOnDestroy
        {
            get { return m_PreserveMeshAssetOnDestroy; }
            set { m_PreserveMeshAssetOnDestroy = value; }
        }

        /// <summary>
        /// Tests whether the mesh contains the requested arrays.
        /// </summary>
        /// <param name="channels">A flag containing the array types that a ProBuilder mesh stores.</param>
        /// <returns>True if all arrays in the flag are present, false if not.</returns>
        public bool HasArrays(MeshArrays channels)
        {
            bool missing = false;

            int vc = vertexCount;

            missing |= (channels & MeshArrays.Position) == MeshArrays.Position && m_Positions == null;
            missing |= (channels & MeshArrays.Normal) == MeshArrays.Normal && (m_Normals == null || m_Normals.Length != vc);
            missing |= (channels & MeshArrays.Texture0) == MeshArrays.Texture0 && (m_Textures0 == null || m_Textures0.Length != vc);
            missing |= (channels & MeshArrays.Texture2) == MeshArrays.Texture2 && (m_Textures2 == null || m_Textures2.Count != vc);
            missing |= (channels & MeshArrays.Texture3) == MeshArrays.Texture3 && (m_Textures3 == null || m_Textures3.Count != vc);
            missing |= (channels & MeshArrays.Color) == MeshArrays.Color && (m_Colors == null || m_Colors.Length != vc);
            missing |= (channels & MeshArrays.Tangent) == MeshArrays.Tangent && (m_Tangents == null || m_Tangents.Length != vc);

            // UV2 is a special case. It is not stored in ProBuilderMesh, does not necessarily match the vertex count,
            // and it has a cost to check.
            if ((channels & MeshArrays.Texture1) == MeshArrays.Texture1 && mesh != null)
            {
#if UNITY_2019_3_OR_NEWER
                missing |= !mesh.HasVertexAttribute(VertexAttribute.TexCoord1);
#else
                var m_Textures1 = m_Mesh.uv2;
                missing |= (m_Textures1 == null || m_Textures1.Length < 3);
#endif
            }

            return !missing;
        }

        internal Face[] facesInternal
        {
            get { return m_Faces; }
            set { m_Faces = value; }
        }

        /// <summary>
        /// Gets or sets the faces that ProBuilder uses to compile a mesh.
        ///
        /// Meshes are composed of vertices and faces. Faces primarily contain triangles and material information.
        /// </summary>
        /// <returns>A collection of the Face objects that make up this mesh.</returns>
        public IList<Face> faces
        {
            get { return new ReadOnlyCollection<Face>(m_Faces); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                m_Faces = value.ToArray();
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
            if (m_Faces == null)
            {
                m_Faces = new Face[0];
                return;
            }

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
            get { return m_SharedVertices; }

            set
            {
                m_SharedVertices = value;
                InvalidateSharedVertexLookup();
            }
        }

        /// <summary>
        /// Gets or sets the sharedVertices for this ProBuilderMesh object.
        ///
        /// ProBuilder makes the assumption that no <see cref="Face" /> references a vertex used by another.
        /// However, ProBuilder needs to be able to associate vertices in the Editor for many operations.
        /// These vertices are usually called [coincident](../manual/gloss.html#coincident), or "shared" vertices.
        /// ProBuilder manages these associations with the sharedIndexes array.
        ///
        /// Each array contains a list of triangles that points to vertices considered to be coincident.
        /// When ProBuilder compiles a <see cref="UnityEngine.Mesh" /> from the ProBuilderMesh, it condenses
        /// these vertices to a single vertex where possible.
        /// </summary>
        /// <returns>The shared (or common) index array for this mesh.</returns>
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
        /// Sets the sharedIndexes array for this mesh with a lookup dictionary.
        /// </summary>
        /// <param name="indexes">The new sharedIndexes array.</param>
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
            get { return m_Positions; }
            set { m_Positions = value; }
        }

        /// <summary>
        /// Gets or sets the vertex positions that compose this mesh.
        /// </summary>
        public IList<Vector3> positions
        {
            get { return new ReadOnlyCollection<Vector3>(m_Positions); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                m_Positions = value.ToArray();
            }
        }

        /// <summary>
        /// Creates a new array of vertices with values from a ProBuilderMesh component.
        /// </summary>
        /// <param name="indexes">An optional list of indices used to designate the subset of vertices values to retrieve from the mesh in the array.</param>
        /// <returns>An array of vertices matching either the specified list in the `indexes` parameter. If the `indexes` parameter contains no list, this returns all mesh vertices. </returns>
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

            bool _hasPositions = positions != null && positions.Length == meshVertexCount;
            bool _hasColors = colors != null && colors.Length == meshVertexCount;
            bool _hasNormals = normals != null && normals.Length == meshVertexCount;
            bool _hasTangents = tangents != null && tangents.Length == meshVertexCount;
            bool _hasUv0 = uv0s != null && uv0s.Length == meshVertexCount;
            bool _hasUv2 = uv2s != null && uv2s.Length == meshVertexCount;
            bool _hasUv3 = uv3s.Count == meshVertexCount;
            bool _hasUv4 = uv4s.Count == meshVertexCount;

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
        /// Returns a list of vertices from a ProBuilderMesh component.
        /// </summary>
        /// <param name="vertices">The list to populate.</param>
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

            bool _hasPositions = positions != null && positions.Length == vc;
            bool _hasColors = colors != null && colors.Length == vc;
            bool _hasNormals = normals != null && normals.Length == vc;
            bool _hasTangents = tangents != null && tangents.Length == vc;
            bool _hasUv0 = uv0s != null && uv0s.Length == vc;
            bool _hasUv2 = uv2s != null && uv2s.Length == vc;
            bool _hasUv3 = uv3s.Count == vc;
            bool _hasUv4 = uv4s.Count == vc;

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
        /// Sets the vertex element arrays on this mesh.
        /// </summary>
        /// <param name="vertices">The new vertex array.</param>
        /// <param name="applyMesh">
        /// Optionally indicate whether to apply elements to the <see cref="UnityEngine.MeshFilter.sharedMesh" />.
        /// Note that you should only use this when the mesh is in its original state, not optimized
        /// (that is, when it won't affect triangles which can be
        /// <see cref="UnityEditor.ProBuilder.EditorMeshUtility.Optimize" >optimized</see>).
        /// </param>
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

            m_Positions = position;
            m_Colors = color;
            m_Normals = normal;
            m_Tangents = tangent;
            m_Textures0 = uv0;
            m_Textures2 = uv3;
            m_Textures3 = uv4;

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

        /// <summary>
        /// Gets the normals for this mesh.
        /// </summary>
        /// <see cref="Refresh"/>
        /// <see cref="Normals.CalculateNormals"/>
        /// <returns>A collection of normals for this mesh.</returns>
        public IList<Vector3> normals
        {
            get { return m_Normals != null ? new ReadOnlyCollection<Vector3>(m_Normals) : null; }
        }

        internal Vector3[] normalsInternal
        {
            get { return m_Normals; }
            set { m_Normals = value; }
        }

        /// <summary>
        /// Gets the normals array for this mesh.
        /// </summary>
        /// <returns>An array of normals for this mesh.</returns>
        public Vector3[] GetNormals()
        {
            if (!HasArrays(MeshArrays.Normal))
                Normals.CalculateNormals(this);

            return normals.ToArray();
        }

        internal Color[] colorsInternal
        {
            get { return m_Colors; }
            set { m_Colors = value; }
        }

        /// <summary>
        /// Gets or sets a collecton of the vertex colors for this mesh.
        /// When setting, the value must match the length of the positions.
        /// </summary>
        /// <returns>A collection of colors for this mesh.</returns>
        public IList<Color> colors
        {
            get { return m_Colors != null ? new ReadOnlyCollection<Color>(m_Colors) : null; }

            set
            {
                if (value == null || value.Count == 0)
                    m_Colors = null;
                else if (value.Count != vertexCount)
                    throw new ArgumentOutOfRangeException("value", "Array length must match vertex count.");
                else
                    m_Colors = value.ToArray();
            }
        }

        /// <summary>
        /// Returns the Color values from the mesh.
        /// </summary>
        /// <returns>
        /// An array of colors for this mesh. If the mesh does not contain colors, it returns a new array
        /// filled with the default value (<see cref="UnityEngine.Color.white" />).
        /// </returns>
        public Color[] GetColors()
        {
            if (HasArrays(MeshArrays.Color))
                return colors.ToArray();
            return ArrayUtility.Fill(Color.white, vertexCount);
        }

        /// <summary>
        /// Gets or sets the array of tangents that the user explicitly set for this mesh.
        /// </summary>
        /// <returns>A collection of tangents for this mesh or null if the user hasn't set any tangents yet.</returns>
        /// <remarks>
        /// To get the generated tangents that are applied to the mesh through Refresh(), use GetTangents().
        /// </remarks>
        /// <seealso cref="GetTangents"/>
        public IList<Vector4> tangents
        {
            get
            {
                return m_Tangents == null || m_Tangents.Length != vertexCount
                    ? null
                    : new ReadOnlyCollection<Vector4>(m_Tangents);
            }

            set
            {
                if (value == null)
                    m_Tangents = null;
                else if (value.Count != vertexCount)
                    throw new ArgumentOutOfRangeException("value", "Tangent array length must match vertex count");
                else
                    m_Tangents = value.ToArray();
            }
        }

        internal Vector4[] tangentsInternal
        {
            get { return m_Tangents; }
            set { m_Tangents = value; }
        }

        /// <summary>
        /// Returns the tangents applied to the mesh. If they haven't been initialized yet, it creates and caches them.
        /// </summary>
        /// <returns>
        /// The tangents applied to <see cref="UnityEngine.MeshFilter.sharedMesh" />
        /// or null if the tangents array length doesn't match the vertex count.
        /// </returns>
        public Vector4[] GetTangents()
        {
            if (!HasArrays(MeshArrays.Tangent))
                Normals.CalculateTangents(this);

            return tangents.ToArray();
        }

        internal Vector2[] texturesInternal
        {
            get { return m_Textures0; }
            set { m_Textures0 = value; }
        }

		internal List<Vector4> textures2Internal
        {
            get { return m_Textures2; }
            set { m_Textures2 = value; }
        }

		internal List<Vector4> textures3Internal
        {
            get { return m_Textures3; }
            set { m_Textures3 = value; }
        }

        /// <summary>
        /// Gets or sets the UV0 channel.
        /// </summary>
        /// <returns>The list of texture UVs for this mesh or null if there are none.</returns>
        /// <seealso cref="GetUVs"/>
        public IList<Vector2> textures
        {
            get { return m_Textures0 != null ? new ReadOnlyCollection<Vector2>(m_Textures0) : null; }
            set
            {
                if (value == null)
                    m_Textures0 = null;
                else if (value.Count != vertexCount)
                    throw new ArgumentOutOfRangeException("value");
                else
                    m_Textures0 = value.ToArray();
            }
        }

        /// <summary>
        /// Copies values from the specified UV channel to the list of texture UVs.
        /// </summary>
        /// <param name="channel">The index of the UV channel to fetch values from. The valid range is `{0, 1, 2, 3}`.</param>
        /// <param name="uvs">The list of texture UVs to clear and populate with the copied UVs.</param>
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
                        uvs.Add((Vector4)m_Textures0[i]);
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
                    if (m_Textures2 != null)
                        uvs.AddRange(m_Textures2);
                    break;

                case 3:
                    if (m_Textures3 != null)
                        uvs.AddRange(m_Textures3);
                    break;
            }
        }

        internal ReadOnlyCollection<Vector2> GetUVs(int channel)
        {
            if (channel == 0)
                return new ReadOnlyCollection<Vector2>(m_Textures0);

            if (channel == 1)
                return new ReadOnlyCollection<Vector2>(mesh.uv2);

            if (channel == 2)
                return m_Textures2 == null ? null : new ReadOnlyCollection<Vector2>(m_Textures2.Cast<Vector2>().ToList());

            if (channel == 3)
                return m_Textures3 == null ? null : new ReadOnlyCollection<Vector2>(m_Textures3.Cast<Vector2>().ToList());

            return null;
        }

        /// <summary>
        /// Sets the mesh UVs per channel. Channels 0 and 1 are cast to Vector2, but channels 2 and 3 remain Vector4.
        /// </summary>
        /// <remarks>Does not apply to mesh (use Refresh to reflect changes after application).</remarks>
        /// <param name="channel">The index of the UV channel to copy values to. The valid range is `{0, 1, 2, 3}`.</param>
        /// <param name="uvs">The list of new UV values.</param>
        public void SetUVs(int channel, List<Vector4> uvs)
        {
            switch (channel)
            {
                case 0:
                    m_Textures0 = uvs != null ? uvs.Select(x => (Vector2)x).ToArray() : null;
                    break;

                case 1:
                    mesh.uv2 = uvs != null ? uvs.Select(x => (Vector2)x).ToArray() : null;
                    break;

                case 2:
                    m_Textures2 = uvs != null ? new List<Vector4>(uvs) : null;
                    break;

                case 3:
                    m_Textures3 = uvs != null ? new List<Vector4>(uvs) : null;
                    break;
            }
        }

        /// <summary>
        /// Gets the number of faces that this mesh has.
        /// </summary>
        /// <returns>The number of faces on this mesh.</returns>
        public int faceCount
        {
            get { return m_Faces == null ? 0 : m_Faces.Length; }
        }

        /// <summary>
        /// Gets the number of vertices in the positions array.
        /// </summary>
        /// <returns>The number of vertex positions for this mesh.</returns>
        public int vertexCount
        {
            get { return m_Positions == null ? 0 : m_Positions.Length; }
        }

        /// <summary>
        /// Gets the number of edges that compose this mesh.
        /// </summary>
        /// <returns>The number of edges in this mesh.</returns>
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

        /// <summary>
        /// Gets the number of vertex indices that compose this mesh.
        /// </summary>
        /// <returns>The number of vertices in this mesh.</returns>
        public int indexCount
        {
            get { return m_Faces == null ? 0 : m_Faces.Sum(x => x.indexesInternal.Length); }
        }

        /// <summary>
        /// Gets the number of triangles that compose this mesh.
        /// </summary>
        /// <returns>The number of triangles in this mesh.</returns>
        public int triangleCount
        {
            get { return m_Faces == null ? 0 : m_Faces.Sum(x => x.indexesInternal.Length) / 3; }
        }

        /// <summary>
        /// Invoked when this ProBuilderMesh is deleted.
        ///
        /// In the Editor, when a ProBuilderMesh is destroyed it also destroys the
        /// <see cref="UnityEngine.MeshFilter.sharedMesh" /> that it finds with the parent GameObject.
        /// To override the default behavior, subscribe to onDestroyObject. When onDestroyObject has a
        /// subscriber, ProBuilder invokes it instead of cleaning up unused meshes by itself.
        /// </summary>
        /// <seealso cref="preserveMeshAssetOnDestroy"/>
        /// <seealso cref="OnDestroy"/>
        public static event Action<ProBuilderMesh> meshWillBeDestroyed;

        /// <summary>
        /// Mesh was rebuilt from Awake call.
        /// </summary>
        internal static event Action<ProBuilderMesh> meshWasInitialized;

        /// <summary>
        /// Invoked from ProBuilderMesh.OnDestroy before any cleanup is performed.
        /// </summary>
        internal static event Action<ProBuilderMesh> componentWillBeDestroyed;

        /// <summary>
        /// Invoked from ProBuilderMesh.Reset after component is rebuilt.
        /// </summary>
        internal static event Action<ProBuilderMesh> componentHasBeenReset;

        /// <summary>
        /// Invoked when the element selection changes on any ProBuilderMesh.
        /// </summary>
        /// <seealso cref="SetSelectedFaces"/>
        /// <seealso cref="SetSelectedVertices"/>
        /// <seealso cref="SetSelectedEdges"/>
        public static event Action<ProBuilderMesh> elementSelectionChanged;

        internal Mesh mesh
        {
            get
            {
                if (m_Mesh == null && filter != null)
                    m_Mesh = filter.sharedMesh;
                return m_Mesh;
            }

            set
            { 
                m_Mesh = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            }
        }

        internal int id
        {
            get { return gameObject.GetInstanceID(); }
        }

        /// <summary>
        /// Gets a flag that indicates whether the <see cref="UnityEngine.Mesh" /> is in sync with the ProBuilderMesh.
        /// </summary>
        /// <returns>
        /// A flag that describes the state of the synchronicity between the
        /// <see cref="UnityEngine.MeshFilter.sharedMesh" /> and ProBuilderMesh components.
        /// </returns>
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
