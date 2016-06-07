/*
 *	ProBuilder Object
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProBuilder2.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

[AddComponentMenu("")]	// Don't let the user add this to any object.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(pb_Entity))]
[ExecuteInEditMode]
/**
 *	\brief Object class for all ProBuilder geometry.
 */	
public class pb_Object : MonoBehaviour
{
#region MONOBEHAVIOUR

	void Awake()
	{
		if(GetComponent<MeshRenderer>().isPartOfStaticBatch)
			return;

		// Absolutely no idea why normals sometimes go haywire
		if(msh == null || msh.normals == null || msh.normals.Length != msh.vertexCount || msh.normals[0] == Vector3.zero)
		{
			// means this object is probably just now being instantiated
			if(_vertices == null)
				return;

			ToMesh();
			Refresh();
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

		Color[] c = new Color[pb.vertexCount];
		System.Array.Copy(pb.colors, c, pb.vertexCount);

		pb_Face[] f = new pb_Face[pb.faces.Length];
		
		for(int i = 0; i < f.Length; i++)
			f[i] = new pb_Face(pb.faces[i]);

		pb_Object p = CreateInstanceWithElements(v, u, c, f, pb.GetSharedIndices(), pb.GetSharedIndicesUV());

		p.gameObject.name = pb.gameObject.name + "-clone";

		return p;
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
		_gameObject.name = "ProBuilder Mesh";

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
	public static pb_Object CreateInstanceWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		GameObject _gameObject = new GameObject();	
		pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
		_gameObject.name = "ProBuilder Mesh";
		pb_obj.GeometryWithVerticesFaces(v, f);
		return pb_obj;
	}

	/**
	 * Creates a new pb_Object instance with the provided vertices, faces, and sharedIndex information.
	 */
	public static pb_Object CreateInstanceWithElements(Vector3[] v, Vector2[] u, Color[] c, pb_Face[] f, pb_IntArray[] si, pb_IntArray[] si_uv)
	{
		GameObject _gameObject = new GameObject();
		pb_Object pb = _gameObject.AddComponent<pb_Object>();

		pb.SetVertices(v);
		pb.SetUV(u);
		pb.SetColors(c);

		pb.SetSharedIndices( si ?? pb_IntArrayUtility.ExtractSharedIndices(v) );

		pb.SetSharedIndicesUV( si_uv ?? new pb_IntArray[0] {});

		pb.SetFaces(f);

		pb.ToMesh();
		pb.Refresh();

		pb.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);

		return pb;
	}

	/**
	 * Creates a new pb_Object instance with the provided vertices, faces, and sharedIndex information.
	 */
	public static pb_Object CreateInstanceWithElements(pb_Vertex[] vertices, pb_Face[] faces, pb_IntArray[] si, pb_IntArray[] si_uv)
	{
		GameObject _gameObject = new GameObject();
		pb_Object pb = _gameObject.AddComponent<pb_Object>();

		Vector3[] 	position;
		Color[] 	color;
		Vector2[] 	uv0;
		Vector3[] 	normal;
		Vector4[] 	tangent;
		Vector2[] 	uv2;
		List<Vector4> uv3;
		List<Vector4> uv4;

		pb_Vertex.GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4);

		pb.SetVertices(position);
		pb.SetColors(color);
		pb.SetUV(uv0);
		if(uv3 != null) pb._uv3 = uv3;
		if(uv4 != null) pb._uv4 = uv4;

		pb.SetSharedIndices( si ?? pb_IntArrayUtility.ExtractSharedIndices(position) );
		pb.SetSharedIndicesUV( si_uv ?? new pb_IntArray[0] {});

		pb.SetFaces(faces);

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
	private List<Vector4>				_uv3;

	[SerializeField]
	private List<Vector4>				_uv4;

	[SerializeField]
	private Vector4[] 					_tangents;

	[SerializeField]
	private pb_IntArray[] 				_sharedIndicesUV = new pb_IntArray[0];

	[SerializeField]
	private Color[] 					_colors;

	public Vector3 						previousTransform = new Vector3(0f, 0f, 0f);
	public bool 						userCollisions = false;	/// If false, ProBuilder will automatically create and scale colliders.
	public bool 						isSelectable = true;	// Optional flag - if true editor should ignore clicks on this object.

	public string						asset_guid;

	// usually when you delete a pb_Object you want to also clean up the mesh asset.  However, there 
	// are situations you'd want to keep the mesh around - like when stripping probuilder scripts.
	public bool dontDestroyMeshOnDelete = false;
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

	public pb_IntArray[] sharedIndices { get { return _sharedIndices; } }	// returns a reference
	public pb_IntArray[] sharedIndicesUV { get { return _sharedIndicesUV; } } 

	public int id { get { return gameObject.GetInstanceID(); } }

	public Vector3[] vertices { get { return _vertices; } }
	public Color[] colors { get { return _colors; } }

	public Vector2[] uv { get { return _uv; } }

	public bool hasUv3 { get { return _uv3 != null && _uv3.Count == vertexCount; } }
	public bool hasUv4 { get { return _uv4 != null && _uv4.Count == vertexCount; } }

	public List<Vector4> uv3 { get { return _uv3; } }
	public List<Vector4> uv4 { get { return _uv4; } }

	public int faceCount { get { return _faces.Length; } }
	public int vertexCount { get { return _vertices.Length; } }
	public int triangleCount { get { return _faces.Sum(x => x.indices.Length ); } }

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

	public pb_Face[]					SelectedFaces { get { return pbUtil.ValuesWithIndices(this.faces, m_selectedFaces); } }
	public int 							SelectedFaceCount { get { return m_selectedFaces.Length; } }
	public int[]						SelectedFaceIndices { get { return m_selectedFaces; } }
	public int[]						SelectedTriangles { get { return m_selectedTriangles; } }
	public int 							SelectedTriangleCount { get { return m_selectedTriangles.Length; } }
	public pb_Edge[]					SelectedEdges { get { return m_SelectedEdges; } }
	public int							SelectedEdgeCount { get { return m_SelectedEdges.Length; } }

	[SerializeField] private int[]		m_selectedFaces 		= new int[]{};
	[SerializeField] private pb_Edge[]	m_SelectedEdges 		= new pb_Edge[]{};
	[SerializeField] private int[]		m_selectedTriangles 	= new int[]{};

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
	public void SetSelectedFaces(IEnumerable<pb_Face> faces)
	{
		SetSelectedFaces(faces.Select(x => System.Array.IndexOf(this.faces, x)).ToArray());
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

	public void SetSelectedEdges(IEnumerable<pb_Edge> edges)
	{
		this.m_selectedFaces = new int[0];
		this.m_SelectedEdges = edges.Select(x => new pb_Edge(x)).ToArray();
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
	 * Sets the internal vertex cache, but does NOT rebuild the mesh.vertices array.
	 * Usually you'll want to call ToMesh() immediately following this.
	 */
	public void SetVertices(Vector3[] v)
	{
		_vertices = v;
	}

	public void SetVertices(IList<pb_Vertex> vertices)
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
		if(uv3 != null) _uv3 = uv3;
		if(uv4 != null) _uv4 = uv4;
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
		_quads = _qds.Where(x => x != null).ToArray();
		if(_quads.Length != _qds.Length)
			Debug.LogWarning("SetFaces() pruned " + (_qds.Length - _quads.Length) + " null faces from this object.");
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
		SetColors( pbUtil.FilledArray<Color>(Color.white, v.Length) );

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
#endregion

#region MESH CONSTRUCTION

	/**
	 * Checks if the mesh component is lost or does not match _vertices, and if so attempt to rebuild.
	 * returns True if object is okay, false if a rebuild was necessary and you now need to regenerate UV2.
	 */
	public MeshRebuildReason Verify()
	{	
		if(msh == null)
		{
			// attempt reconstruction
			try
			{
				ToMesh();
				Refresh();
			}
			catch(System.Exception e)
			{
				Debug.LogError("Failed rebuilding null pb_Object.  Cached mesh attributes are invalid or missing.\n" + e.ToString());
			}

			return MeshRebuildReason.Null;
		}

		int meshNo;
		int.TryParse(msh.name.Replace("pb_Mesh", ""), out meshNo);

		if(meshNo != id)
			return MeshRebuildReason.InstanceIDMismatch;

		return msh.uv2 == null ? MeshRebuildReason.Lightmap : MeshRebuildReason.None;
	}

	/**
	 *	\brief Force regenerate geometry.  Also responsible for sorting faces with shared materials into the same submeshes.
	 */
	public void ToMesh()
	{
		Mesh m = msh;

		// if the mesh vertex count hasn't been modified, we can keep most of the mesh elements around
		if(m != null && m.vertexCount == _vertices.Length)
		{
			m = msh;

			m.vertices = _vertices;
			
			// we're upgrading from a release that didn't cache UVs probably (anything 2.2.5 or lower)
			if(_uv != null)
				m.uv = _uv;
		}
		else
		{
			if(m == null)
				m = new Mesh();
			else
				m.Clear();
	
			m.vertices = _vertices;
		}

		m.uv2 = null;

		int[][] tris;
		Material[] mats;

		m.subMeshCount = pb_Face.MeshTriangles(faces, out tris, out mats);

		for(int i = 0; i < tris.Length; i++)
			m.SetTriangles(tris[i], i);

		m.name = "pb_Mesh" + id;

		GetComponent<MeshFilter>().sharedMesh = m;
#if !PROTOTYPE
		GetComponent<MeshRenderer>().sharedMaterials = mats;
#endif
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

		if(_uv != null && _uv.Length == vertexCount)
		{
			Vector2[] u = new Vector2[vertexCount];
			System.Array.Copy(_uv, u, vertexCount);
			SetUV(u);
		}

		msh = new Mesh();
		
		ToMesh();
		Refresh();
	}

	/**
	 *	\brief Recalculates standard mesh properties - normals, collisions, UVs, tangents, and colors.
	 */
	public void Refresh()
	{	
		// Mesh
		Mesh m = msh;

		m.RecalculateBounds();
		
		if(!userCollisions && GetComponent<Collider>())
		{
			foreach(Collider c in gameObject.GetComponents<Collider>())
			{
				System.Type t = c.GetType();

				if(t == typeof(BoxCollider))
				{
					((BoxCollider)c).center = m.bounds.center;
					((BoxCollider)c).size = m.bounds.size;
				} else
				if(t == typeof(SphereCollider))
				{
					((SphereCollider)c).center = m.bounds.center;
					((SphereCollider)c).radius = pb_Math.LargestValue(m.bounds.extents);
				} else
				if(t == typeof(CapsuleCollider))
				{
					((CapsuleCollider)c).center = m.bounds.center;
					Vector2 xy = new Vector2(m.bounds.extents.x, m.bounds.extents.z);
					((CapsuleCollider)c).radius = pb_Math.LargestValue(xy);
					((CapsuleCollider)c).height = m.bounds.size.y;
				} else
				if(t == typeof(WheelCollider))
				{
					((WheelCollider)c).center = m.bounds.center;
					((WheelCollider)c).radius = pb_Math.LargestValue(m.bounds.extents);
				} else
				if(t == typeof(MeshCollider))
				{
					gameObject.GetComponent<MeshCollider>().sharedMesh = null;	// this is stupid.
					gameObject.GetComponent<MeshCollider>().sharedMesh = m;
				} 
			}
		}

		RefreshUV();
		RefreshColors();
		RefreshNormals();
		RefreshTangents();
	}	
#endregion

#region UV

	/**
	 *	Returns a new unused texture group id.
	 */
	public int UnusedTextureGroup(int i = 1)
	{
		while( System.Array.Exists(faces, element => element.textureGroup == i) )
			i++;

		return i;
	}

	/**
	 * Returns a new unused element group.   Will be greater than or equal to i.
	 */
	public int UnusedElementGroup(int i = 1)
	{
		while( System.Array.Exists(faces, element => element.elementGroup == i) )
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
	 *	Copy values in UV channel to uvs.
	 *	channel is zero indexed.
	 *		mesh.uv0/1 = 0
	 *		mesh.uv2 = 1
	 *		mesh.uv3 = 2
	 *		mesh.uv4 = 3
	 */
	public void GetUVs(int channel, List<Vector4> uvs)
	{
		uvs.Clear();

		switch(channel)
		{
			case 0:
			default:
				for(int i = 0; i < vertexCount; i++)
					uvs.Add((Vector4)_uv[i]);
				break;

			case 1:
				if(msh != null && msh.uv2 != null)
				{
					Vector2[] uv2 = msh.uv2;
					for(int i = 0; i < uv2.Length; i++)
						uvs.Add((Vector4)uv2[i]);
				}
				break;

			case 2:
				if(_uv3 != null)
					uvs.AddRange(_uv3);
				break;

			case 3:
				if(_uv4 != null)
					uvs.AddRange(_uv4);
				break;
		}
	}

	/**
	 *	Sets the UVs on channel.  Does not apply to mesh (use RefreshUV).
	 */
	public void SetUVs(int channel, List<Vector4> uvs)
	{
		switch(channel)
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
				foreach(pb_Face f in this.faces)
					f.manualUV = false;

				// this necessitates rebuilding ALL the face uvs, so make sure we do that.
				faces = this.faces;

				newUVs = new Vector2[vertexCount];
			}
		}

		int n = -2;
		foreach(pb_Face f in faces)
		{
			if(f == null || f.manualUV) 
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
				nrm = transform.TransformDirection(nrm);
				uvs = pb_UVUtility.PlanarMap( transform.ToWorldSpace(vertices.ValuesWithIndices(pb_Face.AllTrianglesDistinct(kvp.Value).ToArray())),
					kvp.Value[0].uv, nrm);
			}
			else
			{
				uvs = pb_UVUtility.PlanarMap( vertices.ValuesWithIndices(pb_Face.AllTrianglesDistinct(kvp.Value).ToArray()), kvp.Value[0].uv, nrm);
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
		
#if UNITY_5_3
		if(hasUv3) msh.SetUVs(2, uv3);
		if(hasUv4) msh.SetUVs(3, uv4);
#endif
	}

	/**
	 *	Set the material on all faces.  Call ToMesh() and Refresh() after to force these changes to take effect.
	 */
	public void SetFaceMaterial(pb_Face[] quad, Material mat)
	{
#if PROTOTYPE
		GetComponent<MeshRenderer>().sharedMaterials = new Material[1] { mat };
#else
		for(int i = 0; i < quad.Length; i++)
			quad[i].SetMaterial(mat);
#endif
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

	public void RefreshColors()
	{
		Mesh m = GetComponent<MeshFilter>().sharedMesh;

		if(_colors == null || _colors.Length != vertexCount)
			_colors = pbUtil.FilledArray<Color>(Color.white, vertexCount);

		m.colors = _colors;
	}

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
#endregion

#region NORMALS AND TANGENTS

	/**
	 *	Set the tangent array on this mesh.
	 */
	public void SetTangents(Vector4[] tangents)
	{
		_tangents = tangents;
	}

	/**
	 * Refreshes the normals of this object taking into account the smoothing groups.
	 */
	public void RefreshNormals()
	{
		// All hard edges
		msh.RecalculateNormals();
			
		// average the soft edge faces
		Vector3[] normals = msh.normals;

		int[] smoothGroup = new int[normals.Length];

		/**
		 * Create a lookup of each triangles smoothing group.
		 */
		foreach(pb_Face face in faces)
		{
			foreach(int tri in face.distinctIndices)
				smoothGroup[tri] = face.smoothingGroup;
		}

		List<int> list;

		/**
		 * For each sharedIndices group (individual vertex), find vertices that are in the same smoothing
		 * group and average their normals.
		 */
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			Dictionary<int, List<int>> shareable = new Dictionary<int, List<int>>();

			/**
			 * Sort indices that share a smoothing group
			 */
			foreach(int tri in sharedIndices[i].array)
			{
				if(smoothGroup[tri] < 1 || smoothGroup[tri] > 24)	
					continue;

				if( shareable.TryGetValue(smoothGroup[tri], out list) )
					list.Add(tri);
				else
					shareable.Add(smoothGroup[tri], new List<int>() { tri });
			}

			/**
			 * Average the normals
			 */
			foreach(KeyValuePair<int, List<int>> skvp in shareable)
			{
				Vector3 avg = Vector3.zero;

				List<int> indices = skvp.Value;

				for(int vertexNormalIndex = 0; vertexNormalIndex < indices.Count; vertexNormalIndex++)
				{
					avg += normals[indices[vertexNormalIndex]];
				}

				// apply normal average back to the mesh
				avg = (avg / (float)skvp.Value.Count).normalized;

				foreach(int vertexNormalIndex in skvp.Value)
					normals[vertexNormalIndex] = avg;
			}
		}

		GetComponent<MeshFilter>().sharedMesh.normals = normals;
	}

	public void RefreshTangents()
	{
		Mesh m = GetComponent<MeshFilter>().sharedMesh;

		if(_tangents != null && _tangents.Length == vertexCount)
			m.tangents = _tangents;
		else
			pb_MeshUtility.GenerateTangent(ref m);
	}
#endregion

#region CLEANUP

	public void OnDestroy()
	{
		if(!dontDestroyMeshOnDelete)
			DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh, true);
	}
#endregion

}
