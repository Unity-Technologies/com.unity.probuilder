using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using System;
using System.Collections.ObjectModel;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// ProBuilder mesh class. Stores all the information necessary to create a UnityEngine.Mesh.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class ProBuilderMesh : MonoBehaviour
    {
#region Fields, Properties, and Events
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
        IntArray[] m_SharedIndicesUV = new IntArray[0];

        [SerializeField]
        [FormerlySerializedAs("_colors")]
        Color[] m_Colors;

	    /// <summary>
	    /// If false, ProBuilder will automatically create and scale colliders.
	    /// </summary>
	    public bool userCollisions { get; set; }

	    [SerializeField]
	    bool m_IsSelectable = true;

	    /// <summary>
	    /// If false mesh elements will not be selectable.
	    /// </summary>
	    public bool isSelectable
	    {
		    get { return m_IsSelectable; }
		    set { m_IsSelectable = value; }
	    }

	    /// <summary>
	    /// UV2 generation parameters.
	    /// </summary>
	    public UnwrapParamaters unwrapParameters { get; set; }

	    [FormerlySerializedAs("dontDestroyMeshOnDelete")]
	    [SerializeField]
	    bool m_PreserveMeshAssetOnDestroy;

        /// <summary>
        /// If "Meshes are Assets" feature is enabled, this is used to relate pb_Objects to stored meshes.
        /// </summary>
        [SerializeField]
        internal string assetGuid;

        /// <summary>
        /// Usually when you delete a pb_Object you want to also clean up the mesh asset.
        /// However, there are situations you'd want to keep the mesh around, like when stripping probuilder scripts.
        /// </summary>
        public bool preserveMeshAssetOnDestroy
        {
            get { return m_PreserveMeshAssetOnDestroy; }
            set { m_PreserveMeshAssetOnDestroy = value; }
        }

        /// <summary>
        /// Get a reference to the faces array on this mesh.
        /// </summary>
        internal Face[] facesInternal
        {
            get { return m_Faces; }
            set { m_Faces = value; }
        }

        public ReadOnlyCollection<Face> faces
        {
            get { return new ReadOnlyCollection<Face>(m_Faces); }
        }

        public void SetFaces(Face[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            int len = array.Length;
            m_Faces = new Face[len];
            Array.Copy(array, m_Faces, len);
        }

	    internal IntArray[] sharedIndicesInternal
	    {
		    get { return m_SharedIndices; }
		    set { m_SharedIndices = value; }
	    }

	    public ReadOnlyCollection<IntArray> sharedIndices
	    {
		    get { return new ReadOnlyCollection<IntArray>(m_SharedIndices); }
	    }

	    public IntArray[] GetSharedIndices()
	    {
		    int len = m_SharedIndices.Length;
		    IntArray[] copy = new IntArray[len];
		    for(var i = 0; i < len; i++)
			    copy[i] = new IntArray(m_SharedIndices[i]);
		    return copy;
	    }

	    public void SetSharedIndices(IntArray[] indices)
	    {
		    if (indices == null)
			    throw new ArgumentNullException("indices");
		    int len = indices.Length;
		    m_SharedIndices = new IntArray[len];
		    for (var i = 0; i < len; i++)
			    m_SharedIndices[i] = new IntArray(indices[i]);
	    }

	    public void SetSharedIndices(IEnumerable<KeyValuePair<int, int>> indices)
	    {
		    if (indices == null)
			    throw new ArgumentNullException("indices");
		    m_SharedIndices = IntArrayUtility.ToSharedIndices(indices);
	    }

	    /// <summary>
        /// Get a reference to the shared uv indices array.
        /// </summary>
        internal IntArray[] sharedIndicesUVInternal
        {
            get { return m_SharedIndicesUV; }
            set { m_SharedIndicesUV = value; }
        }

        /// <summary>
        /// Returns a copy of the sharedIndicesUV array.
        /// </summary>
        /// <returns></returns>
        public IntArray[] GetSharedIndicesUV()
        {
            int sil = m_SharedIndicesUV.Length;
            IntArray[] sharedIndicesCopy = new IntArray[sil];
            for (var i = 0; i < sil; i++)
                sharedIndicesCopy[i] = m_SharedIndicesUV[i];
            return sharedIndicesCopy;
        }

	    internal void SetSharedIndicesUV(IntArray[] indices)
	    {
		    int len = indices == null ? 0 : indices.Length;
		    m_SharedIndicesUV = new IntArray[len];
		    for (var i = 0; i < len; i++)
			    m_SharedIndicesUV[i] = new IntArray(indices[i]);
	    }

        internal void SetSharedIndicesUV(IEnumerable<KeyValuePair<int, int>> indices)
        {
	        if (indices == null)
		        m_SharedIndicesUV = null;
			else
	            m_SharedIndicesUV = IntArrayUtility.ToSharedIndices(indices);
        }

        /// <summary>
        /// Get a reference to the positions array.
        /// </summary>
        /// <remarks>
        /// The stored vertex positions array is not guaranteed to match the Unity mesh vertices array.
        /// </remarks>
        internal Vector3[] positionsInternal
        {
            get { return m_Positions; }
            set { m_Positions = value; }
        }

        public ReadOnlyCollection<Vector3> positions
        {
            get { return new ReadOnlyCollection<Vector3>(m_Positions); }
        }

        public void SetPositions(Vector3[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            int len = array.Length;
            m_Positions = new Vector3[len];
            Array.Copy(array, m_Positions, len);
        }

        /// <summary>
        /// Set the vertex element arrays on this pb_Object. By default this function does not apply these values to the mesh.  An optional parameter `applyMesh` will apply elements to the mesh - note that this should only be used when the mesh is in its original state, not optimized (meaning it won't affect triangles which can be modified by Optimize).
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="applyMesh"></param>
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

                if (first.hasPosition) m.vertices = position;
                if (first.hasColor) m.colors = color;
                if (first.hasUv0) m.uv = uv0;
                if (first.hasNormal) m.normals = normal;
                if (first.hasTangent) m.tangents = tangent;
                if (first.hasUv2) m.uv2 = uv2;
#if !UNITY_4_7 && !UNITY_5_0
                if (first.hasUv3) if (uv3 != null) m.SetUVs(2, uv3);
                if (first.hasUv4) if (uv4 != null) m.SetUVs(3, uv4);
#endif
            }
        }

	    /// <summary>
	    /// pb_Object doesn't store normals, so this function either:
	    ///		1. Copies them from the MeshFilter.sharedMesh (if vertex count matches the pb_Object::vertexCount)
	    ///		2. Calculates a new set of normals and returns.
	    /// </summary>
	    /// <returns></returns>
	    public Vector3[] GetNormals()
	    {
		    // If mesh isn't optimized try to return a copy from the compiled mesh
		    if (mesh.vertexCount == vertexCount)
			    return mesh.normals;

		    return MeshUtility.CalculateNormals(this);
	    }

        /// <summary>
        /// Get a reference to the colors array.
        /// </summary>
        internal Color[] colorsInternal
		{
			get { return m_Colors; }
			set { m_Colors = value; }
		}

        public ReadOnlyCollection<Color> colors
        {
            get { return new ReadOnlyCollection<Color>(m_Colors); }
        }

        public void SetColors(Color[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            int len = array.Length;
            if (len != vertexCount)
                throw new ArgumentOutOfRangeException("array", "Array length must match vertex count.");
            m_Colors = new Color[len];
            Array.Copy(array, m_Colors, len);
        }

	    public ReadOnlyCollection<Vector4> tangents
	    {
		    get { return new ReadOnlyCollection<Vector4>(m_Tangents); }
	    }

        /// <summary>
        /// Set the tangent array on this mesh.
        /// </summary>
        /// <param name="array"></param>
        public void SetTangents(Vector4[] array)
        {
	        if (array == null)
		        throw new ArgumentNullException("array");
	        int vc = vertexCount;
	        if (array.Length != vc)
		        throw new ArgumentOutOfRangeException("array", "Tangent array length must match vertex count");
	        m_Tangents = new Vector4[vc];
	        Array.Copy(array, m_Tangents, vc);
        }

        /// <summary>
        /// Get a reference to uv0 channel.
        /// </summary>
        internal Vector2[] texturesInternal
		{
			get { return m_Textures0; }
			set { m_Textures0 = value; }
		}

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
        ///	Copy values in UV channel to uvs.
        ///	channel is zero indexed.
        ///		mesh.uv0/1 = 0
        ///		mesh.uv2 = 1
        ///		mesh.uv3 = 2
        ///		mesh.uv4 = 3
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="uvs"></param>
        public void GetUVs(int channel, List<Vector4> uvs)
        {
            if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();

            switch (channel)
            {
                case 0:
                default:
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
        /// Sets the UVs on channel.
        /// </summary>
        /// <remarks>Does not apply to mesh (use Refresh to reflect changes after application).</remarks>
        /// <param name="channel"></param>
        /// <param name="uvs"></param>
        public void SetUVs(int channel, List<Vector4> uvs)
        {
            switch (channel)
            {
                case 1:
                    mesh.uv2 = uvs.Cast<Vector2>().ToArray();
                    break;

                case 2:
                    m_Textures3 = uvs;
                    break;

                case 3:
                    m_Textures4 = uvs;
                    break;

                case 0:
                default:
                    m_Textures0 = uvs.Cast<Vector2>().ToArray();
                    break;
            }
        }

        /// <summary>
        /// True if this mesh has a valid UV2 channel.
        /// </summary>
        public bool hasUv2
		{
			get { return mesh.uv2 != null && mesh.uv2.Length == vertexCount; }
		}

		/// <summary>
		/// True if this mesh has a valid UV3 channel.
		/// </summary>
		public bool hasUv3
		{
			get { return m_Textures3 != null && m_Textures3.Count == vertexCount; }
		}

		/// <summary>
		/// True if this mesh has a valid UV4 channel.
		/// </summary>
		public bool hasUv4
		{
			get { return m_Textures4 != null && m_Textures4.Count == vertexCount; }
		}

		/// <summary>
		/// How many faces does this mesh have?
		/// </summary>
		public int faceCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Length; }
		}

		/// <summary>
		/// How many vertices are in the positions array.
		/// </summary>
		public int vertexCount
		{
			get { return m_Positions == null ? 0 : m_Positions.Length; }
		}

		/// <summary>
		/// How many triangle indices make up this mesh.
		/// </summary>
		/// <remarks>This calls Linq Sum on the faces array. Cache this value if you're accessing it frequently.</remarks>
		public int triangleCount
		{
			get { return m_Faces == null ? 0 : m_Faces.Sum(x => x.indices.Length); }
		}

	    /// <summary>
	    /// If onDestroyObject has a subscriber ProBuilder will invoke it instead of cleaning up unused meshes by itself.
	    /// </summary>
	    public static event Action<ProBuilderMesh> onDestroyObject;

	    internal static event Action<ProBuilderMesh> onElementSelectionChanged;

	    /// <summary>
	    /// Convenience property for getting the mesh from the MeshFilter component.
	    /// </summary>
	    internal Mesh mesh
	    {
		    get { return GetComponent<MeshFilter>().sharedMesh; }
		    set { gameObject.GetComponent<MeshFilter>().sharedMesh = value; }
	    }

	    /// <summary>
	    /// Get a unique id for this pb_Object. Not guaranteed to be persistent.
	    /// </summary>
	    public int id
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
			m_SharedIndicesUV = null;
			m_Colors = null;
			SetSelectedTriangles(null);
		}

		void Awake()
		{
			if (GetComponent<MeshRenderer>().isPartOfStaticBatch)
				return;

			// Absolutely no idea why normals sometimes go haywire
			Vector3[] normals = mesh != null ? mesh.normals : null;

			if (normals == null ||
			    normals.Length != mesh.vertexCount ||
			    (normals.Length > 0 && normals[0] == Vector3.zero))
			{
				// means this object is probably just now being instantiated
				if (m_Positions == null)
					return;

				ToMesh();
				Refresh();
			}
		}

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
					GameObject.DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh, true);
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
		/// Creates a new pb_Object with passed vertex positions array and pb_Face array. Allows for a great deal of control when constructing geometry.
		/// </summary>
		/// <param name="v">Vertex positions array.</param>
		/// <param name="f">Faces array.</param>
		/// <returns></returns>
		public static ProBuilderMesh CreateInstanceWithVerticesFaces(Vector3[] v, Face[] f)
		{
			GameObject go = new GameObject();
			ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
			go.name = "ProBuilder Mesh";
			pb.GeometryWithVerticesFaces(v, f);
			return pb;
		}
#endregion

#region Selection
		/// <summary>
		/// Get a copy of the selected face array.
		/// </summary>
		public Face[] SelectedFaces
		{
			get { return InternalUtility.ValuesWithIndices(this.facesInternal, m_selectedFaces); }
		}

		/// <summary>
		/// Get the number of faces that are currently selected on this object. Faster than checking SelectedFaces.Length.
		/// </summary>
		public int SelectedFaceCount
		{
			get { return m_selectedFaces.Length; }
		}

		/// <summary>
		/// Get the selected vertex indices array.
		/// </summary>
		public int[] SelectedTriangles
		{
			get { return m_selectedTriangles; }
		}

		/// <summary>
		/// Get the count of selected vertex indices.
		/// </summary>
		public int SelectedTriangleCount
		{
			get { return m_selectedTriangles.Length; }
		}

		/// <summary>
		/// Get the selected edges array.
		/// </summary>
		public Edge[] SelectedEdges
		{
			get { return m_SelectedEdges; }
		}

		/// <summary>
		/// Get the count of selected edges.
		/// </summary>
		public int SelectedEdgeCount
		{
			get { return m_SelectedEdges.Length; }
		}

		[SerializeField] int[] m_selectedFaces = new int[] { };
		[SerializeField] Edge[] m_SelectedEdges = new Edge[] { };
		[SerializeField] int[] m_selectedTriangles = new int[] { };

		/// <summary>
		/// Adds a face to this pb_Object's selected array.  Also updates the SelectedEdges and SelectedTriangles arrays.
		/// </summary>
		/// <param name="face"></param>
		internal void AddToFaceSelection(Face face)
		{
			int index = System.Array.IndexOf(this.facesInternal, face);

			if (index > -1)
				SetSelectedFaces(m_selectedFaces.Add(index));
		}

		internal void SetSelectedFaces(IEnumerable<Face> selected)
		{
			List<int> indices = new List<int>();
			foreach (Face f in selected)
			{
				int index = System.Array.IndexOf(this.facesInternal, f);
				if (index > -1)
					indices.Add(index);
			}
			SetSelectedFaces(indices);
		}

		internal void SetSelectedFaces(IEnumerable<int> selected)
		{
			m_selectedFaces = selected.ToArray();
			m_selectedTriangles = m_selectedFaces.SelectMany(x => facesInternal[x].distinctIndices).ToArray();

			// Copy the edges- otherwise Unity's Undo does unholy things to the actual edges reference
			// @todo test this now that pb_Edge is a struct
			Edge[] edges = EdgeExtension.AllEdges(SelectedFaces);
			int len = edges.Length;
			m_SelectedEdges = new Edge[len];
			for (int i = 0; i < len; i++)
				m_SelectedEdges[i] = edges[i];

			if (onElementSelectionChanged != null)
				onElementSelectionChanged(this);
		}

		internal void SetSelectedEdges(IEnumerable<Edge> edges)
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = edges.ToArray();
			m_selectedTriangles = m_SelectedEdges.AllTriangles();

			if (onElementSelectionChanged != null)
				onElementSelectionChanged(this);
		}

		/// <summary>
		/// Sets this pb_Object's SelectedTriangles array. Clears SelectedFaces and SelectedEdges arrays.
		/// </summary>
		/// <param name="tris"></param>
		internal void SetSelectedTriangles(int[] tris)
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new Edge[0];
			m_selectedTriangles = tris != null ? tris.Distinct().ToArray() : new int[0];

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
			int indx = System.Array.IndexOf(this.facesInternal, face);

			if (indx > -1)
				SetSelectedFaces(m_selectedFaces.Remove(indx));
		}

		/// <summary>
		/// Clears SelectedFaces, SelectedEdges, and SelectedTriangle arrays.  You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
		/// </summary>
		internal void ClearSelection()
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new Edge[0];
			m_selectedTriangles = new int[0];
		}

		/// <summary>
		/// Set the internal face array with the passed pb_Face array.
		/// </summary>
		/// <param name="newFaces"></param>
		public void SetFaces(IEnumerable<Face> newFaces)
		{
			m_Faces = newFaces.Where(x => x != null).ToArray();

			if (m_Faces.Length != facesInternal.Count())
				Log.Warning("SetFaces() pruned " + (facesInternal.Count() - m_Faces.Length) + " null faces from this object.");
		}
#endregion

#region Mesh Building
		void GeometryWithPoints(Vector3[] positions)
		{
			// Wrap in faces
			Face[] f = new Face[positions.Length / 4];

			for (int i = 0; i < positions.Length; i += 4)
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
            SetPositions(positions);
			SetFaces(f);
			SetSharedIndices(IntArrayUtility.ExtractSharedIndices(positions));

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Initialize the mesh with vertex positions and faces.
		/// </summary>
		/// <remarks>Rebuilds the sharedIndex array and uniqueIndex array each time called.</remarks>
		/// <param name="vertices">Vertex positions array.</param>
		/// <param name="f">Faces array.</param>
		public void GeometryWithVerticesFaces(Vector3[] vertices, Face[] f)
		{
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Clear();
            SetPositions(vertices);
			SetFaces(f);
			SetSharedIndices(IntArrayUtility.ExtractSharedIndices(vertices));

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Checks if the mesh component is lost or does not match _vertices, and if so attempt to rebuild. returns True if object is okay, false if a rebuild was necessary and you now need to regenerate UV2.
		/// </summary>
		/// <returns></returns>
		public MeshRebuildReason Verify()
		{
			if (mesh == null)
			{
				// attempt reconstruction
				try
				{
					ToMesh();
					Refresh();
				}
				catch (System.Exception e)
				{
					Log.Error("Failed rebuilding null pb_Object. Cached mesh attributes are invalid or missing.\n" + e.ToString());
				}

				return MeshRebuildReason.Null;
			}

			int meshNo;
			int.TryParse(mesh.name.Replace("pb_Mesh", ""), out meshNo);

			if (meshNo != id)
				return MeshRebuildReason.InstanceIDMismatch;

			return mesh.uv2 == null ? MeshRebuildReason.Lightmap : MeshRebuildReason.None;
		}

		/// <summary>
		/// Rebuild the mesh positions, uvs, and submeshes. If vertex count matches new positions array the existing attributes are kept, otherwise the mesh is cleared. UV2 is the exception, it is always cleared.
		/// </summary>
		public void ToMesh()
		{
			ToMesh(MeshTopology.Triangles);
		}

		/// <summary>
		/// Rebuild the mesh positions, uvs, and submeshes. If vertex count matches new positions array the existing attributes are kept, otherwise the mesh is cleared. UV2 is the exception, it is always cleared.
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

			if (m_Textures0 != null)
				m.uv = m_Textures0;

			m.uv2 = null;

			Submesh[] submeshes = Face.GetMeshIndices(facesInternal, preferredTopology);
            m.subMeshCount = submeshes.Length;

			for (int i = 0; i < m.subMeshCount; i++)
#if UNITY_5_5_OR_NEWER
				m.SetIndices(submeshes[i].m_Indices, submeshes[i].m_Topology, i, false);
#else
				m.SetIndices(submeshes[i].indices, submeshes[i].topology, i);
#endif

			m.name = string.Format("pb_Mesh{0}", id);

			GetComponent<MeshFilter>().sharedMesh = m;
#if !PROTOTYPE
			GetComponent<MeshRenderer>().sharedMaterials = submeshes.Select(x => x.m_Material).ToArray();
#endif
		}

		/// <summary>
		/// Deep copy the mesh attribute arrays back to itself. Useful when copy/paste creates duplicate references.
		/// </summary>
		internal void MakeUnique()
		{
			Face[] q = new Face[m_Faces.Length];

			for (int i = 0; i < q.Length; i++)
				q[i] = new Face(m_Faces[i]);

			IntArray[] sv = new IntArray[m_SharedIndices.Length];
			System.Array.Copy(m_SharedIndices, sv, sv.Length);

			SetSharedIndices(sv);
			SetFaces(q);

			Vector3[] v = new Vector3[vertexCount];
			System.Array.Copy(m_Positions, v, vertexCount);
            SetPositions(v);

			if (m_Textures0 != null && m_Textures0.Length == vertexCount)
			{
				Vector2[] u = new Vector2[vertexCount];
				System.Array.Copy(m_Textures0, u, vertexCount);
                m_Textures0 = u;
			}

			mesh = new Mesh();

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Recalculates mesh attributes: normals, collisions, UVs, tangents, and colors.
		/// </summary>
		/// <param name="mask">
		/// Optionally pass a mask to define what components are updated (UV and Collisions are expensive to rebuild, and can usually be deferred til completion of task).
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

		/// <summary>
		/// Rebuild the collider for this mesh.
		/// </summary>
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
						((SphereCollider) c).radius = ProBuilderMath.LargestValue(m.bounds.extents);
					}
					else if (t == typeof(CapsuleCollider))
					{
						((CapsuleCollider) c).center = m.bounds.center;
						Vector2 xy = new Vector2(m.bounds.extents.x, m.bounds.extents.z);
						((CapsuleCollider) c).radius = ProBuilderMath.LargestValue(xy);
						((CapsuleCollider) c).height = m.bounds.size.y;
					}
					else if (t == typeof(WheelCollider))
					{
						((WheelCollider) c).center = m.bounds.center;
						((WheelCollider) c).radius = ProBuilderMath.LargestValue(m.bounds.extents);
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
					nrm = ProBuilderMath.Normal(this, kvp.Value[0]);

				if (kvp.Value[0].uv.useWorldSpace)
					UnwrappingUtility.PlanarMap2(world, newUVs, indices, kvp.Value[0].uv, transform.TransformDirection(nrm));
				else
					UnwrappingUtility.PlanarMap2(positionsInternal, newUVs, indices, kvp.Value[0].uv, nrm);

				// Apply UVs to array, and update the localPivot and localSize caches.
				Vector2 pivot = kvp.Value[0].uv.localPivot;

				foreach (Face f in kvp.Value)
					f.uv.localPivot = pivot;
			}

			m_Textures0 = newUVs;
			mesh.uv = newUVs;

			if (hasUv3) mesh.SetUVs(2, m_Textures3);
			if (hasUv4) mesh.SetUVs(3, m_Textures4);
		}

		/// <summary>
		/// Set mesh UV2.
		/// </summary>
		/// <remarks>
		/// Applies directly to UnityEngine mesh.
		/// </remarks>
		/// <param name="v"></param>
		public void SetUV2(Vector2[] v)
		{
			GetComponent<MeshFilter>().sharedMesh.uv2 = v;
		}

		void RefreshColors()
		{
			Mesh m = GetComponent<MeshFilter>().sharedMesh;

			if (m_Colors == null || m_Colors.Length != vertexCount)
				m_Colors = InternalUtility.FilledArray<Color>(Color.white, vertexCount);

			m.colors = m_Colors;
		}

		/// <summary>
		/// Set a faces vertices to a color.
		/// </summary>
		/// <param name="face"></param>
		/// <param name="color"></param>
		public void SetFaceColor(Face face, Color color)
		{
            if (face == null)
                throw new ArgumentNullException("face");

			if (m_Colors == null)
                m_Colors = InternalUtility.FilledArray<Color>(Color.white, vertexCount);

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
