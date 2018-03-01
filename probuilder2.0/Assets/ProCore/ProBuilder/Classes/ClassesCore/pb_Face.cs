using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// A face is composed of a set of triangles, and a material.
	/// </summary>
	[System.Serializable]
	public class pb_Face
	{
		public pb_Face() {}

		public pb_Face(int[] i)
		{
			SetIndices(i);
			_uv = new pb_UV();
			_mat = pb_Material.DefaultMaterial;
			_smoothingGroup = pb_Smoothing.SMOOTHING_GROUP_NONE;
			textureGroup = -1;
			elementGroup = 0;
		}

		public pb_Face(int[] i, Material m, pb_UV u, int smoothingGroup, int textureGroup, int elementGroup, bool manualUV)
		{
			this.SetIndices(i);
			this._uv = new pb_UV(u);
			this._mat = m;
			this._smoothingGroup = smoothingGroup;
			this.textureGroup = textureGroup;
			this.elementGroup = elementGroup;
			this.manualUV = manualUV;
		}

		/// <summary>
		/// Deep copy constructor.
		/// </summary>
		/// <param name="face"></param>
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

		/// <summary>
		/// Copies values from other to this face.
		/// </summary>
		/// <param name="other"></param>
		public void CopyFrom(pb_Face other)
		{
			int len = other.indices == null ? 0 : other.indices.Length;
			_indices = new int[len];
			System.Array.Copy(other.indices, _indices, len);
			_smoothingGroup = other.smoothingGroup;
			_uv = new pb_UV(other.uv);
			_mat = other.material;
			manualUV = other.manualUV;
			elementGroup = other.elementGroup;
			RebuildCaches();
		}

		[SerializeField] int[] _indices;
		[SerializeField] int[] _distinctIndices;

		/// <summary>
		/// A cache of the calculated #pb_Edge edges for this face. Call RebuildCaches to update.
		/// </summary>
		[SerializeField] pb_Edge[] _edges;

		/// <summary>
		/// Adjacent faces sharing this smoothingGroup will have their abutting edge normals averaged.
		/// </summary>
		[SerializeField] int _smoothingGroup;

		/// <summary>
		/// If manualUV is false, these parameters determine how this face's vertices are projected to 2d space.
		/// </summary>
		[SerializeField] pb_UV _uv;

		/// <summary>
		/// What material does this face use.
		/// </summary>
		[SerializeField] Material _mat;

		/// <summary>
		/// If this face has had it's UV coordinates done by hand, don't update them with the auto unwrap crowd.
		/// </summary>
		public bool manualUV;

		/// <summary>
		/// UV element group. Used by the UV editor to group faces.
		/// </summary>
		internal int elementGroup;

		/// <summary>
		/// What texture group this face belongs to. Used when projecting auto UVs.
		/// </summary>
		public int textureGroup = -1;

		/// <summary>
		/// Return a reference to the triangle indices that make up this face.
		/// </summary>
		public int[] indices { get { return _indices; } }

		/// <summary>
		/// Returns a reference to the cached distinct indices (each vertex index is only referenced once in distinctIndices).
		/// </summary>
		public int[] distinctIndices { get { return _distinctIndices == null ? CacheDistinctIndices() : _distinctIndices; } }

		/// <summary>
		/// A reference to the border edges that make up this face.
		/// </summary>
		public pb_Edge[] edges { get { return _edges == null ? CacheEdges() : _edges; } }

		/// <summary>
		/// What smoothing group this face belongs to, if any. This is used to calculate vertex normals.
		/// </summary>
		public int smoothingGroup { get { return _smoothingGroup; } set { _smoothingGroup = value; } }

		/// <summary>
		/// Get the material that face uses.
		/// </summary>
		public Material material { get { return _mat; } set { _mat = value; } }

		/// <summary>
		/// A reference to the Auto UV mapping parameters.
		/// </summary>
		public pb_UV uv { get { return _uv; } set { _uv = value; } }

		/// <summary>
		/// Accesses the indices array.
		/// </summary>
		/// <param name="i"></param>
		public int this[int i] { get { return indices[i]; } }

		/// <summary>
		/// Check if this face has more than 2 indices.
		/// </summary>
		/// <returns></returns>
		public bool IsValid()
		{
			return indices.Length > 2;
		}

		/// <summary>
		/// Return all edges, including non-perimeter ones.
		/// </summary>
		/// <returns></returns>
		public pb_Edge[] GetAllEdges()
		{
			pb_Edge[] edges = new pb_Edge[indices.Length];

			for(int i = 0; i < indices.Length; i+=3)
			{
				edges[i  ] = new pb_Edge(indices[i+0], indices[i+1]);
				edges[i+1] = new pb_Edge(indices[i+1], indices[i+2]);
				edges[i+2] = new pb_Edge(indices[i+2], indices[i+0]);
			}
			return edges;
		}

		/// <summary>
		/// Sets this face's indices to a new value.
		/// </summary>
		/// <param name="i"></param>
		public void SetIndices(int[] i)
		{
			_indices = i;
			RebuildCaches();
		}

		/// <summary>
		/// Add offset to each value in the indices array.
		/// </summary>
		/// <param name="offset"></param>
		public void ShiftIndices(int offset)
		{
			for(int i = 0; i <_indices.Length; i++)
				_indices[i] += offset;
		}

		/// <summary>
		/// Returns the smallest value in the indices array.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Shifts all triangles to be zero indexed.
		/// Ex:
		/// new pb_Face(3,4,5).ShiftIndicesToZero();
		/// Sets the pb_Face index array to 0,1,2
		/// </summary>
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

		/// <summary>
		/// Reverse the winding order of this face.
		/// </summary>
		public void ReverseIndices()
		{
			System.Array.Reverse(_indices);
			RebuildCaches();
		}

		/// <summary>
		/// Rebuilds all property caches on pb_Face.
		/// </summary>
		public void RebuildCaches()
		{
			CacheDistinctIndices();
			CacheEdges();
		}

		pb_Edge[] CacheEdges()
		{
			if(_indices == null)
				return null;

			HashSet<pb_Edge> dist = new HashSet<pb_Edge>();
			List<pb_Edge> dup = new List<pb_Edge>();

			for(int i = 0; i < indices.Length; i+=3)
			{
				pb_Edge a = new pb_Edge(indices[i+0],indices[i+1]);
				pb_Edge b = new pb_Edge(indices[i+1],indices[i+2]);
				pb_Edge c = new pb_Edge(indices[i+2],indices[i+0]);

				if(!dist.Add(a)) dup.Add(a);
				if(!dist.Add(b)) dup.Add(b);
				if(!dist.Add(c)) dup.Add(c);
			}

			dist.ExceptWith(dup);

			_edges = dist.ToArray();

			return _edges;
		}

		int[] CacheDistinctIndices()
		{
			if(_indices == null)
				return null;

			_distinctIndices = new HashSet<int>(_indices).ToArray();

			return distinctIndices;
		}

		/// <summary>
		/// Test if the face contains a triangle.
		/// </summary>
		/// <param name="triangle"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Returns all triangles contained within the #pb_Face array.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		internal static int[] AllTriangles(pb_Face[] q)
		{
			List<int> all = new List<int>(q.Length * 6);

			foreach(pb_Face quad in q)
				all.AddRange(quad.indices);

			return all.ToArray();
		}

		/// <summary>
		/// Attempts to create quad, or on failing just return the triangle indices.
		/// </summary>
		/// <param name="quadOrTris"></param>
		/// <returns></returns>
		public MeshTopology ToQuadOrTriangles(out int[] quadOrTris)
		{
			if(ToQuad(out quadOrTris))
				return MeshTopology.Quads;

			int len = indices == null ? 0 : System.Math.Max(0, indices.Length);
			quadOrTris = new int[len];
			System.Array.Copy(indices, quadOrTris, len);
			return MeshTopology.Triangles;
		}

		/// <summary>
		/// Convert a 2 triangle face to a quad representation. If face does not contain exactly 6 indices this function returns null.
		/// </summary>
		/// <returns></returns>
		public int[] ToQuad()
		{
			int[] quad;
			ToQuad(out quad);
			return quad;
		}

		/// <summary>
		/// Convert a 2 triangle face to a quad representation. If face does not contain exactly 6 indices this function returns null.
		/// </summary>
		/// <param name="quad"></param>
		/// <returns></returns>
		public bool ToQuad(out int[] quad)
		{
			if(indices == null || indices.Length != 6)
			{
				quad = null;
				return false;
			}

			quad = new int[4] { edges[0].x, edges[0].y, -1, -1 };

			if(edges[1].x == quad[1])
				quad[2] = edges[1].y;
			else if(edges[2].x == quad[1])
				quad[2] = edges[2].y;
			else if(edges[3].x == quad[1])
				quad[2] = edges[3].y;

			if(edges[1].x == quad[2])
				quad[3] = edges[1].y;
			else if(edges[2].x == quad[2])
				quad[3] = edges[2].y;
			else if(edges[3].x == quad[2])
				quad[3] = edges[3].y;

			return true;
		}

		/// <summary>
		/// Create submeshes from a set of faces. Currently only Quads and Triangles are supported.
		/// </summary>
		/// <param name="faces"></param>
		/// <param name="submeshes"></param>
		/// <param name="preferredTopology"></param>
		/// <returns>The number of submeshes created.</returns>
		/// <exception cref="NotImplementedException"></exception>
		public static int GetMeshIndices(pb_Face[] faces, out pb_Submesh[] submeshes, MeshTopology preferredTopology = MeshTopology.Triangles)
		{
			if(preferredTopology != MeshTopology.Triangles && preferredTopology != MeshTopology.Quads)
				throw new System.NotImplementedException("Currently only Quads and Triangles are supported.");

			bool wantsQuads = preferredTopology == MeshTopology.Quads;

			Dictionary<Material, List<int>> quads = wantsQuads ? new Dictionary<Material, List<int>>() : null;
			Dictionary<Material, List<int>> tris = new Dictionary<Material, List<int>>();

			int count = faces == null ? 0 : faces.Length;

			for(int i = 0; i < count; i++)
			{
				pb_Face face = faces[i];

				if(face.indices == null || face.indices.Length < 1)
					continue;

				Material material = face.material ?? pb_Material.DefaultMaterial;
				List<int> polys = null;

				int[] res;

				if(wantsQuads && face.ToQuad(out res))
				{
					if(quads.TryGetValue(material, out polys))
						polys.AddRange(res);
					else
						quads.Add(material, new List<int>(res));
				}
				else
				{
					if(tris.TryGetValue(material, out polys))
						polys.AddRange(face.indices);
					else
						tris.Add(material, new List<int>(face.indices));
				}
			}

			int submeshCount = (quads != null ? quads.Count : 0) + tris.Count;
			submeshes = new pb_Submesh[submeshCount];
			int ii = 0;

			if(quads != null)
			{
				foreach(var kvp in quads)
					submeshes[ii++] = new pb_Submesh(kvp.Key, MeshTopology.Quads, kvp.Value.ToArray());
			}

			foreach(var kvp in tris)
				submeshes[ii++] = new pb_Submesh(kvp.Key, MeshTopology.Triangles, kvp.Value.ToArray());

			return submeshCount;
		}

		public override string ToString()
		{
			// shouldn't ever be the case
			if(indices.Length % 3 != 0)
				return "Index count is not a multiple of 3.";

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

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

			return sb.ToString();
		}
	}
}
