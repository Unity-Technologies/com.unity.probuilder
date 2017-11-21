using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// A set of indices and material.
	/// </summary>
	[System.Serializable]
	public class pb_Submesh
	{
		/// <summary>
		/// Indices making up this submesh. Can be triangles or quads.
		/// </summary>
		public int[] indices;

		/// <summary>
		/// What topology is this submesh?
		/// </summary>
		public MeshTopology topology;

		/// <summary>
		/// What material does this submesh use?
		/// </summary>
		public Material material;

		/// <summary>
		/// Create new pb_Submesh. Constructor does not copy indices.
		/// </summary>
		/// <param name="material"></param>
		/// <param name="topology"></param>
		/// <param name="indices"></param>
		public pb_Submesh(Material material, MeshTopology topology, int[] indices)
		{
			this.indices = indices;
			this.topology = topology;
			this.material = material;
		}

		/// <summary>
		/// Create new pb_Submesh from a mesh, submesh index and material.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="subMeshIndex"></param>
		/// <param name="material"></param>
		public pb_Submesh(Mesh mesh, int subMeshIndex, Material material)
		{
			this.indices = mesh.GetIndices(subMeshIndex);
			this.topology = mesh.GetTopology(subMeshIndex);
			this.material = material;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", material != null ? material.name : "null", topology.ToString(), indices != null ? indices.Length.ToString() : "0");
		}
	}
}
