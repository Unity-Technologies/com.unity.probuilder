using UnityEngine;

namespace ProBuilder2.Common
{
	/**
	 * A set of indices and material.
	 */
	[System.Serializable]
	public class pb_Submesh
	{
		// Indices making up this submesh. Can be points, line segments, line strip, quads, or triangles.
		public int[] indices;

		// What topology is this submesh?
		public MeshTopology topology;

		// What material does this submesh use?
		public Material material;

		/**
		 * Create new pb_Submesh. Constructor does not copy indices.
		 */
		public pb_Submesh(Material material, MeshTopology topology, int[] indices)
		{
			this.indices = indices;
			this.topology = topology;
			this.material = material;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", material != null ? material.name : "null", topology.ToString(), indices != null ? indices.Length.ToString() : "0");
		}
	}
}
