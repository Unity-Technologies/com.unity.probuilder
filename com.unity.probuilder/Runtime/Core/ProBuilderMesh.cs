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
    public sealed partial class ProBuilderMesh : MonoBehaviour
    {
	    const int k_UVChannelCount = 4;

#region Serialized Fields and Properties
        [SerializeField]
        [FormerlySerializedAs("_quads")]
        Face[] m_Faces;

        [SerializeField]
        [FormerlySerializedAs("_sharedIndices")]
        IntArray[] m_SharedIndexes;

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
        IntArray[] m_SharedIndexesUV;

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
#endregion

#region Properties

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

	    internal IntArray[] sharedIndexesInternal
	    {
		    get { return m_SharedIndexes; }
		    set { m_SharedIndexes = value; }
	    }

	    /// <summary>
	    /// ProBuilder makes the assumption that no @"UnityEngine.ProBuilder.Face" references a vertex used by another. However, we need a way to associate vertexes in the editor for many operations. These vertexes are usually called coincident, or shared vertexes. ProBuilder manages these associations with the sharedIndexes array. Each array contains a list of triangles that point to vertices considered to be coincident. When ProBuilder compiles a UnityEngine.Mesh from the ProBuilderMesh, these vertices will be condensed to a single vertex where possible.
	    /// </summary>
	    /// <value>
	    /// The shared (or common) index array for this mesh.
	    /// </value>
	    /// <seealso cref="SetSharedIndexes(UnityEngine.ProBuilder.IntArray[])"/>
	    public ReadOnlyCollection<IntArray> sharedIndexes
	    {
		    get { return new ReadOnlyCollection<IntArray>(m_SharedIndexes); }
	    }

	    /// <value>
	    /// Get a copy of the shared (or common) index array for this mesh.
	    /// </value>
	    /// <seealso cref="sharedIndexes"/>
	    public IntArray[] GetSharedIndexes()
	    {
		    int len = m_SharedIndexes.Length;
		    IntArray[] copy = new IntArray[len];
		    for(var i = 0; i < len; i++)
			    copy[i] = new IntArray(m_SharedIndexes[i]);
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
		    m_SharedIndexes = new IntArray[len];
		    for (var i = 0; i < len; i++)
			    m_SharedIndexes[i] = new IntArray(indexes[i]);
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
		    m_SharedIndexes = IntArrayUtility.ToIntArray(indexes);
	    }

        internal IntArray[] sharedIndexesUVInternal
        {
            get { return m_SharedIndexesUV; }
            set { m_SharedIndexesUV = value; }
        }

        internal IntArray[] GetSharedIndexesUV()
        {
            int sil = m_SharedIndexesUV.Length;
            IntArray[] sharedIndexesCopy = new IntArray[sil];
            for (var i = 0; i < sil; i++)
                sharedIndexesCopy[i] = m_SharedIndexesUV[i];
            return sharedIndexesCopy;
        }

	    internal void SetSharedIndexesUV(IntArray[] indexes)
	    {
		    int len = indexes == null ? 0 : indexes.Length;
		    m_SharedIndexesUV = new IntArray[len];
		    for (var i = 0; i < len; i++)
			    m_SharedIndexesUV[i] = new IntArray(indexes[i]);
	    }

        internal void SetSharedIndexesUV(IEnumerable<KeyValuePair<int, int>> indexes)
        {
	        if (indexes == null)
		        m_SharedIndexesUV = new IntArray[0];
			else
	            m_SharedIndexesUV = IntArrayUtility.ToIntArray(indexes);
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
        /// <param name="vertexes">The new vertex array.</param>
        /// <param name="applyMesh">An optional parameter that will apply elements to the MeshFilter.sharedMesh. Note that this should only be used when the mesh is in its original state, not optimized (meaning it won't affect triangles which can be modified by Optimize).</param>
        public void SetVertexes(IList<Vertex> vertexes, bool applyMesh = false)
        {
            if (vertexes == null)
                throw new ArgumentNullException("vertexes");

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
            m_Textures3 = uv3;
            m_Textures4 = uv4;

            if (applyMesh)
            {
                Mesh umesh = mesh;

                Vertex first = vertexes[0];

                if (first.HasAttribute(MeshAttributes.Position)) umesh.vertices = position;
                if (first.HasAttribute(MeshAttributes.Color)) umesh.colors = color;
                if (first.HasAttribute(MeshAttributes.UV0)) umesh.uv = uv0;
                if (first.HasAttribute(MeshAttributes.Normal)) umesh.normals = normal;
                if (first.HasAttribute(MeshAttributes.Tangent)) umesh.tangents = tangent;
                if (first.HasAttribute(MeshAttributes.UV1)) umesh.uv2 = uv2;
                if (first.HasAttribute(MeshAttributes.UV2)) if (uv3 != null) umesh.SetUVs(2, uv3);
                if (first.HasAttribute(MeshAttributes.UV3)) if (uv4 != null) umesh.SetUVs(3, uv4);
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
	    public static event Action<ProBuilderMesh> onDestroyObject;

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
#endregion
    }
}
