using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using System;
using System.Collections.ObjectModel;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// This component is responsible for storing all the data necessary for editing and compiling UnityEngine.Mesh objects.
	/// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public sealed partial class ProBuilderMesh : MonoBehaviour, ISerializationCallbackReceiver
    {
	    const int k_UVChannelCount = 4;

	    /// <summary>
	    /// The maximum number of vertexes that a ProBuilderMesh can accomodate.
	    /// </summary>
	    public const uint maxVertexCount = ushort.MaxValue;

        [SerializeField]
        [FormerlySerializedAs("_quads")]
        Face[] m_Faces;

        [SerializeField]
        [FormerlySerializedAs("_sharedIndices")]
        SharedVertex[] m_SharedVertexes;

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

		[SerializeField]
        [FormerlySerializedAs("_colors")]
        Color[] m_Colors;

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

		    var m_Textures1 = mesh != null ? mesh.uv2 : null;

		    missing |= (channels & MeshArrays.Position) == MeshArrays.Position && m_Positions == null;
		    missing |= (channels & MeshArrays.Texture0) == MeshArrays.Texture0 && (m_Textures0 == null || m_Textures0.Length != vc);
		    missing |= (channels & MeshArrays.Texture1) == MeshArrays.Texture1 && (m_Textures1 == null || m_Textures1.Length != vc);
		    missing |= (channels & MeshArrays.Texture2) == MeshArrays.Texture2 && (m_Textures2 == null || m_Textures2.Count != vc);
		    missing |= (channels & MeshArrays.Texture3) == MeshArrays.Texture3 && (m_Textures3 == null || m_Textures3.Count != vc);
		    missing |= (channels & MeshArrays.Color) == MeshArrays.Color && (m_Colors == null || m_Colors.Length != vc);
		    missing |= (channels & MeshArrays.Tangent) == MeshArrays.Tangent && (m_Tangents == null || m_Tangents.Length != vc);

		    return !missing;
	    }

	    internal Face[] facesInternal
        {
            get { return m_Faces; }
            set { m_Faces = value; }
        }

	    /// <summary>
	    /// Meshes are composed of vertexes and faces. Faces primarily contain triangles and material information. With these components, ProBuilder will compile a mesh.
	    /// </summary>
	    /// <value>
	    /// A collection of the @"UnityEngine.ProBuilder.Face"'s that make up this mesh.
	    /// </value>
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
		    if(m_SharedVertexLookup == null)
			    m_SharedVertexLookup = new Dictionary<int, int>();
		    m_SharedVertexLookup.Clear();
	    }

	    internal void InvalidateSharedTextureLookup()
	    {
		    if(m_SharedTextureLookup == null)
			    m_SharedTextureLookup = new Dictionary<int, int>();
		    m_SharedTextureLookup.Clear();
	    }

	    internal SharedVertex[] sharedVertexesInternal
	    {
		    get { return m_SharedVertexes; }

		    set
		    {
			    m_SharedVertexes = value;
			    InvalidateSharedVertexLookup();
		    }
	    }

	    /// <summary>
	    /// ProBuilder makes the assumption that no @"UnityEngine.ProBuilder.Face" references a vertex used by another. However, we need a way to associate vertexes in the editor for many operations. These vertexes are usually called coincident, or shared vertexes. ProBuilder manages these associations with the sharedIndexes array. Each array contains a list of triangles that point to vertices considered to be coincident. When ProBuilder compiles a UnityEngine.Mesh from the ProBuilderMesh, these vertices will be condensed to a single vertex where possible.
	    /// </summary>
	    /// <value>
	    /// The shared (or common) index array for this mesh.
	    /// </value>
	    public IList<SharedVertex> sharedVertexes
	    {
		    get { return new ReadOnlyCollection<SharedVertex>(m_SharedVertexes); }

		    set
		    {
			    if (value == null)
				    throw new ArgumentNullException("value");

			    int len = value.Count;
			    m_SharedVertexes = new SharedVertex[len];
			    for (var i = 0; i < len; i++)
				    m_SharedVertexes[i] = new SharedVertex(value[i]);

			    InvalidateSharedVertexLookup();
		    }
	    }

	    internal Dictionary<int, int> sharedVertexLookup
	    {
		    get
		    {
			    if (m_SharedVertexLookup == null)
				    m_SharedVertexLookup = new Dictionary<int, int>();
			    if (!m_SharedVertexLookup.Any())
				    SharedVertex.GetSharedVertexLookup(m_SharedVertexes, m_SharedVertexLookup);
			    return m_SharedVertexLookup;
		    }
	    }

	    /// <summary>
	    /// Set the sharedIndexes array for this mesh with a lookup dictionary.
	    /// </summary>
	    /// <param name="indexes">
	    /// The new sharedIndexes array.
	    /// </param>
	    /// <seealso cref="sharedVertexes"/>
	    internal void SetSharedVertexes(IEnumerable<KeyValuePair<int, int>> indexes)
	    {
		    if (indexes == null)
			    throw new ArgumentNullException("indexes");
		    m_SharedVertexes = SharedVertex.ToSharedVertexes(indexes);
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
			    if (m_SharedTextureLookup == null)
				    m_SharedTextureLookup = new Dictionary<int, int>();

			    if (!m_SharedTextureLookup.Any())
			    {
				    if (m_SharedTextures == null)
					    m_SharedTextureLookup.Clear();
				    else
					    SharedVertex.GetSharedVertexLookup(m_SharedTextures, m_SharedTextureLookup);
			    }

			    return m_SharedTextureLookup;
		    }
	    }

	    internal void SetSharedTextures(IEnumerable<KeyValuePair<int, int>> indexes)
	    {
		    if (indexes == null)
			    throw new ArgumentNullException("indexes");
		    m_SharedTextures = SharedVertex.ToSharedVertexes(indexes);
		    InvalidateSharedTextureLookup();
	    }

        internal Vector3[] positionsInternal
        {
            get { return m_Positions; }
            set { m_Positions = value; }
        }

	    /// <value>
	    /// The vertex positions that make up this mesh.
	    /// </value>
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
		/// Creates a new array of vertexes with values from a @"UnityEngine.ProBuilder.ProBuilderMesh" component.
		/// </summary>
		/// <param name="indexes">An optional list of indexes pointing to the mesh attribute indexes to include in the returned Vertex array.</param>
		/// <returns>An array of vertexes.</returns>
		public Vertex[] GetVertexes(IList<int> indexes = null)
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
        /// Set the vertex element arrays on this mesh.
        /// </summary>
        /// <param name="vertexes">The new vertex array.</param>
        /// <param name="applyMesh">An optional parameter that will apply elements to the MeshFilter.sharedMesh. Note that this should only be used when the mesh is in its original state, not optimized (meaning it won't affect triangles which can be modified by Optimize).</param>
        public void SetVertexes(IList<Vertex> vertexes, bool applyMesh = false)
        {
            if (vertexes == null)
                throw new ArgumentNullException("vertexes");

	        var first = vertexes.FirstOrDefault();

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

	        Vertex.GetArrays(vertexes, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4);

            m_Positions = position;
            m_Colors = color;
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
            }
        }

	    /// <summary>
	    /// ProBuilderMesh doesn't store normals, so this function will either:
	    ///		1. Copy them from the MeshFilter.sharedMesh (if vertex count matches the @"UnityEngine.ProBuilder.ProBuilderMesh.vertexCount")
	    ///		2. Calculate a new set of normals using @"UnityEngine.ProBuilder.MeshUtility.CalculateNormals".
	    /// </summary>
	    /// <returns>An array of vertex normals.</returns>
	    /// <seealso cref="UnityEngine.ProBuilder.ProBuilderMesh.CalculateNormals"/>
	    public Vector3[] GetNormals()
	    {
		    // If mesh isn't optimized try to return a copy from the compiled mesh
		    if (mesh != null && mesh.vertexCount == vertexCount)
		    {
			    var nrm = mesh.normals;
			    if (nrm != null && nrm.Length == vertexCount)
				    return nrm;
		    }

		    return CalculateNormals();
	    }

	    internal Color[] colorsInternal
	    {
		    get { return m_Colors; }
		    set { m_Colors = value; }
	    }

		/// <value>
		/// Vertex colors array for this mesh. When setting, the value must match the length of positions.
		/// </value>
	    public IList<Color> colors
        {
            get { return m_Colors != null ? new ReadOnlyCollection<Color>(m_Colors) : null; }

			set
			{
				if (value == null)
					m_Colors = null;
				else if (value.Count() != vertexCount)
					throw new ArgumentOutOfRangeException("value", "Array length must match vertex count.");
				else
					m_Colors = value.ToArray();
			}
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
		/// Get the user-set tangents array for this mesh. If tangents have not been explictly set, this value will be null.
		/// </value>
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
				else if (value.Count() != vertexCount)
					throw new ArgumentOutOfRangeException("value", "Tangent array length must match vertex count");
				else
					m_Tangents = value.ToArray();
			}
	    }

	    /// <summary>
	    /// Get the tangents applied to the mesh. Does not calculate new tangents if none are available (unlike GetNormals()).
	    /// </summary>
	    /// <returns>The tangents applied to the MeshFilter.sharedMesh. If the tangents array length does not match the vertex count, null is returned.</returns>
	    public Vector4[] GetTangents()
	    {
		    if (m_Tangents != null && m_Tangents.Length == vertexCount)
			    return m_Tangents.ToArray();
		    return mesh == null ? null : mesh.tangents;
	    }

        internal Vector2[] texturesInternal
		{
			get { return m_Textures0; }
			set { m_Textures0 = value; }
		}

	    /// <value>
	    /// The UV0 channel. Null if not present.
	    /// </value>
	    /// <seealso cref="GetUVs"/>
	    public IList<Vector2> textures
	    {
		    get { return m_Textures0 != null ? new ReadOnlyCollection<Vector2>(m_Textures0) : null; }
		    set
		    {
			    if (value == null)
				    m_Textures0 = null;
			    else if(value.Count() != vertexCount)
				    throw new ArgumentOutOfRangeException("value");
			    else
				    m_Textures0 = value.ToArray();
		    }
	    }

        /// <summary>
        ///	Copy values in a UV channel to uvs.
        /// </summary>
        /// <param name="channel">The index of the UV channel to fetch values from. The valid range is `{0, 1, 2, 3}`.</param>
        /// <param name="uvs">A list that will be cleared and populated with the UVs copied from this mesh.</param>
        public void GetUVs(int channel, List<Vector4> uvs)
        {
            if (uvs == null)
                throw new ArgumentNullException("uvs");

	        if(channel < 0 || channel > 3)
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

		/// <value>
		/// How many faces does this mesh have?
		/// </value>
		public int faceCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Length; }
		}

		/// <value>
		/// How many vertexes are in the positions array.
		/// </value>
		public int vertexCount
		{
			get { return m_Positions == null ? 0 : m_Positions.Length; }
		}

		/// <value>
		/// How many vertex indexes make up this mesh.
		/// </value>
		public int indexCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Sum(x => x.indexesInternal.Length); }
		}

		/// <value>
		/// How many triangles make up this mesh.
		/// </value>
		public int triangleCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Sum(x => x.indexesInternal.Length) / 3; }
		}

	    /// <summary>
	    /// In the editor, when a ProBuilderMesh is destroyed it will also destroy the MeshFilter.sharedMesh that is found with the parent GameObject. You may override this behaviour by subscribing to onDestroyObject.
	    /// </summary>
	    /// <value>
	    /// If onDestroyObject has a subscriber ProBuilder will invoke it instead of cleaning up unused meshes by itself.
	    /// </value>
	    /// <seealso cref="preserveMeshAssetOnDestroy"/>
	    public static event Action<ProBuilderMesh> meshWillBeDestroyed;

	    /// <value>
	    /// Invoked when the element selection changes on any ProBuilderMesh.
	    /// </value>
	    /// <seealso cref="SetSelectedFaces"/>
	    /// <seealso cref="SetSelectedVertexes"/>
	    /// <seealso cref="SetSelectedEdges"/>
	    public static event Action<ProBuilderMesh> elementSelectionChanged;

	    /// <summary>
	    /// Convenience property for getting the mesh from the MeshFilter component.
	    /// </summary>
	    internal Mesh mesh
	    {
		    get { return GetComponent<MeshFilter>().sharedMesh; }
		    set { gameObject.GetComponent<MeshFilter>().sharedMesh = value; }
	    }

	    internal int id
	    {
		    get { return gameObject.GetInstanceID(); }
	    }

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

			    return mesh.uv2 == null ? MeshSyncState.Lightmap : MeshSyncState.None;
		    }
	    }
    }
}
