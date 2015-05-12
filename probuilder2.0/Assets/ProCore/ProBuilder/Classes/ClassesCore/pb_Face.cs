/*
 *	Storage class for associating triangles to faces.
 *	Basically behaves like a dictionary entry, but with
 *	some added functionality.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

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
		info.AddValue("material",			_mat.name,					typeof(string));
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

		string matName = (string)info.GetValue("material", typeof(string));

		foreach(Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)))
		{
			if(mat.name.Equals(matName))
			{
				this._mat = mat;
				break;
			}
		}
	}
#endregion

#region CONSTRUCTORS

	public pb_Face() {}

	public pb_Face(int[] i)
	{
		SetIndices(i);
		_uv = new pb_UV();
		_mat = pb_Constant.DefaultMaterial;
		_smoothingGroup = 0;
		elementGroup = 0;

		RebuildCaches();
	}

	public pb_Face(int[] i, Material m, pb_UV u, int smoothingGroup, int textureGroup, int elementGroup, bool manualUV)
	{
		this._indices = i;
		this._uv = u;
		this._mat = m;
		this._smoothingGroup = smoothingGroup;
		this.textureGroup = textureGroup;
		this.elementGroup = elementGroup;
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

		RebuildCaches();
	}

	public static explicit operator pb_EdgeConnection(pb_Face face)
	{
		return new pb_EdgeConnection(face, null);
	}

	public static explicit operator pb_VertexConnection(pb_Face face)
	{
		return new pb_VertexConnection(face, null);
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
#endregion

#region GET

	public bool IsValid()
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
		_distinctIndices = i.Distinct().ToArray();
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

	
	private pb_Edge[] CacheEdges()
	{
		_edges = pb_Edge.GetPerimeterEdges( GetEdges() );
		return _edges;
	}

	private int[] CacheDistinctIndices()
	{
		_distinctIndices = _indices.Distinct().ToArray();
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

	/**
	 * Sorts faces by material and returns a jagged array of their combined triangles.
	 */
	public static int MeshTriangles(pb_Face[] faces, out int[][] submeshes, out Material[] materials)
	{
		// Sort the faces into groups of like materials
		Dictionary<Material, List<pb_Face>> matDic = new Dictionary<Material, List<pb_Face>>();
		
		int i = 0;

#if PROTOTYPE
			matDic.Add(pb_Constant.DefaultMaterial, new List<pb_Face>(faces));
#else
			for(i = 0; i < faces.Length; i++)
			{
				Material face_mat = faces[i].material ?? pb_Constant.UnityDefaultDiffuse;

				if(matDic.ContainsKey(face_mat))
				{
					matDic[face_mat].Add(faces[i]);
				}
				else
				{
					matDic.Add(face_mat, new List<pb_Face>(1) { faces[i] } );
				}
			}
#endif

		materials = new Material[matDic.Count];
		submeshes = new int[materials.Length][];

		i = 0;
		foreach( KeyValuePair<Material, List<pb_Face>> kvp in matDic )
		{
			submeshes[i] = pb_Face.AllTriangles(kvp.Value);
			materials[i] = kvp.Key;
			i++;
		}

		return submeshes.Length;
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
		sb.Append(" }"); // \nMaterial: " + material.name + "\n" + (manualUV ? "Manual UV" : "Auto UV") + "\nSmoothing: " + smoothingGroup + "\nTexture: " + textureGroup);

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