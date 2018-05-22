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
    public sealed class ProBuilderMesh : MonoBehaviour
    {
	    const int k_UVChannelCount = 4;

#region Serialized Fields and Properties
        [SerializeField]
        [FormerlySerializedAs("_quads")]
        Face[] m_Faces;

        [SerializeField]
        [FormerlySerializedAs("_sharedIndices")]
        IntArray[] m_SharedIndices;

        [SerializeField]
        [FormerlySerializedAs("_vertices")]
        Vector3[] m_Positions;

        [SerializeField]
        [FormerlySerializedAs("_uv")]
        Vector2[] m_Textures0;

        [SerializeField]
        [FormerlySerializedAs("_uv3")]
        List<Vector4> m_Textures3;

        [SerializeField]
        [FormerlySerializedAs("_uv4")]
        List<Vector4> m_Textures4;

        [SerializeField]
        [FormerlySerializedAs("_tangents")]
        Vector4[] m_Tangents;

        [SerializeField]
        [FormerlySerializedAs("_sharedIndicesUV ")]
        IntArray[] m_SharedIndicesUV;

        [SerializeField]
        [FormerlySerializedAs("_colors")]
        Color[] m_Colors;

	    /// <value>
	    /// If false, ProBuilder will automatically create and scale colliders.
	    /// </value>
	    public bool userCollisions { get; set; }

	    [SerializeField]
	    bool m_IsSelectable = true;

	    /// <value>
	    /// If false mesh elements will not be selectable. This is used by @"UnityEditor.ProBuilder.ProBuilderEditor".
	    /// </value>
	    public bool isSelectable
	    {
		    get { return m_IsSelectable; }
		    set { m_IsSelectable = value; }
	    }

	    /// <value>
	    /// UV2 generation parameters.
	    /// </value>
	    public UnwrapParameters unwrapParameters { get; set; }

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
#endregion

#region Properties

	    internal Face[] facesInternal
        {
            get { return m_Faces; }
            set { m_Faces = value; }
        }

	    /// <summary>
	    /// Meshes are composed of vertices and faces. Faces primarily contain triangles and material information. With these components, ProBuilder will compile a mesh.
	    /// </summary>
	    /// <value>
	    /// A collection of the @"UnityEngine.ProBuilder.Face"'s that make up this mesh.
	    /// </value>
	    /// <seealso cref="SetFaces"/>
        public ReadOnlyCollection<Face> faces
        {
            get { return new ReadOnlyCollection<Face>(m_Faces); }
        }

	    /// <summary>
	    /// Set the internal faces array.
	    /// </summary>
	    /// <param name="faces">The new faces array.</param>
	    /// <exception cref="ArgumentNullException">Thrown if faces is null.</exception>
        public void SetFaces(IEnumerable<Face> faces)
        {
            if (faces == null)
                throw new ArgumentNullException("faces");
	        m_Faces = faces.ToArray();
        }

	    internal IntArray[] sharedIndicesInternal
	    {
		    get { return m_SharedIndices; }
		    set { m_SharedIndices = value; }
	    }

	    /// <summary>
	    /// ProBuilder makes the assumption that no @"UnityEngine.ProBuilder.Face" references a vertex used by another. However, we need a way to associate vertices in the editor for many operations. These vertices are usually called coincident, or shared vertices. ProBuilder manages these associations with the sharedIndexes array. Each array contains a list of triangles that point to vertices considered to be coincident. When ProBuilder compiles a UnityEngine.Mesh from the ProBuilderMesh, these vertices will be condensed to a single vertex where possible.
	    /// </summary>
	    /// <value>
	    /// The shared (or common) index array for this mesh.
	    /// </value>
	    /// <seealso cref="SetSharedIndexes(UnityEngine.ProBuilder.IntArray[])"/>
	    public ReadOnlyCollection<IntArray> sharedIndexes
	    {
		    get { return new ReadOnlyCollection<IntArray>(m_SharedIndices); }
	    }

	    /// <value>
	    /// Get a copy of the shared (or common) index array for this mesh.
	    /// </value>
	    /// <seealso cref="sharedIndexes"/>
	    public IntArray[] GetSharedIndexes()
	    {
		    int len = m_SharedIndices.Length;
		    IntArray[] copy = new IntArray[len];
		    for(var i = 0; i < len; i++)
			    copy[i] = new IntArray(m_SharedIndices[i]);
		    return copy;
	    }

	    /// <summary>
	    /// Set the sharedIndexes array for this mesh.
	    /// </summary>
	    /// <param name="indexes">
	    /// The new sharedIndexes array.
	    /// </param>
	    /// <seealso cref="sharedIndexes"/>
	    public void SetSharedIndexes(IntArray[] indexes)
	    {
		    if (indexes == null)
			    throw new ArgumentNullException("indexes");
		    int len = indexes.Length;
		    m_SharedIndices = new IntArray[len];
		    for (var i = 0; i < len; i++)
			    m_SharedIndices[i] = new IntArray(indexes[i]);
	    }

	    /// <summary>
	    /// Set the sharedIndexes array for this mesh with a lookup dictionary.
	    /// </summary>
	    /// <param name="indexes">
	    /// The new sharedIndexes array.
	    /// </param>
	    /// <seealso cref="sharedIndexes"/>
	    /// <seealso cref="IntArrayUtility.ToDictionary"/>
	    public void SetSharedIndexes(IEnumerable<KeyValuePair<int, int>> indexes)
	    {
		    if (indexes == null)
			    throw new ArgumentNullException("indexes");
		    m_SharedIndices = IntArrayUtility.ToIntArray(indexes);
	    }

        internal IntArray[] sharedIndicesUVInternal
        {
            get { return m_SharedIndicesUV; }
            set { m_SharedIndicesUV = value; }
        }

        internal IntArray[] GetSharedIndexesUV()
        {
            int sil = m_SharedIndicesUV.Length;
            IntArray[] sharedIndicesCopy = new IntArray[sil];
            for (var i = 0; i < sil; i++)
                sharedIndicesCopy[i] = m_SharedIndicesUV[i];
            return sharedIndicesCopy;
        }

	    internal void SetSharedIndexesUV(IntArray[] indices)
	    {
		    int len = indices == null ? 0 : indices.Length;
		    m_SharedIndicesUV = new IntArray[len];
		    for (var i = 0; i < len; i++)
			    m_SharedIndicesUV[i] = new IntArray(indices[i]);
	    }

        internal void SetSharedIndexesUV(IEnumerable<KeyValuePair<int, int>> indices)
        {
	        if (indices == null)
		        m_SharedIndicesUV = new IntArray[0];
			else
	            m_SharedIndicesUV = IntArrayUtility.ToIntArray(indices);
        }

        internal Vector3[] positionsInternal
        {
            get { return m_Positions; }
            set { m_Positions = value; }
        }

	    /// <value>
	    /// The vertex positions that make up this mesh.
	    /// </value>
	    /// <seealso cref="SetPositions"/>
        public ReadOnlyCollection<Vector3> positions
        {
            get { return new ReadOnlyCollection<Vector3>(m_Positions); }
        }

		/// <summary>
		/// Set the vertex positions for this mesh.
		/// </summary>
		/// <param name="array">The new positions array.</param>
		/// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
	    public void SetPositions(IEnumerable<Vector3> array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
	        m_Positions = array.ToArray();
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
            m_Tangents = tangent;
            m_Textures0 = uv0;
            m_Textures3 = uv3;
            m_Textures4 = uv4;

            if (applyMesh)
            {
                Mesh m = mesh;

                Vertex first = vertices[0];

                if (first.HasAttribute(MeshAttributes.Position)) m.vertices = position;
                if (first.HasAttribute(MeshAttributes.Color)) m.colors = color;
                if (first.HasAttribute(MeshAttributes.UV0)) m.uv = uv0;
                if (first.HasAttribute(MeshAttributes.Normal)) m.normals = normal;
                if (first.HasAttribute(MeshAttributes.Tangent)) m.tangents = tangent;
                if (first.HasAttribute(MeshAttributes.UV1)) m.uv2 = uv2;
                if (first.HasAttribute(MeshAttributes.UV2)) if (uv3 != null) m.SetUVs(2, uv3);
                if (first.HasAttribute(MeshAttributes.UV3)) if (uv4 != null) m.SetUVs(3, uv4);
            }
        }

	    /// <summary>
	    /// ProBuilderMesh doesn't store normals, so this function will either:
	    ///		1. Copy them from the MeshFilter.sharedMesh (if vertex count matches the @"UnityEngine.ProBuilder.ProBuilderMesh.vertexCount")
	    ///		2. Calculate a new set of normals using @"UnityEngine.ProBuilder.MeshUtility.CalculateNormals".
	    /// </summary>
	    /// <returns>An array of vertex normals.</returns>
	    /// <seealso cref="UnityEngine.ProBuilder.MeshUtility.CalculateNormals"/>
	    public Vector3[] GetNormals()
	    {
		    // If mesh isn't optimized try to return a copy from the compiled mesh
		    if (mesh != null && mesh.vertexCount == vertexCount)
		    {
			    var nrm = mesh.normals;
			    if (nrm != null && nrm.Length == vertexCount)
				    return nrm;
		    }

		    return MeshUtility.CalculateNormals(this);
	    }

        internal Color[] colorsInternal
		{
			get { return m_Colors; }
			set { m_Colors = value; }
		}

		/// <value>
		/// Get the vertex colors array for this mesh.
		/// </value>
		/// <seealso cref="SetColors"/>
	    public ReadOnlyCollection<Color> colors
        {
            get { return m_Colors != null ? new ReadOnlyCollection<Color>(m_Colors) : null; }
        }

	    /// <summary>
	    /// Set the colors array for this mesh. Colors size must match vertex count.
	    /// </summary>
	    /// <param name="array"></param>
	    /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
	    /// <exception cref="ArgumentOutOfRangeException">Thrown if array.Length does not equal @"UnityEngine.ProBuilder.ProBuilderMesh.vertexCount".</exception>
        public void SetColors(IEnumerable<Color> array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            int len = array.Count();
            if (len != vertexCount)
                throw new ArgumentOutOfRangeException("array", "Array length must match vertex count.");
	        m_Colors = array.ToArray();
        }

		/// <value>
		/// Get the user-set tangents array for this mesh. If tangents have not been explictly set, this value will be null.
		/// </value>
		/// <remarks>
		/// To get the generated tangents that are applied to the mesh through Refresh(), use GetTangents().
		/// </remarks>
		/// <seealso cref="SetTangents"/>
		/// <seealso cref="GetTangents"/>
	    public ReadOnlyCollection<Vector4> tangents
	    {
		    get { return m_Tangents == null || m_Tangents.Length != vertexCount
			    ? null
			    : new ReadOnlyCollection<Vector4>(m_Tangents); }
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

	    /// <summary>
        /// Set the tangent array on this mesh. The length must match vertexCount.
        /// </summary>
        /// <param name="array">The new tangents array.</param>
        public void SetTangents(IEnumerable<Vector4> array)
	    {
		    if (array == null)
			    m_Tangents = null;
	        else if (array.Count() != vertexCount)
		        throw new ArgumentOutOfRangeException("array", "Tangent array length must match vertex count");
		    else
		        m_Tangents = array.ToArray();
        }

        internal Vector2[] texturesInternal
		{
			get { return m_Textures0; }
			set { m_Textures0 = value; }
		}

	    /// <value>
	    /// The UV0 channel. Null if not present.
	    /// </value>
	    /// <seealso cref="SetUVs(Vector2[])"/>
	    /// <seealso cref="GetUVs"/>
	    public ReadOnlyCollection<Vector2> textures
	    {
		    get { return m_Textures0 != null ? new ReadOnlyCollection<Vector2>(m_Textures0) : null; }
	    }

	    /// <summary>
	    /// Set the UV channel array.
	    /// </summary>
	    /// <param name="uvs">The new UV array.</param>
	    /// <exception cref="ArgumentNullException">Thrown if uvs is null.</exception>
	    /// <exception cref="ArgumentOutOfRangeException">Thrown if uvs length does not match the vertex count.</exception>
	    public void SetUVs(Vector2[] uvs)
	    {
		    if(uvs == null)
			    throw new ArgumentNullException("uvs");
		    int vc = vertexCount;
		    if(uvs.Length != vc)
			    throw new ArgumentOutOfRangeException("uvs");
		    m_Textures0 = new Vector2[vc];
		    Array.Copy(uvs, m_Textures0, vc);
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
                    if (m_Textures3 != null)
                        uvs.AddRange(m_Textures3);
                    break;

                case 3:
                    if (m_Textures4 != null)
                        uvs.AddRange(m_Textures4);
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
                    m_Textures3 = uvs != null ? new List<Vector4>(uvs) : null;
                    break;

                case 3:
                    m_Textures4 = uvs != null ? new List<Vector4>(uvs) : null;
                    break;
            }
        }

        internal bool hasUv2
		{
			get { return mesh.uv2 != null && mesh.uv2.Length == vertexCount; }
		}

	    internal bool hasUv3
		{
			get { return m_Textures3 != null && m_Textures3.Count == vertexCount; }
		}

	    internal bool hasUv4
		{
			get { return m_Textures4 != null && m_Textures4.Count == vertexCount; }
		}

		/// <value>
		/// How many faces does this mesh have?
		/// </value>
		public int faceCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Length; }
		}

		/// <value>
		/// How many vertices are in the positions array.
		/// </value>
		public int vertexCount
		{
			get { return m_Positions == null ? 0 : m_Positions.Length; }
		}

		/// <value>
		/// How many triangle indices make up this mesh.
		/// </value>
		/// <remarks>This calls Linq Sum on the faces array. Cache this value if you're accessing it frequently.</remarks>
		public int triangleCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Sum(x => x.indices.Length); }
		}

	    /// <summary>
	    /// In the editor, when a ProBuilderMesh is destroyed it will also destroy the MeshFilter.sharedMesh that is found with the parent GameObject. You may override this behaviour by subscribing to onDestroyObject.
	    /// </summary>
	    /// <value>
	    /// If onDestroyObject has a subscriber ProBuilder will invoke it instead of cleaning up unused meshes by itself.
	    /// </value>
	    /// <seealso cref="preserveMeshAssetOnDestroy"/>
	    public static event Action<ProBuilderMesh> onDestroyObject;

	    /// <value>
	    /// Invoked when the element selection changes on any ProBuilderMesh.
	    /// </value>
	    /// <seealso cref="SetSelectedFaces"/>
	    /// <seealso cref="SetSelectedVertices"/>
	    /// <seealso cref="SetSelectedEdges"/>
	    public static event Action<ProBuilderMesh> onElementSelectionChanged;

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
#endregion

#region Initialize and Destroy
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
			m_Textures3 = null;
			m_Textures4 = null;
			m_Tangents = null;
			m_SharedIndices = new IntArray[0];
			m_SharedIndicesUV = new IntArray[0];
			m_Colors = null;
			ClearSelection();
		}

//		void Awake()
//		{
//			if (GetComponent<MeshRenderer>().isPartOfStaticBatch)
//				return;
//
//			// Absolutely no idea why normals sometimes go haywire
//			Vector3[] normals = mesh != null ? mesh.normals : null;
//
//			if (normals == null ||
//			    normals.Length != mesh.vertexCount ||
//			    (normals.Length > 0 && normals[0] == Vector3.zero))
//			{
//				Log.Info("Mesh normals broken on play mode start.");
//
//				// means this object is probably just now being instantiated
//				if (m_Positions == null)
//					return;
//
//				ToMesh();
//				Refresh();
//			}
//		}

		void OnDestroy()
		{
			// Time.frameCount is zero when loading scenes in the Editor. It's the only way I could figure to
			// differentiate between OnDestroy invoked from user delete & editor scene loading.
			if (!preserveMeshAssetOnDestroy &&
			    Application.isEditor &&
			    !Application.isPlaying &&
			    Time.frameCount > 0)
			{
				if (onDestroyObject != null)
					onDestroyObject(this);
				else
					DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh, true);
			}
		}

		/// <summary>
		///	Creates a new #pb_Object using passed vertices to construct geometry.
		///	Typically you would not call this directly, as the #ProBuilder class contains
		///	a wrapper for this purpose.
		///	@param vertices A vertex array (Vector3[]) containing the points to be used in
		///	the construction of the #pb_Object.  Vertices must be wound in counter-clockise
		///	order.  Triangles will be wound in vertex groups of 4, with the winding order
		///	0,1,2 1,3,2.  Ex:
		///	\code{.cs}
		///	// Creates a pb_Object plane
		///	pb_Object.CreateInstanceWithPoints(new Vector3[4]{
		///		new Vector3(-.5f, -.5f, 0f),
		///		new Vector3(.5f, -.5f, 0f),
		///		new Vector3(-.5f, .5f, 0f),
		///		new Vector3(.5f, .5f, 0f)
		///		});
		///
		///	\endcode
		/// </summary>
		/// <param name="vertices"></param>
		/// <returns>The resulting #pb_Object.</returns>
		internal static ProBuilderMesh CreateInstanceWithPoints(Vector3[] vertices)
		{
			if (vertices.Length % 4 != 0)
			{
				Log.Warning("Invalid Geometry.  Make sure vertices in are pairs of 4 (faces).");
				return null;
			}

			GameObject go = new GameObject();
			ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
			go.name = "ProBuilder Mesh";
			pb.GeometryWithPoints(vertices);

			return pb;
		}

		/// <summary>
		/// Create a new GameObject with a ProBuilderMesh component, MeshFilter, and MeshRenderer, then initializes the ProBuilderMesh with a set of positions and faces.
		/// </summary>
		/// <param name="vertices">Vertex positions array.</param>
		/// <param name="faces">Faces array.</param>
		/// <returns></returns>
		public static ProBuilderMesh CreateInstanceWithVerticesFaces(IEnumerable<Vector3> vertices, IEnumerable<Face> faces)
		{
			GameObject go = new GameObject();
			ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
			go.name = "ProBuilder Mesh";
			pb.GeometryWithVerticesFaces(vertices, faces);
			return pb;
		}
#endregion

#region Selection

	    [SerializeField] int[] m_selectedFaces = new int[] { };
	    [SerializeField] Edge[] m_SelectedEdges = new Edge[] { };
	    [SerializeField] int[] m_selectedTriangles = new int[] { };

	    /// <value>
	    /// Get the number of faces that are currently selected on this object.
	    /// </value>
	    public int selectedFaceCount
	    {
		    get { return m_selectedFaces.Length; }
	    }

	    /// <value>
	    /// Get the number of selected vertex indices.
	    /// </value>
	    public int selectedVertexCount
	    {
		    get { return m_selectedTriangles.Length; }
	    }

	    /// <value>
	    /// Get the number of selected edges.
	    /// </value>
	    public int selectedEdgeCount
	    {
		    get { return m_SelectedEdges.Length; }
	    }

	    /// <summary>
		/// Get a copy of the selected face array.
		/// </summary>
		public Face[] GetSelectedFaces()
		{
			int len = m_selectedFaces.Length;
			var selected = new Face[len];
			for (var i = 0; i < len; i++)
				selected[i] = m_Faces[m_selectedFaces[i]];
			return selected;
		}

	    internal Face[] selectedFacesInternal
	    {
		    get { return GetSelectedFaces(); }
	    }

	    /// <value>
	    /// A collection of the currently selected faces by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.faces" array.
	    /// </value>
	    public ReadOnlyCollection<int> selectedFaceIndexes
	    {
		    get { return new ReadOnlyCollection<int>(m_selectedFaces); }
	    }

	    /// <value>
	    /// A collection of the currently selected vertices by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.positions" array.
	    /// </value>
	    public ReadOnlyCollection<int> selectedVertices
	    {
			get { return new ReadOnlyCollection<int>(m_selectedTriangles); }
	    }

	    /// <value>
	    /// A collection of the currently selected edges.
	    /// </value>
	    public ReadOnlyCollection<Edge> selectedEdges
	    {
		    get { return new ReadOnlyCollection<Edge>(m_SelectedEdges); }
	    }

	    internal int[] selectedIndicesInternal
	    {
		    get { return m_selectedTriangles; }
	    }

		internal void AddToFaceSelection(Face face)
		{
			int index = Array.IndexOf(this.facesInternal, face);
			if (index > -1)
				SetSelectedFaces(m_selectedFaces.Add(index));
		}

		internal void AddToFaceSelection(int index)
		{
			if (index > -1)
				SetSelectedFaces(m_selectedFaces.Add(index));
		}

		/// <summary>
		/// Set the face selection for this mesh. Also sets the vertex and edge selection to match.
		/// </summary>
		/// <param name="selected">The new face selection.</param>
	    public void SetSelectedFaces(IEnumerable<Face> selected)
		{
			SetSelectedFaces(selected != null ? selected.Select(x => Array.IndexOf(facesInternal, x)) : null);
		}

		internal void SetSelectedFaces(IEnumerable<int> selected)
		{
			if (selected == null)
			{
				ClearSelection();
			}
			else
			{
				m_selectedFaces = selected.ToArray();
				m_selectedTriangles = m_selectedFaces.SelectMany(x => facesInternal[x].distinctIndices).ToArray();
				m_SelectedEdges = m_selectedFaces.SelectMany(x => facesInternal[x].edges).ToArray();
			}

			if (onElementSelectionChanged != null)
				onElementSelectionChanged(this);
		}

	    /// <summary>
	    /// Set the edge selection for this mesh. Also sets the face and vertex selection to match.
	    /// </summary>
	    /// <param name="edges">The new edge selection.</param>
		public void SetSelectedEdges(IEnumerable<Edge> edges)
		{
			if (edges == null)
			{
				ClearSelection();
			}
			else
			{
				m_selectedFaces = new int[0];
				m_SelectedEdges = edges.ToArray();
				m_selectedTriangles = m_SelectedEdges.AllTriangles();
			}

			if (onElementSelectionChanged != null)
				onElementSelectionChanged(this);
		}

		/// <summary>
		/// Sets the selected vertices array. Clears SelectedFaces and SelectedEdges arrays.
		/// </summary>
		/// <param name="vertices">The new vertex selection.</param>
		public void SetSelectedVertices(IEnumerable<int> vertices)
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new Edge[0];
			m_selectedTriangles = vertices != null ? vertices.Distinct().ToArray() : new int[0];

			if (onElementSelectionChanged != null)
				onElementSelectionChanged(this);
		}

		/// <summary>
		/// Removes face at index in SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
		/// </summary>
		/// <param name="index"></param>
		internal void RemoveFromFaceSelectionAtIndex(int index)
		{
			SetSelectedFaces(m_selectedFaces.RemoveAt(index));
		}

		/// <summary>
		/// Removes face from SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
		/// </summary>
		/// <param name="face"></param>
		internal void RemoveFromFaceSelection(Face face)
		{
			int indx = Array.IndexOf(facesInternal, face);

			if (indx > -1)
				SetSelectedFaces(m_selectedFaces.Remove(indx));
		}

		/// <summary>
		/// Clears selected face, edge, and vertex arrays. You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
		/// </summary>
		public void ClearSelection()
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new Edge[0];
			m_selectedTriangles = new int[0];
		}
#endregion

#region Mesh Building
		void GeometryWithPoints(Vector3[] vertices)
		{
			// Wrap in faces
			Face[] f = new Face[vertices.Length / 4];

			for (int i = 0; i < vertices.Length; i += 4)
			{
				f[i / 4] = new Face(new int[6]
					{
						i + 0, i + 1, i + 2,
						i + 1, i + 3, i + 2
					},
					BuiltinMaterials.DefaultMaterial,
					new AutoUnwrapSettings(),
					0,
					-1,
					-1,
					false);
			}

            Clear();
            SetPositions(vertices);
			SetFaces(f);
			m_SharedIndices = IntArrayUtility.GetSharedIndexesWithPositions(vertices);

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Clear all mesh attributes and reinitialize with new positions and face collections.
		/// </summary>
		/// <param name="vertices">Vertex positions array.</param>
		/// <param name="faces">Faces array.</param>
		public void GeometryWithVerticesFaces(IEnumerable<Vector3> vertices, IEnumerable<Face> faces)
		{
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Clear();
            m_Positions = vertices.ToArray();
			m_Faces = faces.ToArray();
			SetSharedIndexes(IntArrayUtility.GetSharedIndexesWithPositions(m_Positions));
			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Ensure that the UnityEngine.Mesh is in sync with the ProBuilderMesh.
		/// </summary>
		/// <returns>A flag describing the state of the synchronicity between the MeshFilter.sharedMesh and ProBuilderMesh components.</returns>
		public MeshSyncState Verify()
		{
			if (mesh == null)
				return MeshSyncState.Null;

			int meshNo;

			int.TryParse(mesh.name.Replace("pb_Mesh", ""), out meshNo);

			if (meshNo != id)
				return MeshSyncState.InstanceIDMismatch;

			return mesh.uv2 == null ? MeshSyncState.Lightmap : MeshSyncState.None;
		}

	    /// <summary>
	    /// Wraps ToMesh and Refresh in a single call.
	    /// </summary>
	    /// <seealso cref="ToMesh()"/>
	    /// <seealso cref="Refresh"/>
	    public void Rebuild()
	    {
		    ToMesh();
		    Refresh();
	    }

		/// <summary>
		/// Rebuild the mesh positions and submeshes. If vertex count matches new positions array the existing attributes are kept, otherwise the mesh is cleared. UV2 is the exception, it is always cleared.
		/// </summary>
		public void ToMesh()
		{
			// ReSharper disable once IntroduceOptionalParameters.Global
			ToMesh(MeshTopology.Triangles);
		}

		/// <summary>
		/// Rebuild the mesh positions and submeshes. If vertex count matches new positions array the existing attributes are kept, otherwise the mesh is cleared. UV2 is the exception, it is always cleared.
		/// </summary>
		/// <param name="preferredTopology">Triangles and Quads are supported.</param>
		public void ToMesh(MeshTopology preferredTopology)
		{
			Mesh m = mesh;

			// if the mesh vertex count hasn't been modified, we can keep most of the mesh elements around
			if (m != null && m.vertexCount == m_Positions.Length)
				m = mesh;
			else if (m == null)
				m = new Mesh();
			else
				m.Clear();

			m.vertices = m_Positions;
			m.uv2 = null;

			Submesh[] submeshes = Face.GetSubmeshes(facesInternal, preferredTopology);
            m.subMeshCount = submeshes.Length;

			for (int i = 0; i < m.subMeshCount; i++)
#if UNITY_5_5_OR_NEWER
				m.SetIndices(submeshes[i].m_Indices, submeshes[i].m_Topology, i, false);
#else
				m.SetIndices(submeshes[i].indices, submeshes[i].topology, i);
#endif

			m.name = string.Format("pb_Mesh{0}", id);

			GetComponent<MeshFilter>().sharedMesh = m;
			GetComponent<MeshRenderer>().sharedMaterials = submeshes.Select(x => x.m_Material).ToArray();
		}

		/// <summary>
		/// Deep copy the mesh attribute arrays back to itself. Useful when copy/paste creates duplicate references.
		/// </summary>
		internal void MakeUnique()
		{
			SetPositions(positions);
			SetSharedIndexes(sharedIndicesInternal);
			SetSharedIndexesUV(sharedIndicesUVInternal);
			SetFaces(faces);
			List<Vector4> uvs = new List<Vector4>();
			for (var i = 0; i < k_UVChannelCount; i++)
			{
				GetUVs(i, uvs);
				SetUVs(i, uvs);
			}
			SetTangents(tangents);
			SetColors(colors);
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
			if(other == null)
				throw new ArgumentNullException("other");

		    Clear();
			SetPositions(other.positions);
		    SetSharedIndexes(other.sharedIndicesInternal);
		    SetSharedIndexesUV(other.sharedIndicesUVInternal);
		    SetFaces(other.faces);

		    List<Vector4> uvs = new List<Vector4>();

		    for (var i = 0; i < k_UVChannelCount; i++)
		    {
			    other.GetUVs(1, uvs);
			    SetUVs(1, uvs);
		    }

			SetTangents(other.tangents);
		    SetColors(other.colors);
		    userCollisions = other.userCollisions;
		    isSelectable = other.isSelectable;
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
				RefreshUV();

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
			Mesh m = mesh;

			m.RecalculateBounds();

			if (!userCollisions && GetComponent<Collider>())
			{
				foreach (Collider c in gameObject.GetComponents<Collider>())
				{
					System.Type t = c.GetType();

					if (t == typeof(BoxCollider))
					{
						((BoxCollider) c).center = m.bounds.center;
						((BoxCollider) c).size = m.bounds.size;
					}
					else if (t == typeof(SphereCollider))
					{
						((SphereCollider) c).center = m.bounds.center;
						((SphereCollider) c).radius = Math.LargestValue(m.bounds.extents);
					}
					else if (t == typeof(CapsuleCollider))
					{
						((CapsuleCollider) c).center = m.bounds.center;
						Vector2 xy = new Vector2(m.bounds.extents.x, m.bounds.extents.z);
						((CapsuleCollider) c).radius = Math.LargestValue(xy);
						((CapsuleCollider) c).height = m.bounds.size.y;
					}
					else if (t == typeof(WheelCollider))
					{
						((WheelCollider) c).center = m.bounds.center;
						((WheelCollider) c).radius = Math.LargestValue(m.bounds.extents);
					}
					else if (t == typeof(MeshCollider))
					{
						gameObject.GetComponent<MeshCollider>().sharedMesh = null; // this is stupid.
						gameObject.GetComponent<MeshCollider>().sharedMesh = m;
					}
				}
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
			while (System.Array.Exists(facesInternal, element => element.textureGroup == i))
				i++;

			return i;
		}

		/// <summary>
		/// Returns a new unused element group.
		/// Will be greater than or equal to i.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		internal int UnusedElementGroup(int i = 1)
		{
			while (System.Array.Exists(facesInternal, element => element.elementGroup == i))
				i++;

			return i;
		}

		/// <summary>
		/// Re-project AutoUV faces and re-assign ManualUV to mesh.uv channel.
		/// </summary>
		void RefreshUV()
		{
			RefreshUV(facesInternal);
		}

		/// <summary>
		/// Re-project AutoUV faces and re-assign ManualUV to mesh.uv channel.
		/// </summary>
		/// <param name="facesToRefresh"></param>
		internal void RefreshUV(IEnumerable<Face> facesToRefresh)
		{
			Vector2[] oldUvs = mesh.uv;
			Vector2[] newUVs;

			// thanks to the upgrade path, this is necessary.  maybe someday remove it.
			if (m_Textures0 != null && m_Textures0.Length == vertexCount)
			{
				newUVs = m_Textures0;
			}
			else
			{
				if (oldUvs != null && oldUvs.Length == vertexCount)
				{
					newUVs = oldUvs;
				}
				else
				{
					foreach (Face f in this.facesInternal)
						f.manualUV = false;

					// this necessitates rebuilding ALL the face uvs, so make sure we do that.
					facesToRefresh = this.facesInternal;

					newUVs = new Vector2[vertexCount];
				}
			}

			int n = -2;
			var textureGroups = new Dictionary<int, List<Face>>();
			bool anyWorldSpace = false;
			List<Face> group;

			foreach (Face f in facesToRefresh)
			{
				if (f.uv.useWorldSpace)
					anyWorldSpace = true;

				if (f == null || f.manualUV)
					continue;

				if (f.textureGroup > 0 && textureGroups.TryGetValue(f.textureGroup, out group))
					group.Add(f);
				else
					textureGroups.Add(f.textureGroup > 0 ? f.textureGroup : n--, new List<Face>() {f});
			}

			// Add any non-selected faces in texture groups to the update list
			if (this.facesInternal.Length != facesToRefresh.Count())
			{
				foreach (Face f in this.facesInternal)
				{
					if (f.manualUV)
						continue;

					if (textureGroups.ContainsKey(f.textureGroup) && !textureGroups[f.textureGroup].Contains(f))
						textureGroups[f.textureGroup].Add(f);
				}
			}

			n = 0;

			Vector3[] world = anyWorldSpace ? this.VerticesInWorldSpace() : null;

			foreach (KeyValuePair<int, List<Face>> kvp in textureGroups)
			{
				Vector3 nrm;
				int[] indices = kvp.Value.SelectMany(x => x.distinctIndices).ToArray();

				if (kvp.Value.Count > 1)
					nrm = Projection.FindBestPlane(m_Positions, indices).normal;
				else
					nrm = Math.Normal(this, kvp.Value[0]);

				if (kvp.Value[0].uv.useWorldSpace)
					UnwrappingUtility.PlanarMap2(world, newUVs, indices, kvp.Value[0].uv, transform.TransformDirection(nrm));
				else
					UnwrappingUtility.PlanarMap2(positionsInternal, newUVs, indices, kvp.Value[0].uv, nrm);
			}

			m_Textures0 = newUVs;
			mesh.uv = newUVs;

			if (hasUv3) mesh.SetUVs(2, m_Textures3);
			if (hasUv4) mesh.SetUVs(3, m_Textures4);
		}

		void RefreshColors()
		{
			Mesh m = GetComponent<MeshFilter>().sharedMesh;

			if (m_Colors == null || m_Colors.Length != vertexCount)
				m_Colors = ArrayUtility.FilledArray<Color>(Color.white, vertexCount);

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

			if (m_Colors == null)
                m_Colors = ArrayUtility.FilledArray<Color>(Color.white, vertexCount);

			foreach (int i in face.distinctIndices)
				m_Colors[i] = color;
		}

		void RefreshNormals()
		{
			GetComponent<MeshFilter>().sharedMesh.normals = MeshUtility.CalculateNormals(this);
		}

		void RefreshTangents()
		{
			Mesh m = GetComponent<MeshFilter>().sharedMesh;

			if (m_Tangents != null && m_Tangents.Length == vertexCount)
				m.tangents = m_Tangents;
			else
				MeshUtility.GenerateTangent(m);
		}
#endregion
    }
}
