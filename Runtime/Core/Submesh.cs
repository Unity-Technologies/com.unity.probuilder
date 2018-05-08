using System;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A set of indices and material.
	/// </summary>
	[Serializable]
	public sealed class Submesh
	{
		/// <summary>
		/// Indices making up this submesh. Can be triangles or quads.
		/// </summary>
        [SerializeField]
		internal int[] m_Indices;

		/// <summary>
		/// What topology is this submesh?
		/// </summary>
        [SerializeField]
		internal MeshTopology m_Topology;

		/// <summary>
		/// What material does this submesh use?
		/// </summary>
        [SerializeField]
		internal Material m_Material;

		/// <summary>
		/// Create new pb_Submesh.
		/// </summary>
		/// <param name="material"></param>
		/// <param name="topology"></param>
		/// <param name="indexes"></param>
		public Submesh(Material material, MeshTopology topology, int[] indexes)
		{
            if (indexes == null)
                throw new ArgumentNullException("indexes");
            int len = indexes.Length;
            m_Indices = new int[len];
            Array.Copy(indexes, m_Indices, len);
			m_Topology = topology;
			m_Material = material;
		}

		/// <summary>
		/// Create new pb_Submesh from a mesh, submesh index and material.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="subMeshIndex"></param>
		/// <param name="material"></param>
		public Submesh(Mesh mesh, int subMeshIndex, Material material)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			this.m_Indices = mesh.GetIndices(subMeshIndex);
			this.m_Topology = mesh.GetTopology(subMeshIndex);
			this.m_Material = material;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", m_Material != null ? m_Material.name : "null", m_Topology.ToString(), m_Indices != null ? m_Indices.Length.ToString() : "0");
		}
	}
}
