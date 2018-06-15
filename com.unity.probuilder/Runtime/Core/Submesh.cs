using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A set of indexes and material.
	/// </summary>
	[Serializable]
	public sealed class Submesh
	{
        [SerializeField]
		internal int[] m_Indexes;

        [SerializeField]
		internal MeshTopology m_Topology;

        [SerializeField]
		internal Material m_Material;

		/// <value>
		/// Indexes making up this submesh. Can be triangles or quads, check with topology.
		/// </value>
		public IEnumerable<int> indexes
		{
			get { return new ReadOnlyCollection<int>(m_Indexes); }
			set { m_Indexes = value.ToArray(); }
		}

		/// <value>
		/// What is the topology (triangles, quads) of this submesh?
		/// </value>
		public MeshTopology topology
		{
			get { return m_Topology; }
			set { m_Topology = value; }
		}

		/// <value>
		/// What material does this submesh use?
		/// </value>
		public Material material
		{
			get { return m_Material; }
			set { m_Material = value; }
		}

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

			m_Indexes = indexes.ToArray();
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

			m_Indexes = mesh.GetIndices(subMeshIndex);
			m_Topology = mesh.GetTopology(subMeshIndex);
			m_Material = material;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", m_Material != null ? m_Material.name : "null", m_Topology.ToString(), m_Indexes != null ? m_Indexes.Length.ToString() : "0");
		}

		/// <summary>
		/// Create submeshes from a set of faces. Currently only Quads and Triangles are supported.
		/// </summary>
		/// <param name="faces">The faces to be included in the resulting submeshes. This method handles groups submeshes by comparing the material property of each face.</param>
		/// <param name="preferredTopology">Should the resulting submeshes be in quads or triangles. Note that quads are not guaranteed; ie, some faces may not be able to be represented in quad format and will fall back on triangles.</param>
		/// <returns>An array of Submeshes.</returns>
		/// <exception cref="NotImplementedException">Thrown in the event that a MeshTopology other than Quads or Triangles is passed.</exception>
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
				if(face.indexesInternal == null || face.indexesInternal.Length < 1)
					continue;

				Material material = face.material != null ? face.material : BuiltinMaterials.defaultMaterial;
				List<int> polys = null;

				if(wantsQuads && face.IsQuad())
				{
					int[] res = face.ToQuad();

					if(quads.TryGetValue(material, out polys))
						polys.AddRange(res);
					else
						quads.Add(material, new List<int>(res));
				}
				else
				{
					if(tris.TryGetValue(material, out polys))
						polys.AddRange(face.indexesInternal);
					else
						tris.Add(material, new List<int>(face.indexesInternal));
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
	}
}
