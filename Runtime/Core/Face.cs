using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using System.Collections.ObjectModel;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A face is composed of a set of triangles, and a material.
    /// <br />
    /// Triangle indices may point to the same vertex index as long as the vertices are unique to the face. Ie, every vertex that a face references should only be used by that face's indices. To associate vertices that share common attributes (usually position), use the @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" property.
    /// <br />
    /// ProBuilder automatically manages condensing common vertices in the EditorMeshUtility.Optimize function.
    /// </summary>
    [Serializable]
    public sealed class Face : ITriangulatable
    {
        [FormerlySerializedAs("_indices")]
        [SerializeField]
        int[] m_Indices;

        /// <summary>
        /// Adjacent faces sharing this smoothingGroup will have their abutting edge normals averaged.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_smoothingGroup")]
        int m_SmoothingGroup;

        /// <summary>
        /// If manualUV is false, these parameters determine how this face's vertices are projected to 2d space.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_uv")]
        AutoUnwrapSettings m_Uv;

        /// <summary>
        /// What material does this face use.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_mat")]
        Material m_Material;

        /// <value>
        /// If this face has had it's UV coordinates done by hand, don't update them with the auto unwrap crowd.
        /// </value>
        public bool manualUV { get; set; }

        /// <value>
        /// UV element group. Used by the UV editor to group faces.
        /// </value>
        [SerializeField]
        internal int elementGroup;

        /// <value>
        /// What texture group this face belongs to. Used when projecting auto UVs.
        /// </value>
        public int textureGroup { get; set; }

        /// <value>
        /// Return a reference to the triangle indices that make up this face.
        /// </value>
        internal int[] indices
        {
            get { return m_Indices; }
	        set
	        {
		        m_Indices = value;
		        InvalidateCache();
	        }
        }

        /// <value>
        /// The triangle indices that make up this face.
        /// </value>
        public ReadOnlyCollection<int> indexes
        {
            get { return new ReadOnlyCollection<int>(m_Indices); }
        }

        /// <summary>
        /// Set the triangles that compose this face.
        /// </summary>
        /// <param name="array">The new triangle array.</param>
        public void SetIndexes(int[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            int len = array.Length;
            m_Indices = new int[len];
            Array.Copy(array, m_Indices, len);
	        InvalidateCache();
        }

	    [NonSerialized]
	    int[] m_DistinctIndices;

	    [NonSerialized]
	    Edge[] m_Edges;

        /// <value>
        /// Returns a reference to the cached distinct indices (each vertex index is only referenced once in distinctIndices).
        /// </value>
        internal int[] distinctIndices
        {
            get { return m_DistinctIndices == null ? CacheDistinctIndices() : m_DistinctIndices; }
        }

        /// <value>
        /// A collection of the vertex indices that the indexes array references, made distinct.
        /// </value>
        public ReadOnlyCollection<int> distinctIndexes
        {
            get { return new ReadOnlyCollection<int>(distinctIndices); }
        }

	    internal Edge[] edgesInternal
	    {
		    get { return m_Edges == null ? CacheEdges() : m_Edges; }
	    }

        /// <value>
        /// Get the perimeter edges that commpose this face.
        /// </value>
	    public ReadOnlyCollection<Edge> edges
	    {
		    get { return new ReadOnlyCollection<Edge>(edgesInternal); }
	    }

	    /// <value>
		/// What smoothing group this face belongs to, if any. This is used to calculate vertex normals.
		/// </value>
		public int smoothingGroup
		{
			get { return m_SmoothingGroup; }
			set { m_SmoothingGroup = value; }
		}

		/// <value>
		/// Get the material that face uses.
		/// </value>
		public Material material
		{
			get { return m_Material; }
			set { m_Material = value; }
		}

		/// <value>
		/// A reference to the Auto UV mapping parameters.
		/// </value>
		public AutoUnwrapSettings uv
		{
			get { return m_Uv; }
			set { m_Uv = value; }
		}

		/// <summary>
		/// Accesses the indices array.
		/// </summary>
		/// <param name="i"></param>
		public int this[int i]
		{
			get { return indices[i]; }
		}

	    /// <summary>
	    /// Default constructor creates a face with an empty triangles array.
	    /// </summary>
		public Face() {}

		/// <summary>
		/// Initialize a Face with a set of triangles and default values.
		/// </summary>
		/// <param name="array">The new triangles array.</param>
		public Face(int[] array)
		{
			SetIndexes(array);
			m_Uv = new AutoUnwrapSettings();
			m_Material = BuiltinMaterials.DefaultMaterial;
			m_SmoothingGroup = Smoothing.smoothingGroupNone;
			textureGroup = -1;
			elementGroup = 0;
		}

		internal Face(int[] triangles, Material m, AutoUnwrapSettings u, int smoothing, int texture, int element, bool manualUVs)
		{
			SetIndexes(triangles);
			m_Uv = new AutoUnwrapSettings(u);
			m_Material = m;
			m_SmoothingGroup = smoothing;
			textureGroup = texture;
			elementGroup = element;
			manualUV = manualUVs;
		}

		/// <summary>
		/// Deep copy constructor.
		/// </summary>
		/// <param name="other">The Face from which to copy properties.</param>
		public Face(Face other)
		{
			CopyFrom(other);
		}

		/// <summary>
		/// Copies values from other to this face.
		/// </summary>
		/// <param name="other">The Face from which to copy properties.</param>
		public void CopyFrom(Face other)
		{
            if (other == null)
                throw new ArgumentNullException("other");

            int len = other.indices == null ? 0 : other.indices.Length;
			m_Indices = new int[len];
			Array.Copy(other.indices, m_Indices, len);
			m_SmoothingGroup = other.smoothingGroup;
			m_Uv = new AutoUnwrapSettings(other.uv);
			m_Material = other.material;
			manualUV = other.manualUV;
			elementGroup = other.elementGroup;
			InvalidateCache();
		}

		/// <summary>
		/// Check if this face has more than 2 indices.
		/// </summary>
		/// <returns>True if this Face contains at least one valid triangle.</returns>
		public bool IsValid()
		{
			return indices.Length > 2;
		}

		internal void InvalidateCache()
	    {
		    m_Edges = null;
		    m_DistinctIndices = null;
	    }

		Edge[] CacheEdges()
		{
			if(m_Indices == null)
				return null;

			HashSet<Edge> dist = new HashSet<Edge>();
			List<Edge> dup = new List<Edge>();

			for(int i = 0; i < indices.Length; i+=3)
			{
				Edge a = new Edge(indices[i+0],indices[i+1]);
				Edge b = new Edge(indices[i+1],indices[i+2]);
				Edge c = new Edge(indices[i+2],indices[i+0]);

				if(!dist.Add(a)) dup.Add(a);
				if(!dist.Add(b)) dup.Add(b);
				if(!dist.Add(c)) dup.Add(c);
			}

			dist.ExceptWith(dup);
			m_Edges = dist.ToArray();
			return m_Edges;
		}

		int[] CacheDistinctIndices()
		{
			if(m_Indices == null)
				return null;
			m_DistinctIndices = m_Indices.Distinct().ToArray();
			return distinctIndices;
		}

		/// <summary>
		/// Test if a triangle is contained within the triangles array of this face.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <returns></returns>
	    public bool Contains(int a, int b, int c)
		{
			for (int i = 0, cnt = indices.Length; i < cnt; i += 3)
			{
				if (a == indices[i + 0]
					&& b == indices[i + 1]
					&& c == indices[i + 2])
					return true;
			}

			return false;
		}

	    /// <inheritdoc cref="ITriangulatable"/>
	    public int[] ToTriangles()
	    {
		    int len = indices.Length;
		    int[] copy = new int[len];
		    Array.Copy(indices, copy, len);
		    return copy;
	    }

		/// <summary>
		/// Convert a 2 triangle face to a quad representation.
		/// </summary>
		/// <returns>A quad (4 indices), or null if indices are not able to be represented as a quad.</returns>
		public int[] ToQuad()
		{
            if (indices == null || indices.Length != 6)
                return null;

			int[] quad = new int[4] { edgesInternal[0].x, edgesInternal[0].y, -1, -1 };

			if(edgesInternal[1].x == quad[1])
				quad[2] = edgesInternal[1].y;
			else if(edgesInternal[2].x == quad[1])
				quad[2] = edgesInternal[2].y;
			else if(edgesInternal[3].x == quad[1])
				quad[2] = edgesInternal[3].y;

			if(edgesInternal[1].x == quad[2])
				quad[3] = edgesInternal[1].y;
			else if(edgesInternal[2].x == quad[2])
				quad[3] = edgesInternal[2].y;
			else if(edgesInternal[3].x == quad[2])
				quad[3] = edgesInternal[3].y;

			return quad;
		}

		/// <summary>
		/// Create submeshes from a set of faces. Currently only Quads and Triangles are supported.
		/// </summary>
		/// <param name="faces"></param>
		/// <param name="preferredTopology"></param>
		/// <returns>An array of Submeshes.</returns>
		/// <exception cref="NotImplementedException"></exception>
		public static Submesh[] GetSubmeshes(IEnumerable<Face> faces, MeshTopology preferredTopology = MeshTopology.Triangles)
		{
			if(preferredTopology != MeshTopology.Triangles && preferredTopology != MeshTopology.Quads)
				throw new System.NotImplementedException("Currently only Quads and Triangles are supported.");

            if (faces == null)
                throw new ArgumentNullException("faces");

			bool wantsQuads = preferredTopology == MeshTopology.Quads;

			Dictionary<Material, List<int>> quads = wantsQuads ? new Dictionary<Material, List<int>>() : null;
			Dictionary<Material, List<int>> tris = new Dictionary<Material, List<int>>();

            foreach(var face in faces)
			{
				if(face.indices == null || face.indices.Length < 1)
					continue;

				Material material = face.material ?? BuiltinMaterials.DefaultMaterial;
				List<int> polys = null;

				int[] res;

				if(wantsQuads && (res = face.ToQuad()) != null)
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
			var submeshes = new Submesh[submeshCount];
			int ii = 0;

			if(quads != null)
			{
				foreach(var kvp in quads)
					submeshes[ii++] = new Submesh(kvp.Key, MeshTopology.Quads, kvp.Value.ToArray());
			}

			foreach(var kvp in tris)
				submeshes[ii++] = new Submesh(kvp.Key, MeshTopology.Triangles, kvp.Value.ToArray());

			return submeshes;
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
