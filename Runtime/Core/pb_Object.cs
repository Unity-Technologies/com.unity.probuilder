using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// ProBuilder mesh class. Stores all the information necessary to create a UnityEngine.Mesh.
	/// </summary>
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class pb_Object : MonoBehaviour
	{
		[SerializeField] private pb_Face[] _quads;

		[SerializeField] private pb_IntArray[] _sharedIndices;

		[SerializeField] private Vector3[] _vertices;

		[SerializeField] private Vector2[] _uv;

		[SerializeField] private List<Vector4> _uv3;

		[SerializeField] private List<Vector4> _uv4;

		[SerializeField] private Vector4[] _tangents;

		[SerializeField] private pb_IntArray[] _sharedIndicesUV = new pb_IntArray[0];

		[SerializeField] private Color[] _colors;

		/// <summary>
		/// If false, ProBuilder will automatically create and scale colliders.
		/// </summary>
		public bool userCollisions = false;

		/// <summary>
		/// Optional flag - if false the editor will ignore clicks on this object.
		/// </summary>
		public bool isSelectable = true;

		/// <summary>
		/// UV2 generation parameters.
		/// </summary>
		public pb_UnwrapParameters unwrapParameters = new pb_UnwrapParameters();

		/// <summary>
		/// If "Meshes are Assets" feature is enabled, this is used to relate pb_Objects to stored meshes.
		/// </summary>
		internal string asset_guid;

		/// <summary>
		/// If onDestroyObject has a subscriber ProBuilder will invoke it instead of cleaning up unused meshes by itself.
		/// </summary>
		public static event System.Action<pb_Object> onDestroyObject;

		internal static event System.Action<pb_Object> onElementSelectionChanged;

		/// <summary>
		/// Usually when you delete a pb_Object you want to also clean up the mesh asset.
		/// However, there are situations you'd want to keep the mesh around, like when stripping probuilder scripts.
		/// </summary>
		public bool dontDestroyMeshOnDelete = false;

		/// <summary>
		/// Convenience property for getting the mesh from the MeshFilter component.
		/// </summary>
		internal Mesh msh
		{
			get { return GetComponent<MeshFilter>().sharedMesh; }
			set { gameObject.GetComponent<MeshFilter>().sharedMesh = value; }
		}

		/// <summary>
		/// Get a reference to the faces array on this mesh.
		/// </summary>
		public pb_Face[] faces
		{
			get { return _quads; }
		}

		/// <summary>
		/// Get a reference to the shared indices array.
		/// Also called common vertices.
		/// </summary>
		public pb_IntArray[] sharedIndices
		{
			get { return _sharedIndices; }
		}

		/// <summary>
		/// Get a reference to the shared uv indices array.
		/// </summary>
		public pb_IntArray[] sharedIndicesUV
		{
			get { return _sharedIndicesUV; }
		}

		/// <summary>
		/// Get a unique id for this pb_Object. Not guaranteed to be persistent.
		/// </summary>
		public int id
		{
			get { return gameObject.GetInstanceID(); }
		}

		/// <summary>
		/// Get a reference to the positions array.
		/// </summary>
		/// <remarks>
		/// The stored vertex positions array is not guaranteed to match the Unity mesh vertices array.
		/// </remarks>
		public Vector3[] vertices
		{
			get { return _vertices; }
		}

		/// <summary>
		/// Get a reference to the colors array.
		/// </summary>
		public Color[] colors
		{
			get { return _colors; }
		}

		/// <summary>
		/// Get a reference to the last generated UVs for this object.
		/// </summary>
		public Vector2[] uv
		{
			get { return _uv; }
		}

		/// <summary>
		/// True if this mesh has a valid UV2 channel.
		/// </summary>
		public bool hasUv2
		{
			get { return msh.uv2 != null && msh.uv2.Length == vertexCount; }
		}

		/// <summary>
		/// True if this mesh has a valid UV3 channel.
		/// </summary>
		public bool hasUv3
		{
			get { return _uv3 != null && _uv3.Count == vertexCount; }
		}

		/// <summary>
		/// True if this mesh has a valid UV4 channel.
		/// </summary>
		public bool hasUv4
		{
			get { return _uv4 != null && _uv4.Count == vertexCount; }
		}

		/// <summary>
		/// Get the UV3 list for this mesh.
		/// </summary>
		public List<Vector4> uv3
		{
			get { return _uv3; }
		}

		/// <summary>
		/// Get the UV4 list for this mesh.
		/// </summary>
		public List<Vector4> uv4
		{
			get { return _uv4; }
		}

		/// <summary>
		/// How many faces does this mesh have?
		/// </summary>
		public int faceCount
		{
			get { return _quads == null ? 0 : _quads.Length; }
		}

		/// <summary>
		/// How many vertices are in the positions array.
		/// </summary>
		public int vertexCount
		{
			get { return _vertices == null ? 0 : _vertices.Length; }
		}

		/// <summary>
		/// How many triangle indices make up this mesh.
		/// </summary>
		/// <remarks>This calls Linq Sum on the faces array. Cache this value if you're accessing it frequently.</remarks>
		public int triangleCount
		{
			get { return _quads == null ? 0 : _quads.Sum(x => x.indices.Length); }
		}

		/// <summary>
		/// Reset all the attribute arrays on this object.
		/// </summary>
		public void Clear()
		{
			// various editor tools expect faces & vertices to always be valid.
			// ideally we'd null everything here, but that would break a lot of existing code.
			_quads = new pb_Face[0];
			_vertices = new Vector3[0];
			_uv = new Vector2[0];
			_uv3 = null;
			_uv4 = null;
			_tangents = null;
			_sharedIndices = new pb_IntArray[0];
			_sharedIndicesUV = null;
			_colors = null;
			SetSelectedTriangles(null);
		}

		/// <summary>
		/// pb_Object doesn't store normals, so this function either:
		///		1. Copies them from the MeshFilter.sharedMesh (if vertex count matches the pb_Object::vertexCount)
		///		2. Calculates a new set of normals and returns.
		/// </summary>
		/// <returns></returns>
		public Vector3[] GetNormals()
		{
			Vector3[] res = null;

			// If mesh isn't optimized try to return a copy from the compiled mesh
			if (msh.vertexCount == vertexCount)
				res = msh.normals;

			if (res == null || res.Length != vertexCount)
			{
				// todo Write pb_MeshUtility.GenerateNormals that handles smoothing groups to avoid 2 separate calls.
				res = pb_MeshUtility.GenerateNormals(this);
				pb_MeshUtility.SmoothNormals(this, ref res);
			}

			return res;
		}

		/// <summary>
		/// Returns a copy of the sharedIndices array.
		/// </summary>
		/// <returns></returns>
		public pb_IntArray[] GetSharedIndices()
		{
			int sil = _sharedIndices.Length;
			pb_IntArray[] sharedIndicesCopy = new pb_IntArray[sil];
			for (int i = 0; i < sil; i++)
			{
				int[] arr = new int[_sharedIndices[i].Length];
				System.Array.Copy(_sharedIndices[i].array, arr, arr.Length);
				sharedIndicesCopy[i] = new pb_IntArray(arr);
			}

			return sharedIndicesCopy;
		}

		/// <summary>
		/// Returns a copy of the sharedIndicesUV array.
		/// </summary>
		/// <returns></returns>
		public pb_IntArray[] GetSharedIndicesUV()
		{
			int sil = _sharedIndicesUV.Length;
			pb_IntArray[] sharedIndicesCopy = new pb_IntArray[sil];
			for (int i = 0; i < sil; i++)
			{
				int[] arr = new int[_sharedIndicesUV[i].Length];
				System.Array.Copy(_sharedIndicesUV[i].array, arr, arr.Length);
				sharedIndicesCopy[i] = new pb_IntArray(arr);
			}

			return sharedIndicesCopy;
		}

		void Awake()
		{
			if (GetComponent<MeshRenderer>().isPartOfStaticBatch)
				return;

			// Absolutely no idea why normals sometimes go haywire
			Vector3[] normals = msh != null ? msh.normals : null;

			if (normals == null ||
			    normals.Length != msh.vertexCount ||
			    (normals.Length > 0 && normals[0] == Vector3.zero))
			{
				// means this object is probably just now being instantiated
				if (_vertices == null)
					return;

				ToMesh();
				Refresh();
			}
		}

		void OnDestroy()
		{
			// Time.frameCount is zero when loading scenes in the Editor. It's the only way I could figure to
			// differentiate between OnDestroy invoked from user delete & editor scene loading.
			if (!dontDestroyMeshOnDelete &&
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
		/// Duplicates and returns the passed pb_Object.
		/// </summary>
		/// <param name="pb"></param>
		/// <returns></returns>
		public static pb_Object InitWithObject(pb_Object pb)
		{
			Vector3[] v = new Vector3[pb.vertexCount];
			System.Array.Copy(pb.vertices, v, pb.vertexCount);

			Vector2[] u = new Vector2[pb.vertexCount];
			System.Array.Copy(pb.uv, u, pb.vertexCount);

			Color[] c = new Color[pb.vertexCount];
			System.Array.Copy(pb.colors, c, pb.vertexCount);

			pb_Face[] f = new pb_Face[pb.faces.Length];

			for (int i = 0; i < f.Length; i++)
				f[i] = new pb_Face(pb.faces[i]);

			pb_Object p = CreateInstanceWithElements(v, u, c, f, pb.GetSharedIndices(), pb.GetSharedIndicesUV());

			p.gameObject.name = pb.gameObject.name + "-clone";

			return p;
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
		internal static pb_Object CreateInstanceWithPoints(Vector3[] vertices)
		{
			if (vertices.Length % 4 != 0)
			{
				pb_Log.Warning("Invalid Geometry.  Make sure vertices in are pairs of 4 (faces).");
				return null;
			}

			GameObject _gameObject = new GameObject();
			pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
			_gameObject.name = "ProBuilder Mesh";
			pb_obj.GeometryWithPoints(vertices);

			return pb_obj;
		}

		/// <summary>
		/// Creates a new pb_Object with passed vertex positions array and pb_Face array. Allows for a great deal of control when constructing geometry.
		/// </summary>
		/// <param name="v">Vertex positions array.</param>
		/// <param name="f">Faces array.</param>
		/// <returns></returns>
		public static pb_Object CreateInstanceWithVerticesFaces(Vector3[] v, pb_Face[] f)
		{
			GameObject _gameObject = new GameObject();
			pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
			_gameObject.name = "ProBuilder Mesh";
			pb_obj.GeometryWithVerticesFaces(v, f);
			return pb_obj;
		}

		/// <summary>
		/// Creates a new pb_Object instance with the provided vertices, faces, and sharedIndex information.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="u"></param>
		/// <param name="c"></param>
		/// <param name="f"></param>
		/// <param name="si"></param>
		/// <param name="si_uv"></param>
		/// <returns></returns>
		internal static pb_Object CreateInstanceWithElements(Vector3[] v, Vector2[] u, Color[] c, pb_Face[] f,
			pb_IntArray[] si, pb_IntArray[] si_uv)
		{
			GameObject _gameObject = new GameObject();
			pb_Object pb = _gameObject.AddComponent<pb_Object>();

			pb.SetVertices(v);
			pb.SetUV(u);
			pb.SetColors(c);

			pb.SetSharedIndices(si ?? pb_IntArrayUtility.ExtractSharedIndices(v));

			pb.SetSharedIndicesUV(si_uv ?? new pb_IntArray[0] { });

			pb.SetFaces(f);

			pb.ToMesh();
			pb.Refresh();

			return pb;
		}

		/// <summary>
		/// Creates a new pb_Object instance with the provided vertices, faces, and sharedIndex information.
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="faces"></param>
		/// <param name="si">Optional sharedIndices array. If null this value will be generated.</param>
		/// <returns></returns>
		public static pb_Object CreateInstanceWithElements(pb_Vertex[] vertices, pb_Face[] faces, pb_IntArray[] si = null)
		{
			GameObject _gameObject = new GameObject();
			pb_Object pb = _gameObject.AddComponent<pb_Object>();

			Vector3[] position;
			Color[] color;
			Vector2[] uv0;
			Vector3[] normal;
			Vector4[] tangent;
			Vector2[] uv2;
			List<Vector4> uv3;
			List<Vector4> uv4;

			pb_Vertex.GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4);

			pb.SetVertices(position);
			pb.SetColors(color);
			pb.SetUV(uv0);
			if (uv3 != null) pb._uv3 = uv3;
			if (uv4 != null) pb._uv4 = uv4;

			pb.SetSharedIndices(si ?? pb_IntArrayUtility.ExtractSharedIndices(position));
			pb.SetSharedIndicesUV(new pb_IntArray[0] { });

			pb.SetFaces(faces);

			pb.ToMesh();
			pb.Refresh();

			return pb;
		}

		/// <summary>
		/// Get a copy of the selected face array.
		/// </summary>
		public pb_Face[] SelectedFaces
		{
			get { return pb_Util.ValuesWithIndices(this.faces, m_selectedFaces); }
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
		public pb_Edge[] SelectedEdges
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
		[SerializeField] pb_Edge[] m_SelectedEdges = new pb_Edge[] { };
		[SerializeField] int[] m_selectedTriangles = new int[] { };

		/// <summary>
		/// Adds a face to this pb_Object's selected array.  Also updates the SelectedEdges and SelectedTriangles arrays.
		/// </summary>
		/// <param name="face"></param>
		internal void AddToFaceSelection(pb_Face face)
		{
			int index = System.Array.IndexOf(this.faces, face);

			if (index > -1)
				SetSelectedFaces(m_selectedFaces.Add(index));
		}

		internal void SetSelectedFaces(IEnumerable<pb_Face> selected)
		{
			List<int> indices = new List<int>();
			foreach (pb_Face f in selected)
			{
				int index = System.Array.IndexOf(this.faces, f);
				if (index > -1)
					indices.Add(index);
			}
			SetSelectedFaces(indices);
		}

		internal void SetSelectedFaces(IEnumerable<int> selected)
		{
			m_selectedFaces = selected.ToArray();
			m_selectedTriangles = m_selectedFaces.SelectMany(x => faces[x].distinctIndices).ToArray();

			// Copy the edges- otherwise Unity's Undo does unholy things to the actual edges reference
			// @todo test this now that pb_Edge is a struct
			pb_Edge[] edges = pb_EdgeExtension.AllEdges(SelectedFaces);
			int len = edges.Length;
			m_SelectedEdges = new pb_Edge[len];
			for (int i = 0; i < len; i++)
				m_SelectedEdges[i] = edges[i];

			if (onElementSelectionChanged != null)
				onElementSelectionChanged(this);
		}

		internal void SetSelectedEdges(IEnumerable<pb_Edge> edges)
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
			m_SelectedEdges = new pb_Edge[0];
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
		internal void RemoveFromFaceSelection(pb_Face face)
		{
			int indx = System.Array.IndexOf(this.faces, face);

			if (indx > -1)
				SetSelectedFaces(m_selectedFaces.Remove(indx));
		}

		/// <summary>
		/// Clears SelectedFaces, SelectedEdges, and SelectedTriangle arrays.  You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
		/// </summary>
		internal void ClearSelection()
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new pb_Edge[0];
			m_selectedTriangles = new int[0];
		}

		/// <summary>
		/// Sets the internal vertex cache, but does NOT rebuild the mesh.vertices array. Usually you'll want to call ToMesh() immediately following this.
		/// </summary>
		/// <param name="v"></param>
		public void SetVertices(Vector3[] v)
		{
			_vertices = v;
		}

		/// <summary>
		/// Set the vertex element arrays on this pb_Object. By default this function does not apply these values to the mesh.  An optional parameter `applyMesh` will apply elements to the mesh - note that this should only be used when the mesh is in its original state, not optimized (meaning it won't affect triangles which can be modified by Optimize).
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="applyMesh"></param>
		public void SetVertices(IList<pb_Vertex> vertices, bool applyMesh = false)
		{
			Vector3[] position;
			Color[] color;
			Vector2[] uv0;
			Vector3[] normal;
			Vector4[] tangent;
			Vector2[] uv2;
			List<Vector4> uv3;
			List<Vector4> uv4;

			pb_Vertex.GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4);

			SetVertices(position);
			SetColors(color);
			SetUV(uv0);
			if (uv3 != null) _uv3 = uv3;
			if (uv4 != null) _uv4 = uv4;

			if (applyMesh)
			{
				Mesh m = msh;

				pb_Vertex first = vertices[0];

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
		/// Set the UV0 array. Must match vertexCount.
		/// </summary>
		/// <param name="uvs"></param>
		public void SetUV(Vector2[] uvs)
		{
			_uv = uvs;
		}

		/// <summary>
		/// Set the internal face array with the passed pb_Face array.
		/// </summary>
		/// <param name="newFaces"></param>
		public void SetFaces(IEnumerable<pb_Face> newFaces)
		{
			_quads = newFaces.Where(x => x != null).ToArray();

			if (_quads.Length != faces.Count())
				pb_Log.Warning("SetFaces() pruned " + (faces.Count() - _quads.Length) + " null faces from this object.");
		}

		/// <summary>
		/// Sets the internal sharedIndices array reference.
		/// </summary>
		/// <param name="si"></param>
		public void SetSharedIndices(pb_IntArray[] si)
		{
			_sharedIndices = si;
		}

		/// <summary>
		/// Set the sharedIndices array with a dictionary.
		/// </summary>
		/// <param name="si"></param>
		public void SetSharedIndices(IEnumerable<KeyValuePair<int, int>> si)
		{
			_sharedIndices = pb_IntArrayUtility.ToSharedIndices(si);
		}

		internal void SetSharedIndicesUV(pb_IntArray[] si)
		{
			_sharedIndicesUV = si;
		}

		internal void SetSharedIndicesUV(IEnumerable<KeyValuePair<int, int>> si)
		{
			_sharedIndicesUV = pb_IntArrayUtility.ToSharedIndices(si);
		}

		void GeometryWithPoints(Vector3[] v)
		{
			// Wrap in faces
			pb_Face[] f = new pb_Face[v.Length / 4];

			for (int i = 0; i < v.Length; i += 4)
			{
				f[i / 4] = new pb_Face(new int[6]
					{
						i + 0, i + 1, i + 2,
						i + 1, i + 3, i + 2
					},
					pb_Material.DefaultMaterial,
					new pb_UV(),
					0,
					-1,
					-1,
					false);
			}

			SetVertices(v);
			SetUV(new Vector2[v.Length]);
			SetColors(pb_Util.FilledArray<Color>(Color.white, v.Length));

			SetFaces(f);
			SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(v));

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Initialize the mesh with vertex positions and faces.
		/// </summary>
		/// <remarks>Rebuilds the sharedIndex array and uniqueIndex array each time called.</remarks>
		/// <param name="v">Vertex positions array.</param>
		/// <param name="f">Faces array.</param>
		public void GeometryWithVerticesFaces(Vector3[] v, pb_Face[] f)
		{
			SetVertices(v);
			SetUV(new Vector2[v.Length]);

			SetFaces(f);
			SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(v));

			ToMesh();
			Refresh();
		}

		/// <summary>
		/// Checks if the mesh component is lost or does not match _vertices, and if so attempt to rebuild. returns True if object is okay, false if a rebuild was necessary and you now need to regenerate UV2.
		/// </summary>
		/// <returns></returns>
		public MeshRebuildReason Verify()
		{
			if (msh == null)
			{
				// attempt reconstruction
				try
				{
					ToMesh();
					Refresh();
				}
				catch (System.Exception e)
				{
					pb_Log.Error("Failed rebuilding null pb_Object. Cached mesh attributes are invalid or missing.\n" + e.ToString());
				}

				return MeshRebuildReason.Null;
			}

			int meshNo;
			int.TryParse(msh.name.Replace("pb_Mesh", ""), out meshNo);

			if (meshNo != id)
				return MeshRebuildReason.InstanceIDMismatch;

			return msh.uv2 == null ? MeshRebuildReason.Lightmap : MeshRebuildReason.None;
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
			Mesh m = msh;

			// if the mesh vertex count hasn't been modified, we can keep most of the mesh elements around
			if (m != null && m.vertexCount == _vertices.Length)
				m = msh;
			else if (m == null)
				m = new Mesh();
			else
				m.Clear();

			m.vertices = _vertices;

			if (_uv != null)
				m.uv = _uv;

			m.uv2 = null;

			pb_Submesh[] submeshes;

			m.subMeshCount = pb_Face.GetMeshIndices(faces, out submeshes, preferredTopology);

			for (int i = 0; i < m.subMeshCount; i++)
#if UNITY_5_5_OR_NEWER
				m.SetIndices(submeshes[i].indices, submeshes[i].topology, i, false);
#else
				m.SetIndices(submeshes[i].indices, submeshes[i].topology, i);
#endif

			m.name = string.Format("pb_Mesh{0}", id);

			GetComponent<MeshFilter>().sharedMesh = m;
#if !PROTOTYPE
			GetComponent<MeshRenderer>().sharedMaterials = submeshes.Select(x => x.material).ToArray();
#endif
		}

		/// <summary>
		/// Deep copy the mesh attribute arrays back to itself. Useful when copy/paste creates duplicate references.
		/// </summary>
		internal void MakeUnique()
		{
			pb_Face[] q = new pb_Face[_quads.Length];

			for (int i = 0; i < q.Length; i++)
				q[i] = new pb_Face(_quads[i]);

			pb_IntArray[] sv = new pb_IntArray[_sharedIndices.Length];
			System.Array.Copy(_sharedIndices, sv, sv.Length);

			SetSharedIndices(sv);
			SetFaces(q);

			Vector3[] v = new Vector3[vertexCount];
			System.Array.Copy(_vertices, v, vertexCount);
			SetVertices(v);

			if (_uv != null && _uv.Length == vertexCount)
			{
				Vector2[] u = new Vector2[vertexCount];
				System.Array.Copy(_uv, u, vertexCount);
				SetUV(u);
			}

			msh = new Mesh();

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
			Mesh m = msh;

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
						((SphereCollider) c).radius = pb_Math.LargestValue(m.bounds.extents);
					}
					else if (t == typeof(CapsuleCollider))
					{
						((CapsuleCollider) c).center = m.bounds.center;
						Vector2 xy = new Vector2(m.bounds.extents.x, m.bounds.extents.z);
						((CapsuleCollider) c).radius = pb_Math.LargestValue(xy);
						((CapsuleCollider) c).height = m.bounds.size.y;
					}
					else if (t == typeof(WheelCollider))
					{
						((WheelCollider) c).center = m.bounds.center;
						((WheelCollider) c).radius = pb_Math.LargestValue(m.bounds.extents);
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
			while (System.Array.Exists(faces, element => element.textureGroup == i))
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
			while (System.Array.Exists(faces, element => element.elementGroup == i))
				i++;

			return i;
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
			uvs.Clear();

			switch (channel)
			{
				case 0:
				default:
					for (int i = 0; i < vertexCount; i++)
						uvs.Add((Vector4) _uv[i]);
					break;

				case 1:
					if (msh != null && msh.uv2 != null)
					{
						Vector2[] uv2 = msh.uv2;
						for (int i = 0; i < uv2.Length; i++)
							uvs.Add((Vector4) uv2[i]);
					}
					break;

				case 2:
					if (_uv3 != null)
						uvs.AddRange(_uv3);
					break;

				case 3:
					if (_uv4 != null)
						uvs.AddRange(_uv4);
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
					msh.uv2 = uvs.Cast<Vector2>().ToArray();
					break;

				case 2:
					_uv3 = uvs;
					break;

				case 3:
					_uv4 = uvs;
					break;

				case 0:
				default:
					_uv = uvs.Cast<Vector2>().ToArray();
					break;
			}
		}

		/// <summary>
		/// Re-project AutoUV faces and re-assign ManualUV to mesh.uv channel.
		/// </summary>
		void RefreshUV()
		{
			RefreshUV(faces);
		}

		/// <summary>
		/// Re-project AutoUV faces and re-assign ManualUV to mesh.uv channel.
		/// </summary>
		/// <param name="facesToRefresh"></param>
		internal void RefreshUV(IEnumerable<pb_Face> facesToRefresh)
		{
			Vector2[] oldUvs = msh.uv;
			Vector2[] newUVs;

			// thanks to the upgrade path, this is necessary.  maybe someday remove it.
			if (_uv != null && _uv.Length == vertexCount)
			{
				newUVs = _uv;
			}
			else
			{
				if (oldUvs != null && oldUvs.Length == vertexCount)
				{
					newUVs = oldUvs;
				}
				else
				{
					foreach (pb_Face f in this.faces)
						f.manualUV = false;

					// this necessitates rebuilding ALL the face uvs, so make sure we do that.
					facesToRefresh = this.faces;

					newUVs = new Vector2[vertexCount];
				}
			}

			int n = -2;
			Dictionary<int, List<pb_Face>> tex_groups = new Dictionary<int, List<pb_Face>>();
			bool anyWorldSpace = false;
			List<pb_Face> group;

			foreach (pb_Face f in facesToRefresh)
			{
				if (f.uv.useWorldSpace)
					anyWorldSpace = true;

				if (f == null || f.manualUV)
					continue;

				if (f.textureGroup > 0 && tex_groups.TryGetValue(f.textureGroup, out group))
					group.Add(f);
				else
					tex_groups.Add(f.textureGroup > 0 ? f.textureGroup : n--, new List<pb_Face>() {f});
			}

			// Add any non-selected faces in texture groups to the update list
			if (this.faces.Length != facesToRefresh.Count())
			{
				foreach (pb_Face f in this.faces)
				{
					if (f.manualUV)
						continue;

					if (tex_groups.ContainsKey(f.textureGroup) && !tex_groups[f.textureGroup].Contains(f))
						tex_groups[f.textureGroup].Add(f);
				}
			}

			n = 0;

			Vector3[] world = anyWorldSpace ? transform.ToWorldSpace(vertices) : null;

			foreach (KeyValuePair<int, List<pb_Face>> kvp in tex_groups)
			{
				Vector3 nrm;
				int[] indices = kvp.Value.SelectMany(x => x.distinctIndices).ToArray();

				if (kvp.Value.Count > 1)
					nrm = pb_Projection.FindBestPlane(_vertices, indices).normal;
				else
					nrm = pb_Math.Normal(this, kvp.Value[0]);

				if (kvp.Value[0].uv.useWorldSpace)
					pb_UVUtility.PlanarMap2(world, newUVs, indices, kvp.Value[0].uv, transform.TransformDirection(nrm));
				else
					pb_UVUtility.PlanarMap2(vertices, newUVs, indices, kvp.Value[0].uv, nrm);

				// Apply UVs to array, and update the localPivot and localSize caches.
				Vector2 pivot = kvp.Value[0].uv.localPivot;

				foreach (pb_Face f in kvp.Value)
					f.uv.localPivot = pivot;
			}

			_uv = newUVs;
			msh.uv = newUVs;

#if UNITY_5_3_OR_NEWER
			if (hasUv3) msh.SetUVs(2, uv3);
			if (hasUv4) msh.SetUVs(3, uv4);
#endif
		}

		/// <summary>
		/// Set the material on all faces. Call ToMesh() and Refresh() after to force these changes to take effect.
		/// </summary>
		/// <param name="facesToApply"></param>
		/// <param name="mat"></param>
		public void SetFaceMaterial(pb_Face[] facesToApply, Material mat)
		{
#if PROTOTYPE
			GetComponent<MeshRenderer>().sharedMaterials = new Material[1] { mat };
	#else
			for (int i = 0; i < facesToApply.Length; i++)
				facesToApply[i].material = mat;
#endif
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

			if (_colors == null || _colors.Length != vertexCount)
				_colors = pb_Util.FilledArray<Color>(Color.white, vertexCount);

			m.colors = _colors;
		}

		/// <summary>
		/// Set the internal color array.
		/// </summary>
		/// <param name="InColors"></param>
		public void SetColors(Color[] InColors)
		{
			_colors = InColors.Length == vertexCount ? InColors : pb_Util.FilledArray<Color>(Color.white, vertexCount);
		}

		/// <summary>
		/// Set a faces vertices to a color.
		/// </summary>
		/// <param name="face"></param>
		/// <param name="color"></param>
		public void SetFaceColor(pb_Face face, Color color)
		{
			if (_colors == null) _colors = pb_Util.FilledArray<Color>(Color.white, vertexCount);

			foreach (int i in face.distinctIndices)
				_colors[i] = color;
		}

		/// <summary>
		/// Set the tangent array on this mesh.
		/// </summary>
		/// <param name="tangents"></param>
		public void SetTangents(Vector4[] tangents)
		{
			_tangents = tangents;
		}

		void RefreshNormals()
		{
			msh.RecalculateNormals();
			Vector3[] normals = msh.normals;
			pb_MeshUtility.SmoothNormals(this, ref normals);
			GetComponent<MeshFilter>().sharedMesh.normals = normals;
		}

		void RefreshTangents()
		{
			Mesh m = GetComponent<MeshFilter>().sharedMesh;

			if (_tangents != null && _tangents.Length == vertexCount)
				m.tangents = _tangents;
			else
				pb_MeshUtility.GenerateTangent(ref m);
		}
	}
}
