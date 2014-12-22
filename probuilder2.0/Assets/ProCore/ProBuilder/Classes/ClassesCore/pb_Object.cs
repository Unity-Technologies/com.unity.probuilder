/*
 *	ProBuilder Object
 *	@Karl Henkel, @Gabriel Williams
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProBuilder2.Common;
using ProBuilder2.Math;

#if PB_DEBUG
using Parabox.Debug;
#endif

[AddComponentMenu("")]	// Don't let the user add this to any object.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(pb_Entity))]

/**
 *	\brief Object class for all ProBuilder geometry.
 */	
public class pb_Object : MonoBehaviour
{
#region MONOBEHAVIOUR

	void Start()
	{
		if(GetComponent<MeshRenderer>().isPartOfStaticBatch)
			return;

		if(msh == null)
		{
			ReconstructMesh();
		}
		else
		{
			// No clue why this happens
			if(msh.normals[0] == Vector3.zero)
			{
				Refresh();		
			}
		}
	}
#endregion

#region INITIALIZATION

	/**
	 *	\brief Duplicates and returns the passed pb_Object.
	 *	@param pb The pb_Object to duplicate.
	 *	\returns A unique copy of the passed pb_Object.
	 */
	public static pb_Object InitWithObject(pb_Object pb)
	{
		Vector3[] v = new Vector3[pb.vertexCount];
		System.Array.Copy(pb.vertices, v, pb.vertexCount);
		
		Vector2[] u = new Vector2[pb.vertexCount];
		System.Array.Copy(pb.uv, u, pb.vertexCount);

		pb_Face[] f = new pb_Face[pb.faces.Length];
		
		for(int i = 0; i < f.Length; i++)
			f[i] = new pb_Face(pb.faces[i]);

		pb_Object p = CreateInstanceWithElements(v, u, f, pb.GetSharedIndices(), pb.GetSharedIndicesUV());

		p.gameObject.name = pb.gameObject.name + "-clone";

		return p;
	}

	/**
	 * Since MonoBehaviour objects cannot be serialized, use @c pb_SerializableObject to store mesh and
	 * transform data.  This is the constructor for a serialized pb_Object - it retains all information
	 * necessary to reconstruct a pb_Object as well as transform data (position, local rotation, local
	 * scale).
	 */
	public static pb_Object InitWithSerializableObject(pb_SerializableObject serialized)
	{
		pb_Object pb = CreateInstanceWithElements(
			serialized.vertices,
			serialized.uv,
			serialized.faces,
			serialized.sharedIndices.ToPbIntArray(),
			serialized.sharedIndicesUV.ToPbIntArray());

		pb.transform.position 		= serialized.t_position;
		pb.transform.localRotation 	= serialized.t_rotation;
		pb.transform.localScale 	= serialized.t_scale;

		return pb;
	}

	/**
	 *	\brief Creates a new #pb_Object using passed vertices to construct geometry.
	 *	Typically you would not call this directly, as the #ProBuilder class contains
	 *	a wrapper for this purpose.  In fact, I'm not sure why this is public...
	 *	@param vertices A vertex array (Vector3[]) containing the points to be used in 
	 *	the construction of the #pb_Object.  Vertices must be wound in counter-clockise
	 *	order.  Triangles will be wound in vertex groups of 4, with the winding order
	 *	0,1,2 1,3,2.  Ex: 
	 *	\code{.cs}
	 *	// Creates a pb_Object plane
	 *	pb_Object.CreateInstanceWithPoints(new Vector3[4]{
	 *		new Vector3(-.5f, -.5f, 0f),
	 *		new Vector3(.5f, -.5f, 0f),
	 *		new Vector3(-.5f, .5f, 0f),
	 *		new Vector3(.5f, .5f, 0f)
	 *		});
	 *
	 *	\endcode
	 *	\returns The resulting #pb_Object.
	 */
	public static pb_Object CreateInstanceWithPoints(Vector3[] vertices)
	{
		if(vertices.Length % 4 != 0) {
			Debug.LogWarning("Invalid Geometry.  Make sure vertices in are pairs of 4 (faces).");
			return null;
		}
			
		GameObject _gameObject = new GameObject();	
		pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
		pb_obj.SetName("Object");

		pb_obj.GeometryWithPoints(vertices);

		pb_obj.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);

		return pb_obj;
	}

	/**
	 *	\brief Creates a new pb_Object with passed vertex array and pb_Face array.  Allows for a great deal of control when constructing geometry.
	 *	@param _vertices The vertex array to use in construction of mesh.
	 *	@param _faces A pb_Face array containing triangle, material per face, and pb_UV parameters for each face.
	 *	\sa pb_Face pb_UV
	 *	\returns The newly created pb_Object.
	 */

	///< @todo REMOVE ME
	public static pb_Object CreateInstanceWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		GameObject _gameObject = new GameObject();	
		pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
		pb_obj.SetName("Object");

		pb_obj.GeometryWithVerticesFaces(v, f);


		return pb_obj;
	}

	/**
	 * Creates a new pb_Object instance with the provided vertices, faces, and sharedIndex information.
	 */
	public static pb_Object CreateInstanceWithElements(Vector3[] v, Vector2[] u, pb_Face[] f, pb_IntArray[] si, pb_IntArray[] si_uv)
	{
		GameObject _gameObject = new GameObject();
		pb_Object pb = _gameObject.AddComponent<pb_Object>();

		pb.SetVertices(v);
		pb.SetUV(u);

		pb.SetSharedIndices( si ?? pb_IntArrayUtility.ExtractSharedIndices(v) );

		pb.SetSharedIndicesUV( si_uv ?? new pb_IntArray[0] {});

		pb.SetFaces(f);

		pb.ToMesh();
		pb.Refresh();

		pb.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);

		return pb;
	}
#endregion

#region INTERNAL MEMBERS

	[SerializeField]
	private pb_Face[]		 			_quads;
	private pb_Face[]					_faces { get { return _quads; } }

	[SerializeField]
	private pb_IntArray[] 				_sharedIndices;

	[SerializeField]
	private Vector3[] 					_vertices;

	[SerializeField]
	private Vector2[] 					_uv;

	[SerializeField]
	private pb_IntArray[] 				_sharedIndicesUV = new pb_IntArray[0];

	[SerializeField]
	private int[] 						_submeshTriangleCount;	///< Used to detect changes to the material array

	[SerializeField]
	private Color[] 					_colors;

	// UV2 generation params
	public float angleError = 8f;
	public float areaError = 15f;
	public float hardAngle = 88f;
	public float packMargin = 20f;
	
	public pb_Face[]					SelectedFaces { get { return pbUtil.ValuesWithIndices(this.faces, m_selectedFaces); } }
	public int 							SelectedFaceCount { get { return m_selectedFaces.Length; } }
	public int[]						SelectedFaceIndices { get { return m_selectedFaces; } }
	public int[]						SelectedTriangles { get { return m_selectedTriangles; } }
	public int 							SelectedTriangleCount { get { return m_selectedTriangles.Length; } }
	public pb_Edge[]					SelectedEdges { get { return m_SelectedEdges; } }

	[SerializeField] private int[]		m_selectedFaces 		= new int[]{};
	[SerializeField] private pb_Edge[]	m_SelectedEdges 		= new pb_Edge[]{};
	[SerializeField] private int[]		m_selectedTriangles 	= new int[]{};

	public Vector3 						previousTransform = new Vector3(0f, 0f, 0f);
	public bool 						userCollisions = false;	///< If false, ProBuilder will automatically create and scale colliders.

	public bool 						isSelectable = true;	///< Optional flag - if true editor should ignore clicks on this object.

	[SerializeField]
	private string _name = "Object";
#endregion

#region ACCESS
	
	public Mesh msh
	{
		get
		{
			return GetComponent<MeshFilter>().sharedMesh;
		}
		set 
		{
			gameObject.GetComponent<MeshFilter>().sharedMesh = value;
		}
	}

	public pb_Face[] faces { get { return _quads; } }// == null ? Extractfaces(msh) : _faces; } }
	public pb_Face[] quads {get { Debug.LogWarning("pb_Quad is deprecated.  Please use pb_Face instead."); return _quads; } }

	public pb_IntArray[] 	sharedIndices { get { return _sharedIndices; } }	// returns a reference
	public pb_IntArray[] 	sharedIndicesUV { get { return _sharedIndicesUV; } } 

	public int id { get { return gameObject.GetInstanceID(); } }

	public Vector3[] vertices { get { return _vertices; } }
	public Vector2[] uv { get { return _uv; } }
	public Color[] colors { get { return _colors; } }

	public int faceCount { get { return _faces.Length; } }
	public int vertexCount { get { return _vertices.Length; } }

	/**
	 * \brief Returns the material property of the specified #pb_Face. 
	 * \returns Returns the material property of the specified #pb_Face. 
	 * @param face The face to extract material data from.
	 */
	public Material GetMaterial(pb_Face face)
	{
		return face.material;
	}

	/**
	 *	\brief Returns a copy of the sharedIndices array.
	 */
	public pb_IntArray[] GetSharedIndices()
	{
		int sil = _sharedIndices.Length;
		pb_IntArray[] sharedIndicesCopy = new pb_IntArray[sil];
		for(int i = 0; i < sil; i++)
		{
			int[] arr = new int[_sharedIndices[i].Length];
			System.Array.Copy(_sharedIndices[i].array, arr, arr.Length);
			sharedIndicesCopy[i] = new pb_IntArray(arr);
		}

		return sharedIndicesCopy;
	}

	/**
	 *	\brief Returns a copy of the sharedIndicesUV array.
	 */
	public pb_IntArray[] GetSharedIndicesUV()
	{
		int sil = _sharedIndicesUV.Length;
		pb_IntArray[] sharedIndicesCopy = new pb_IntArray[sil];
		for(int i = 0; i < sil; i++)
		{
			int[] arr = new int[_sharedIndicesUV[i].Length];
			System.Array.Copy(_sharedIndicesUV[i].array, arr, arr.Length);
			sharedIndicesCopy[i] = new pb_IntArray(arr);
		}

		return sharedIndicesCopy;
	}
#endregion

#region SELECTION

	/**
	 *	Adds a face to this pb_Object's selected array.  Also updates the SelectedEdges and SelectedTriangles arrays.
	 */
	public void AddToFaceSelection(int indx)
	{
		SetSelectedFaces(m_selectedFaces.Add(indx));
	}

	/**
	 *	Sets this pb_Object's SelectedFaces array, as well as SelectedEdges and SelectedTriangles.
	 */
	public void SetSelectedFaces(pb_Face[] faces)
	{
		int len = faces.Length;
		int[] ind = new int[len];

		for(int i = 0; i < len; i++)
			ind[i] = System.Array.IndexOf(this.faces, faces[i]);

		SetSelectedFaces(ind);
	}

	public void SetSelectedFaces(int[] t_faces)
	{
		this.m_selectedFaces = t_faces;
		this.m_selectedTriangles = pb_Face.AllTriangles( SelectedFaces );
	
		// Copy the edges- otherwise Unity's Undo does unholy things to the actual edges reference		
		pb_Edge[] edges = pb_Edge.AllEdges(SelectedFaces);
		int len = edges.Length;
		this.m_SelectedEdges = new pb_Edge[len];
		for(int i = 0; i < len; i++)
			this.m_SelectedEdges[i] = new pb_Edge(edges[i]);
	}

	public void SetSelectedEdges(pb_Edge[] edges)
	{
		this.m_selectedFaces = new int[0];

		int len = edges.Length;
		this.m_SelectedEdges = new pb_Edge[len];
		for(int i = 0; i < len; i++)
			this.m_SelectedEdges[i] = new pb_Edge(edges[i]);

		this.m_selectedTriangles = m_SelectedEdges.AllTriangles();				
	}

	/**
	 *	Sets this pb_Object's SelectedTriangles array.  Clears SelectedFaces and SelectedEdges arrays.
	 */
	public void SetSelectedTriangles(int[] tris)
	{
		m_selectedFaces = new int[0];
		m_SelectedEdges = new pb_Edge[0];
		m_selectedTriangles = tris;
	}

	/**
	 *	Removes face at index in SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
	 */
	public void RemoveFromFaceSelectionAtIndex(int index)
	{
		SetSelectedFaces(m_selectedFaces.RemoveAt(index));
	}

	/**
	 *	Removes face from SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
	 */
	public void RemoveFromFaceSelection(pb_Face face)
	{		
		int indx = System.Array.IndexOf(this.faces, face);
	
		if(indx > -1)
			SetSelectedFaces(m_selectedFaces.RemoveAt(indx));
	}

	/**
	 *	Clears SelectedFaces, SelectedEdges, and SelectedTriangle arrays.  You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
	 */
	public void ClearSelection()
	{
		m_selectedFaces = new int[0];
		m_SelectedEdges = new pb_Edge[0];
		m_selectedTriangles = new int[0];
	}
#endregion

#region SET

	/**
	 *	\brief Sets the #pb_Object name that is shown in the hierarchy.
	 *	@param __name The name to apply.  Format is pb-Name[#pb_EntityType]-id
	 *	\sa RefreshName
	 */
	public void SetName(string __name)
	{
		_name = __name;
		gameObject.name = "pb-" + _name + id;
	}

	/**
	 * Sets the internal vertex cache, but does NOT rebuild the mesh.vertices array.
	 * Usually you'll want to call ToMesh() immediately following this.
	 */
	public void SetVertices(Vector3[] v)
	{
		_vertices = v;
	}

	/**
	 * Must match size of vertex array.
	 */
	public void SetUV(Vector2[] uvs)
	{
		_uv = uvs;
	}

	/**
	 *	\brief Set the internal face array with the passed pb_Face array.
	 *	@param faces New pb_Face[] containing face data.  Mesh triangle data is extracted from the internal #pb_Face array, so be sure to account for all triangles.
	 */
	public void SetFaces(pb_Face[] _qds)
	{
		_quads = _qds;
	}

	/**
	 * Sets the internal sharedIndices cache.  Also takes care of refreshing the uniqueIndices cache for you.
	 */
	public void SetSharedIndices(pb_IntArray[] si)
	{
		_sharedIndices = si;
	}

	public void SetSharedIndicesUV(pb_IntArray[] si)
	{
		_sharedIndicesUV = si;
	}
#endregion

#region MESH INITIALIZATION

	private void GeometryWithPoints(Vector3[] v)
	{
		// Wrap in faces
		pb_Face[] f = new pb_Face[v.Length/4];

		for(int i = 0; i < v.Length; i+=4)
		{
			f[i/4] = new pb_Face(new int[6]
				{
					i+0, i+1, i+2,
					i+1, i+3, i+2
				},
				pb_Constant.DefaultMaterial,
				new pb_UV(),
				0,
				-1,
				-1,
				false);
		}

		SetVertices(v);
		SetUV(new Vector2[v.Length]);

		SetFaces(f);
	 	SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(v));

		ToMesh();
		Refresh();
	}

	/**
	 *	\brief Rebuilds the sharedIndex array and uniqueIndex array each time
	 *	called.
	 */
	public void GeometryWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		SetVertices(v);
		SetUV(new Vector2[v.Length]);

		SetFaces(f);
		SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(v));

		ToMesh();
		Refresh();
	}

	private void GeometryWithVerticesFacesIndices(Vector3[] v, pb_Face[] f, pb_IntArray[] s)
	{
		SetFaces(f);
		SetVertices(v);
		SetUV(new Vector2[v.Length]);

		SetSharedIndices(s);

		if(msh != null) DestroyImmediate(msh);

		// ToMesh builds the actual Mesh object
		ToMesh();
		// Refresh builds out the UV, Normal, Tangents, etc.
		Refresh();
	}

	/**
	 * Wraps a ToMesh() and Refresh() call and returns true/false on reconstruct success.
	 */
	private bool ReconstructMesh()
	{
		if(msh != null)
			DestroyImmediate(msh);

		if(_vertices == null)
		{
			msh = null;
			return false;
		}

		ToMesh();
		Refresh();

		return true;
	}
#endregion

#region INTERNAL BUILDING STUFF

	/**
	 *	\brief Gets all vertices in local space from face.
	 *	@param _face The #pb_Face to draw vertices from.
	 *	\returns A Vector3[] array containing all vertices contained within a #pb_Face.
	 */
	public Vector3[] GetVertices(pb_Face face)
	{
		Vector3[] v = new Vector3[face.indices.Length];
		for(int i = 0; i < face.indices.Length; i++)
			v[i] = vertices[face.indices[i]];
		
		return v;
	}

	/**
	 * \brief Gets vertex normals for the selected face. 
	 * @param face
	 * \returns Vector3[] containing all normals for a face.
	 */
	public Vector3[] GetNormals(pb_Face face)
	{
		// muhahaha
		Vector3[] normals = msh.normals;
		Vector3[] v = new Vector3[face.indices.Length];
		for(int i = 0; i < face.indices.Length; i++)
			v[i] = normals[face.indices[i]];
		
		return v;
	}

	/**
	 *	\brief Returns vertices in local space.
	 *	\returns Vector3[] Vertices for passed indices in local space.
	 */
	public Vector3[] GetVertices(int[] indices)
	{
		Vector3[] v = new Vector3[indices.Length];
		
		for(int i = 0; i < v.Length; i++)
			v[i] = _vertices[indices[i]];

		return v;
	}

	/**
	 * \brief Returns an array of UV coordinates.
	 */
	public Vector2[] GetUVs(int[] indices)
	{
		Vector2[] uv = new Vector2[indices.Length];
		for(int i = 0; i < uv.Length; i++)
			uv[i] = _uv[indices[i]];
		return uv;
	}

	/**
	 * Get vertices at x,y index with edge.
	 */
	public Vector3[] GetVertices(pb_Edge edge)
	{
		return new Vector3[]
		{
			_vertices[edge.x],
			_vertices[edge.y]
		};
	}

	/**
	 *	\brief Returns vertices in local space.
	 *	\returns List<Vector3> Vertices for passed indices in local space.
	 */
	public List<Vector3> GetVertices(List<int> indices)
	{
		List<Vector3> v = new List<Vector3>(indices.Count);
		
		for(int i = 0; i < indices.Count; i++)
			v.Add( _vertices[indices[i]] );

		return v;
	}
#endregion

#region MESH CONSTRUCTION

	/**
	 *	\brief Force regenerate geometry.  Also responsible for sorting faces with shared materials into the same submeshes.
	 */
	public void ToMesh()
	{
		// dont clear the mesh, cause we want to save everything except triangle data.  Unless it's null, then init stuff
		Mesh m;
		if(msh != null)
		{
			m = msh;
			m.triangles = null;
			m.vertices = _vertices;
			if(_uv != null) m.uv = _uv; // we're upgrading from a release that didn't cache UVs probably (anything 2.2.5 or lower)
		}
		else
		{
			m = new Mesh();
	
			m.vertices = _vertices;
			m.uv = new Vector2[vertexCount];
			m.tangents = new Vector4[vertexCount];
			m.normals = new Vector3[vertexCount];
		}

		// Sort the faces into groups of like materials
		Dictionary<Material, List<pb_Face>> matDic = new Dictionary<Material, List<pb_Face>>();
		
		#if PROTOTYPE
			MeshRenderer mr = GetComponent<MeshRenderer>();
			matDic.Add(mr.sharedMaterial == null ? pb_Constant.DefaultMaterial : mr.sharedMaterial, new List<pb_Face>(this.faces));
		#else
			foreach(pb_Face quad in faces)
			{
				Material face_mat = quad.material ?? pb_Constant.UnityDefaultDiffuse;

				if(matDic.ContainsKey(face_mat))
				{
					matDic[face_mat].Add(quad);
				}
				else
				{
					matDic.Add(face_mat, new List<pb_Face>(1) {quad} );
				}
			}
		#endif

		Material[] mats = new Material[matDic.Count];

		m.subMeshCount = matDic.Count;
		_submeshTriangleCount = new int[matDic.Count];

		int i = 0;
		foreach( KeyValuePair<Material, List<pb_Face>> kvp in matDic )
		{
			m.SetTriangles(pb_Face.AllTriangles(kvp.Value), i);
			_submeshTriangleCount[i] = m.GetTriangles(i).Length;

			mats[i] = kvp.Key;
			i++;
		}

		m.RecalculateBounds();
		m.Optimize();
		m.name = "pb_Mesh" + id;

		GetComponent<MeshFilter>().sharedMesh = m;
		GetComponent<MeshRenderer>().sharedMaterials = mats;
	}

	/**
	 *	\brief Call this to ensure that the mesh is unique.  Basically performs a DeepCopy and assigns back to self.
	 */
	public void MakeUnique()
	{
		pb_Face[] q = new pb_Face[_faces.Length];

		for(int i = 0; i < q.Length; i++)
			q[i] = new pb_Face(_faces[i]);

		pb_IntArray[] sv = new pb_IntArray[_sharedIndices.Length];
		System.Array.Copy(_sharedIndices, sv, sv.Length);
		
		SetSharedIndices(sv);
		SetFaces(q);

		Vector3[] v = new Vector3[vertexCount];
		System.Array.Copy(_vertices, v, vertexCount);
		SetVertices(v);

		Vector2[] u = new Vector2[vertexCount];
		System.Array.Copy(_uv, u, vertexCount);
		SetUV(u);

		msh = pbUtil.DeepCopyMesh(msh);

		ToMesh();
		Refresh();
	}

	/**
	 *	\brief Recalculates standard mesh properties - normals, bounds, collisions, UVs, tangents, and colors.
	 */
	public void Refresh()
	{	
		// Mesh
		RefreshNormals();

		msh.RecalculateBounds();
		
		if(!userCollisions && GetComponent<Collider>())
		{
			foreach(Collider c in gameObject.GetComponents<Collider>())
			{
				System.Type t = c.GetType();

				if(t == typeof(BoxCollider))
				{
					((BoxCollider)c).center = msh.bounds.center;
					((BoxCollider)c).size = msh.bounds.size;
				} else
				if(t == typeof(SphereCollider))
				{
					((SphereCollider)c).center = msh.bounds.center;
					((SphereCollider)c).radius = pb_Math.LargestValue(msh.bounds.extents);
				} else
				if(t == typeof(CapsuleCollider))
				{
					((CapsuleCollider)c).center = msh.bounds.center;
					Vector2 xy = new Vector2(msh.bounds.extents.x, msh.bounds.extents.z);
					((CapsuleCollider)c).radius = pb_Math.LargestValue(xy);
					((CapsuleCollider)c).height = msh.bounds.size.y;
				} else
				if(t == typeof(WheelCollider))
				{
					((WheelCollider)c).center = msh.bounds.center;
					((WheelCollider)c).radius = pb_Math.LargestValue(msh.bounds.extents);
				} else
				if(t == typeof(MeshCollider))
				{
					gameObject.GetComponent<MeshCollider>().sharedMesh = null;	// this is stupid.
					gameObject.GetComponent<MeshCollider>().sharedMesh = msh;
				} 
			}
		}

		// msh.Optimize();
		RefreshColor();

		RefreshUV();

		RefreshTangent();

		RefreshColor();
	}	
#endregion

#region UV

	/**
	 *	Returns a new unused texture group id.
	 */
	public int UnusedTextureGroup(int i)
	{
		int[] used = new int[faces.Length];
		for(int j = 0; j < faces.Length; j++)	
			used[j] = faces[j].textureGroup;
		while(System.Array.IndexOf(used, i) > -1)
			i++;
		return i;
	}

	/**
	 * Returns a new unused element group.   Will be greater than or equal to i.
	 */
	public int UnusedElementGroup(int i)
	{
		while( System.Array.Exists(faces, element => element.elementGroup == i) )
			i++;
		
		return i;
	}

	public int UnusedTextureGroup()
	{
		int i = 1;
	
		int[] used = new int[faces.Length];
		for(int j = 0; j < faces.Length; j++)	
			used[j] = faces[j].textureGroup;
	
		while(System.Array.IndexOf(used, i) > -1)
			i++;

		return i;
	}

	/**
	 * Re-project AutoUV faces and re-assign ManualUV to mesh.uv channel.
	 */
	public void RefreshUV()
	{
		RefreshUV(faces);
	}

	/**
	 * Re-project AutoUV faces and re-assign ManualUV to mesh.uv channel.
	 */
	public void RefreshUV(pb_Face[] faces)
	{
		Dictionary<int, List<pb_Face>> tex_groups = new Dictionary<int, List<pb_Face>>();
		Vector2[] newUVs;

		// thanks to the upgrade path, this is necessary.  maybe someday remove it.
		if(_uv != null && _uv.Length == vertexCount)
		{
			newUVs = _uv;
		}
		else
		{
			if(msh.uv != null && msh.uv.Length == vertexCount)
			{
				newUVs = msh.uv;
			}
			else
			{
				Debug.Log("Big UV kerfuffle.  Resetting all faces to auto projection");
				
				// awwww snap - now we've gon' and lost any hand done uv modifications.  sorry bra.
				foreach(pb_Face f in this.faces)
					f.manualUV = false;

				// this also necessitates rebuilding ALL the face uvs, so make sure we do that.
				faces = this.faces;

				newUVs = new Vector2[vertexCount];
			}
		}

		int n = -2;
		foreach(pb_Face f in faces)
		{
			if(f.manualUV) 
				continue;

			if(f.textureGroup > 0 && tex_groups.ContainsKey(f.textureGroup))
				tex_groups[f.textureGroup].Add(f);
			else
				tex_groups.Add( f.textureGroup > 0 ? f.textureGroup : n--, new List<pb_Face>(1) { f });
		}

		// Add any non-selected faces in texture groups to the update list
		if(this.faces.Length != faces.Length)
		{
			foreach(pb_Face f in this.faces)
			{
				if(f.manualUV) continue;
				if(tex_groups.ContainsKey(f.textureGroup) && !tex_groups[f.textureGroup].Contains(f))
					tex_groups[f.textureGroup].Add(f);
			}
		}

		n = 0;
		foreach(KeyValuePair<int, List<pb_Face>> kvp in tex_groups)
		{
			Vector2[] uvs;
			Vector3 nrm = Vector3.zero;

			foreach(pb_Face face in kvp.Value)
			{
				nrm += pb_Math.Normal( 	_vertices[face.indices[0]],
										_vertices[face.indices[1]],
										_vertices[face.indices[2]] ); 
			}

			nrm /= (float)kvp.Value.Count;

			if(kvp.Value[0].uv.useWorldSpace)
			{
				transform.TransformDirection(nrm);
				uvs = pb_UV_Utility.PlanarMap( transform.ToWorldSpace(GetVertices(pb_Face.AllTrianglesDistinct(kvp.Value).ToArray())), kvp.Value[0].uv, nrm);
			}
			else
			{
				uvs = pb_UV_Utility.PlanarMap( GetVertices(pb_Face.AllTrianglesDistinct(kvp.Value).ToArray()), kvp.Value[0].uv, nrm);
			}
			
			/**
			 * Apply UVs to array, and update the localPivot and localSize caches.
			 */
			int j = 0;

			Vector2 pivot = kvp.Value[0].uv.localPivot, size = kvp.Value[0].uv.localSize;
			foreach(pb_Face f in kvp.Value)
			{
				f.uv.localPivot = pivot;
				f.uv.localSize = size;

				foreach(int i in f.distinctIndices)
					newUVs[i] = uvs[j++];
			}
		}

		_uv = newUVs;
		msh.uv = newUVs;
	}

	/**
	 * Set the material for this face to use.  Also updates the Mesh so that
	 * changes are apparent to user.
	 */
	public void SetFaceMaterial(pb_Face quad, Material mat)
	{
		quad.SetMaterial(mat);
		RefreshUV(new pb_Face[]{quad});
	}

	public void SetFaceMaterial(pb_Face[] quad, Material mat)
	{
		for(int i = 0; i < quad.Length; i++)
			quad[i].SetMaterial(mat);
	}

	/**
	 *	\brief Sets the pb_Face uvSettings param to match the passed #pv_UV _uv 
	 */
	public void SetFaceUV(pb_Face face, pb_UV uvParams)
	{
		face.SetUV(uvParams);

		if(face.uv.useWorldSpace)
		{
			Vector3[] v = new Vector3[face.distinctIndices.Length];
			for(int i = 0; i < v.Length; i++)
				v[i] = _vertices[face.distinctIndices[i]];

			SetUVs(face, pb_UV_Utility.PlanarMap( v, face.uv) );
		}
		else
			SetUVs(face, pb_UV_Utility.PlanarMap( face.GetDistinctVertices(_vertices), face.uv) );
	}

	/**
	 * Apply the UV to the mesh UV channel.
	 */
	private void SetUVs(pb_Face face, Vector2[] uvs)
	{
		int[] vertIndices = face.distinctIndices;
		Vector2[] newUV = new Vector2[msh.uv.Length];
		System.Array.Copy(msh.uv, newUV, msh.uv.Length);
		
		for(int i = 0; i < vertIndices.Length; i++) {
			newUV[vertIndices[i]] = uvs[i];
		}

		gameObject.GetComponent<MeshFilter>().sharedMesh.uv = newUV;		
	}

	private void SetUVs(pb_Face[] quad, Vector2[][] uvs)
	{
		Vector2[] newUV = new Vector2[msh.uv.Length];
		System.Array.Copy(msh.uv, newUV, msh.uv.Length);
		
		for(int i = 0; i < quad.Length; i++) {

			int[] vertIndices = quad[i].distinctIndices;
			for(int n = 0; n < vertIndices.Length; n++)
				newUV[vertIndices[n]] = uvs[i][n];
		
		}

		gameObject.GetComponent<MeshFilter>().sharedMesh.uv = newUV;
	}

	/**
	 * Set mesh UV2.
	 */
	public void SetUV2(Vector2[] v)
	{
		GetComponent<MeshFilter>().sharedMesh.uv2 = v;
	}
#endregion

#region COLORS

	/**
	 * Set the internal color array.
	 */
	public void SetColors(Color[] InColors)
	{
		_colors = InColors.Length == vertexCount ? InColors : pbUtil.FilledArray<Color>(Color.white, vertexCount);
	}

	public void SetFaceColor(pb_Face face, Color color)
	{
		if(_colors == null) _colors = pbUtil.FilledArray<Color>(Color.white, vertexCount);

		foreach(int i in face.distinctIndices)
		{
			_colors[i] = color;
		}
	}

	public void RefreshColor()
	{
		if(_colors == null) _colors = pbUtil.FilledArray<Color>(Color.white, vertexCount);

		msh.colors = _colors;
	}
#endregion

#region TANGENTS

	public void RefreshTangent()
	{
		// implementation found here (no sense re-inventing the wheel, eh?)
		// http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

		//speed up math by copying the mesh arrays
		int[] triangles = msh.triangles;
		Vector3[] vertices = msh.vertices;
		Vector2[] uv = msh.uv;
		Vector3[] normals = msh.normals;

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

			//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
			//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
			Vector3.OrthoNormalize(ref n, ref t);
			tangents[a].x = t.x;
			tangents[a].y = t.y;
			tangents[a].z = t.z;

			tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		}

		gameObject.GetComponent<MeshFilter>().sharedMesh.tangents = tangents;
	}
#endregion

#region NORMALS

	/**
	 * Refreshes the normals of this object taking into account the smoothing groups.
	 */
	public void RefreshNormals()
	{
		Mesh m = msh;

		// All hard edges
		m.RecalculateNormals();
			
		// Per-vertex (not ideal unless you've got a sphere)
		// SmoothPerVertexNormals();

		SmoothPerGroups();
	}
	// groups are per-face
	private void SmoothPerGroups()
	{
		// it might make sense to cache this...
		Dictionary<int, List<pb_Face>> groups = new Dictionary<int, List<pb_Face>>();
		for(int i = 0; i < faces.Length; i++) {
			// smoothing groups 
			// 0 		= none
			// 1 - 24 	= smooth
			// 25 - 42	= hard
			if(faces[i].smoothingGroup > 0 && faces[i].smoothingGroup < 25)
			{
				if(groups.ContainsKey(faces[i].smoothingGroup))
					groups[faces[i].smoothingGroup].Add(faces[i]);
				else
					groups.Add(faces[i].smoothingGroup, new List<pb_Face>(){faces[i]});
			}
		}

		Vector3[] nrmls = msh.normals;
		foreach(KeyValuePair<int, List<pb_Face>> kvp in groups)
		{
			// Sort shared normals into groups discarding normals at indices that don't belong to shared group
			int[][] smoothed;
			{
				List<int> distinct = pb_Face.AllTrianglesDistinct(kvp.Value);
				Dictionary<int, List<int>> shared = new Dictionary<int, List<int>>();
				int i = 0;
				for(i = 0; i < distinct.Count; i++)
				{
					int sharedIndex = sharedIndices.IndexOf(distinct[i]);
					
					if(shared.ContainsKey(sharedIndex))
						shared[sharedIndex].Add(distinct[i]);
					else
						shared.Add(sharedIndex, new List<int>(){distinct[i]});
				}

				smoothed = new int[shared.Count][];
				i = 0;
				foreach(KeyValuePair<int, List<int>> skvp in shared)
				{
					smoothed[i++] = skvp.Value.ToArray();
				}
			}


			for(int i = 0; i < smoothed.Length; i++)
			{
				Vector3[] vN = new Vector3[smoothed[i].Length];
				int n = 0;
				for(n = 0; n < vN.Length; n++)
					vN[n] = nrmls[smoothed[i][n]];

				Vector3 nrml = pb_Math.Average(vN);

				for(n = 0; n < smoothed[i].Length; n++)
					nrmls[smoothed[i][n]] = nrml.normalized;
			}
		}

		GetComponent<MeshFilter>().sharedMesh.normals = nrmls;
	}
#endregion

#region CLEANUP

	// This won't run unless ExecuteInEditMode is set.  If we destroy the mesh, there's no going back,
	// so unless people really complain about that mesh leak when deleting objects, we'll just let it be.
	public void OnDestroy()
	{
		// DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);
	}
#endregion

#region OVERRIDES

	public override string ToString()
	{
		string str =  
			"Name: " + gameObject.name + "\n" +
			"ID: " + id + "\n" +
			"Entity Type: " + GetComponent<pb_Entity>().entityType + "\n" +
			"Shared / Total Vertices: " + sharedIndices.Length + " , " + msh.vertices.Length + "\n" +
			"faces: " + faces.Length;
		return str;
	}

	public string ToStringDetailed()
	{
		string str =  
			"Name: " + gameObject.name + "\n" +
			"\tStatic: " + gameObject.isStatic + "\n" + 
			"\tID: " + id + "\n" +
			"\tEntity Type: " + GetComponent<pb_Entity>().entityType + "\n" +
			"\tShared Vertices: " + sharedIndices.Length + "\n" +
			"\tVertices int/msh: " + _vertices.Length + ", " + msh.vertices.Length + "\n" +
			"\tUVs int/msh: " + _uv.Length + ", " + msh.uv.Length + "\n" +
			"\tTriangles: " + msh.triangles.Length + "\n" + 
			"\tFaces: " + faces.Length + "\n" +
			"\tSubmesh: " + msh.subMeshCount + "\n" +

			"\t# Vertices\n" + pbUtil.ToFormattedString(_vertices, "\n\t\t") + "\n" +
			"\t# UVs\n" + pbUtil.ToFormattedString(_uv, "\n\t\t") + "\n" +
			"\t# Shared:\n" + sharedIndices.ToFormattedString("\n\t\t") + "\n" + 
			"\t# Faces:\n" + pbUtil.ToFormattedString(_faces, "\n\t\t") + "\n"+
			"\t# UV:\n" + _faces.Select(x => x.uv).ToArray().ToFormattedString("\n\t\t");

		return str;
	}
#endregion

#region REBUILDING / INSTANTIATION

	/**
	 *	\brief Forces each pb_Face in the object to rebuild it's edge arrays.
	 *	Recommended to be done after adding or removing vertices / triangles
	 */
	public void RebuildFaceCaches()
	{
		foreach(pb_Face f in faces)
			f.RebuildCaches();
	}

	/**
	 * Checks if the mesh component is lost or does not match _vertices, and if so attempt to rebuild.
	 * returns True if object is okay, false if a rebuild was necessary and you now need to regenerate UV2.
	 */
	public bool Verify()
	{	
		if(msh == null)
		{
			// attempt reconstruction...
			if( !ReconstructMesh() )	
			{
				// reconstruct failed.  this shouldn't happen, but sometimes it does?
				Debug.LogError("ProBuilder Object " + id + " contains null geometry.  Self destruct in 5...4...3...");
				DestroyImmediate(this.gameObject);
			}

			return false;
		}
		
		int meshNo;
		int.TryParse(msh.name.Replace("pb_Mesh", ""), out meshNo);
		if(meshNo != id)
		{
			MakeUnique();
			return false;
		}

		return msh.uv2 != null;

		// // check to make sure that faces and vertex data from mesh match
		// // pb_Object cached values.  Can change when applying/reverting
		// // prefabs
		// if(!msh.vertices.IsEqual(_vertices))
		// {
		// 	ReconstructMesh();
		// 	return false;
		// }	

		// // upgrade path things
		// if(_uv == null || !msh.uv.IsEqual(_uv))	
		// 	RefreshUV();

		// /**
		//  * If the submeshes don't match, rebuild
		//  */
		// if(_submeshTriangleCount == null || _submeshTriangleCount.Length != msh.subMeshCount)
		// {
		// 	ReconstructMesh();
		// }
		// else
		// {
		// 	for(int i = 0; i < _submeshTriangleCount.Length; i++)
		// 	{
		// 		if( _submeshTriangleCount[i] != msh.GetTriangles(i).Length)
		// 		{
		// 			ReconstructMesh();
		// 			break;
		// 		}
		// 	}
		// }

		// return msh.uv2 != null && msh.uv2.Length == vertexCount;
	}
#endregion
}