using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A set of indices and material.
	/// </summary>
	[Serializable]
	public sealed class Submesh
	{
		/// <value>
		/// Indices making up this submesh. Can be triangles or quads.
		/// </value>
        [SerializeField]
		internal int[] m_Indices;

		/// <value>
		/// What topology is this submesh?
		/// </value>
        [SerializeField]
		internal MeshTopology m_Topology;

		/// <value>
		/// What material does this submesh use?
		/// </value>
        [SerializeField]
		internal Material m_Material;

		/// <summary>
		/// Create new Submesh.
		/// </summary>
		/// <param name="material">The material that this submesh renders with.</param>
		/// <param name="topology">What topology is this submesh. ProBuilder only recognizes Triangles and Quads.</param>
		/// <param name="indexes">The triangles or quads.</param>
		public Submesh(Material material, MeshTopology topology, IEnumerable<int> indexes)
		{
            if (indexes == null)
                throw new ArgumentNullException("indexes");

			m_Indices = indexes.ToArray();
			m_Topology = topology;
			m_Material = material;
		}

		/// <summary>
		/// Create new Submesh from a mesh, submesh index, and material.
		/// </summary>
		/// <param name="mesh">The source mesh.</param>
		/// <param name="subMeshIndex">Which submesh to read from.</param>
		/// <param name="material">The material this submesh should render with.</param>
		public Submesh(Mesh mesh, int subMeshIndex, Material material)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			m_Indices = mesh.GetIndices(subMeshIndex);
			m_Topology = mesh.GetTopology(subMeshIndex);
			m_Material = material;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", m_Material != null ? m_Material.name : "null", m_Topology.ToString(), m_Indices != null ? m_Indices.Length.ToString() : "0");
		}
	}
}
