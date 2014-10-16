/*
 *	Storage class for associating triangles to faces.
 *	Basically behaves like a dictionary entry, but with
 *	some added functionality.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProBuilder2.Common {

[System.Serializable]
/**
 *	\brief Contains mesh and material information.  Used in the creation of #pb_Objects.
 */
public class pb_Face : ISerializable
{

#region SERIALIZATION

	// OnSerialize
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("indices",			_indices, 					typeof(int[]));
		info.AddValue("distinctIndices", 	_distinctIndices, 			typeof(int[]));
		info.AddValue("edges", 				_edges, 					typeof(pb_Edge[]));
		info.AddValue("smoothingGroup",	 	_smoothingGroup, 			typeof(int));
		info.AddValue("uv",	 				_uv, 						typeof(pb_UV));
		info.AddValue("material",			_mat.GetInstanceID(),		typeof(int));
		info.AddValue("manualUV", 			manualUV, 					typeof(bool));
		info.AddValue("elementGroup", 		elementGroup, 				typeof(int));
	}

	// The pb_SerializedMesh constructor is used to deserialize values. 
	public pb_Face(SerializationInfo info, StreamingContext context)
	{
		this._indices = 			(int[])		info.GetValue( "indices",			typeof(int[]));
		this._distinctIndices = 	(int[])		info.GetValue( "distinctIndices",	typeof(int[]));
		this._edges = 				(pb_Edge[])	info.GetValue( "edges",				typeof(pb_Edge[]));
		this._smoothingGroup = 		(int) 		info.GetValue( "smoothingGroup",	typeof(int));
		this._uv = 					(pb_UV) 	info.GetValue( "uv",				typeof(pb_UV));
		this.manualUV = 			(bool) 		info.GetValue( "manualUV",			typeof(bool));
		this.elementGroup = 		(int) 		info.GetValue( "elementGroup",		typeof(int));

		// material is a little different - it requires some fanaglin'
		this._mat = pb_Constant.DefaultMaterial;

		int materialInstanceId = (int)info.GetValue("material", typeof(int));

		foreach(Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)))
		{
			if(mat.GetInstanceID() == materialInstanceId)
			{
				this._mat = mat;
				break;
			}
		}
	}
#endregion

#region CONSTRUCTORS

	public pb_Face(int[] i)
	{
		SetIndices(i);
		_uv = new pb_UV();
		_mat = pb_Constant.DefaultMaterial;
		_smoothingGroup = 0;
		elementGroup = 0;
		_colors = pbUtil.FilledArray((Color32)Color.white, indices.Length);

		RebuildCaches();
	}

	public pb_Face(int[] i, Material m, pb_UV u, int smoothingGroup, int textureGroup, int elementGroup, bool manualUV, Color32 c)
	{
		this._indices = i;
		this._uv = u;
		this._mat = m;
		this._smoothingGroup = smoothingGroup;
		this.textureGroup = textureGroup;
		this.elementGroup = elementGroup;
		this._colors = pbUtil.FilledArray(c, i.Length);
		this.manualUV = manualUV;

		RebuildCaches();
	}

	/**
	 * Deep copy constructor.
	 */
	public pb_Face(pb_Face face)
	{
		_indices = new int[face.indices.Length];
		System.Array.Copy(face.indices, _indices, face.indices.Length);
		
		_uv = new pb_UV(face.uv);

		_mat = face.material;

		_smoothingGroup = face.smoothingGroup;
		textureGroup = face.textureGroup;
		elementGroup = face.elementGroup;
		manualUV = face.manualUV;

		_colors = new Color32[face.colors.Length];
		System.Array.Copy(face.colors, _colors, colors.Length);
		
		manualUV = face.manualUV;

		RebuildCaches();
	}

	public static explicit operator EdgeConnection(pb_Face face)
	{
		return new EdgeConnection(face, null);
	}

	public static explicit operator VertexConnection(pb_Face face)
	{
		return new VertexConnection(face, null);
	}
#endregion

#region MEMBERS

	[SerializeField]
	int[] 	_indices;
	[SerializeField]
	int[]	_distinctIndices;
	[SerializeField]
	pb_Edge[] _edges;			///< A cache of the calculated #pb_Edge edges for this face.  Call pb_Face::RebuildCaches to update. 
	[SerializeField]
	int 	_smoothingGroup;	///< Adjacent faces sharing this smoothingGroup will have their abutting edge normals averaged.
	[SerializeField]
	pb_UV 	_uv;				///< If manualUV is false, these parameters determine how this face's vertices are projected to 2d space.
	[SerializeField]
	Material _mat;				///< What material does this face use.
	[SerializeField]
	Color32[] _colors;			///< Vertex colors (matching indices array).
	[SerializeField]
	public bool manualUV;		///< If this face has had it's UV coordinates done by hand, don't update them with the auto unwrap crowd.
	[SerializeField]	
	public int elementGroup;	///< UV Element group.
#endregion

#region ACCESS

	public int[] indices { get { return _indices; } }
	public int[] distinctIndices { get { return _distinctIndices == null ? CacheDistinctIndices() : 
		_distinctIndices; } }
	public pb_Edge[] edges { get { return _edges == null ? CacheEdges() : _edges; } }	// todo -- remove this after a while
	public int smoothingGroup { get { return _smoothingGroup; } }
	public pb_UV uv { get { return _uv; } }
	public Material material { get { return _mat; } }
	public Material mat { get { Debug.LogWarning("pb_Face->mat property is deprecated.  Please use pb_Face->material instead."); return _mat; } }	// TODO -- remove me
	public Color32[] colors { get { return _colors == null || _colors.Length != indices.Length ? InitColors() : _colors; } } 	///< Returns the color properties for this face.
	public Color32 color { get { return _colors == null || _colors.Length != indices.Length ? (Color32)Color.white : _colors[0]; } } 
	public int textureGroup = -1;

	public void SetUV(pb_UV u)
	{
		_uv = u;
	}

	public void SetMaterial(Material m)
	{
		_mat = m;
	}

	public void SetSmoothingGroup(int i)
	{
		_smoothingGroup = i;
	}

	public void SetColor(Color32 c32)
	{
		for(int i = 0; i < indices.Length; i++)
			_colors[i] = c32;
	}

	public void SetColors(Color32[] c32)
	{
		if(c32.Length != indices.Length)
		{
			Debug.LogWarning("Array must have same length as indices array.  Use pb_Face::SetColor() to set all vertices to one color.");
			return;
		}
		_colors = c32;
	}
#endregion

#region GET

	public bool isValid()
	{
		return indices.Length > 2;
	}

	public Vector3[] GetDistinctVertices(Vector3[] verts)
	{
		int[] di = distinctIndices;
		Vector3[] v = new Vector3[di.Length];
		
		for(int i = 0; i < di.Length; i++) {
			v[i] = verts[di[i]];
		}
		return v;
	}

	/**
	 * Returns the triangle at index in the indices array.
	 * {
	 *	 0, 1, 2,	// tri at index 0
	 *	 2, 3, 1,	// tri at index 1, etc
	 * }
	 */
	public int[] GetTriangle(int index)
	{
		if(index*3+3 > indices.Length)
			return null;
		else
			return new int[3]{indices[index*3+0],indices[index*3+1],indices[index*3+2]};
	}

	public pb_Edge[] GetEdges()
	{
		pb_Edge[] edges = new pb_Edge[indices.Length];
		for(int i = 0; i < indices.Length; i+=3) {
			edges[i+0] = new pb_Edge(indices[i+0],indices[i+1]);
			edges[i+1] = new pb_Edge(indices[i+1],indices[i+2]);
			edges[i+2] = new pb_Edge(indices[i+2],indices[i+0]);
		}
		return edges;
	}
#endregion

#region INSTANCE_METHODS

	/**
	 * Sets this face's indices to a new value.
	 */
	public void SetIndices(int[] i)
	{
		_indices = i;
		_distinctIndices = i.ToDistinctArray();
	}

	/**
	 * Add offset to each value in the indices array.
	 */
	public void ShiftIndices(int offset)
	{
		for(int i = 0; i <_indices.Length; i++)
			_indices[i] += offset;
	}

	/**
	 * Returns the smallest value in the indices array.
	 */
	public int SmallestIndexValue()
	{
		int smallest = _indices[0];
		for(int i = 0; i < _indices.Length; i++)
		{
			if(_indices[i] < smallest)
				smallest = _indices[i];
		}
		return smallest;
	}

	/**
	 *	\brief Shifts all triangles to be zero indexed.
	 *	\ex 
	 *	new pb_Face(3,4,5).ShiftIndicesToZero();
	 *	Sets the pb_Face index array to 0,1,2
	 */
	public void ShiftIndicesToZero()
	{
		int offset = SmallestIndexValue();
 
		for(int i = 0; i < indices.Length; i++)
			_indices[i] -= offset;

		for(int i = 0; i < _distinctIndices.Length; i++)
			_distinctIndices[i] -= offset;

		for(int i = 0; i < _edges.Length; i++)
		{
			_edges[i].x -= offset;
			_edges[i].y -= offset;
		}
	}

	/**
	 * Reverse the winding order of this face.
	 */
	public void ReverseIndices()
	{
		System.Array.Reverse(_indices);
	}

	/**
	 *	\brief Rebuilds all property caches on pb_Face.
	 */
	public void RebuildCaches()
	{
		CacheDistinctIndices();
		CacheEdges();
	}
#endregion

#region PRIVATE

	private Color32[] InitColors()
	{
		_colors = pbUtil.FilledArray((Color32)Color.white, indices.Length);
		return _colors;
	}

	private pb_Edge[] CacheEdges()
	{
		_edges = this.GetPerimeterEdges();
		return _edges;
	}

	private int[] CacheDistinctIndices()
	{
		_distinctIndices = _indices.ToDistinctArray();
		return _distinctIndices;
	}
#endregion

#region QUERY

	public bool Contains(int[] triangle)
	{
		for(int i = 0; i < indices.Length; i+=3)
		{
			if(	triangle.Contains(indices[i+0]) &&
				triangle.Contains(indices[i+1]) &&
				triangle.Contains(indices[i+2]) )
				return true;
		}

		return false;
	}

	public bool Equals(pb_Face face)
	{
		int triCount = face.indices.Length/3;
		for(int i = 0; i < triCount; i++)
			if(!Contains(face.GetTriangle(i)))
				return false;
		return true;
	}
#endregion

#region Special

	/**
	 *	\brief Used to get the Mesh.Colors array from each face in mesh.
	 *	Assumes that your pb_Object and pb_Faces are in sync.  Will fail if not.
	 */
	public static Color32[] Color32ArrayWithFaces(pb_Face[] faces, int vertexCount)
	{
		Color32[] clrs = new Color32[vertexCount];

		foreach(pb_Face face in faces)
		{
			int[] ind = face.indices;
			Color32[] t_clrs = face.colors;
			for(int i = 0; i < ind.Length; i++)
				clrs[ind[i]] = t_clrs[i];
		}


		return clrs;
	}

	/**
	 *	\brief Returns all triangles contained within the #pb_Face array.
	 *	@param faces #pb_Face array to extract triangle data from.
	 *	\returns int[] containing all triangles.  Triangles may point to duplicate vertices that share a world point (not 'distinct' by ProBuilder terminology).
	 */
	public static int[] AllTriangles(pb_Face[] q)
	{
		List<int> all = new List<int>();
		foreach(pb_Face quad in q)
		{
			all.AddRange(quad.indices);
		}
		return all.ToArray();
	}

	public static int[] AllTriangles(List<pb_Face> q)
	{
		List<int> all = new List<int>();
		foreach(pb_Face quad in q)
		{
			all.AddRange(quad.indices);
		}
		return all.ToArray();
	}

	/**
	 *	\brief Returns all distinct triangles contained within the #pb_Face array.
	 *	@param faces #pb_Face array to extract triangle data from.
	 *	\returns int[] containing all triangles.  Triangles may point to duplicate vertices that share a world point (not 'distinct' by ProBuilder terminology).
	 */
	public static int[] AllTrianglesDistinct(pb_Face[] q)
	{
		List<int> all = new List<int>();
		foreach(pb_Face quad in q)
			all.AddRange(quad.distinctIndices);

		return all.ToArray();
	}

	public static List<int> AllTrianglesDistinct(List<pb_Face> f)
	{
		List<int> all = new List<int>();
		foreach(pb_Face quad in f)
			all.AddRange(quad.distinctIndices);

		return all;
	}
#endregion

#region OVERRIDE

	public override string ToString()
	{
		// shouldn't ever be the case
		if(indices.Length % 3 != 0)
			return "Index count is not a multiple of 3.";

		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		sb.Append("{ ");
		for(int i = 0; i < indices.Length; i += 3)
		{	
			sb.Append("[");
			sb.Append(indices[i]);
			sb.Append(", ");
			sb.Append(indices[i+1]);
			sb.Append(", ");
			sb.Append(indices[i+2]);
			sb.Append("]");
			
			if(i < indices.Length-3)
				sb.Append(", ");
		}
		sb.Append(" }  mat: " + material.name + "  isManual: " + manualUV + " smoothing: " + smoothingGroup);

		// sb.Append(edges.ToFormattedString(", "));

		return sb.ToString();
	}

	public string ToStringDetailed()
	{
		string str = 
			"index count: " + _indices.Length + "\n" + 
			"mat name : " + material.name + "\n" +
			"isManual : " + manualUV + "\n" +
			"smoothing group: " + smoothingGroup + "\n";

		for(int i = 0; i < indices.Length; i+=3)
			str += "Tri " + i + ": " + _indices[i+0] + ", " +  _indices[i+1] + ", " +  _indices[i+2] + "\n";

		str += "Distinct Indices:\n";
		for(int i = 0; i < distinctIndices.Length; i++)
			str += distinctIndices[i] + ", ";

		return str;
	}
#endregion
}
}