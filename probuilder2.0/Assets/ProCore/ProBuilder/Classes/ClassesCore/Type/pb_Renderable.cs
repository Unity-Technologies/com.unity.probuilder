using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common
{
	/**
	 * A mesh / material(s) structure.
	 */
	public class pb_Renderable
	{
		public Mesh mesh;
		public Material[] materials;

		public pb_Renderable(Mesh InMesh, Material[] InMaterials)
		{
			this.mesh = InMesh;
			this.materials = InMaterials; 
		}

		public pb_Renderable(Mesh InMesh, Material InMaterial)
		{
			this.mesh = InMesh;
			this.materials = new Material[] { InMaterial };
		}

		/**
		 * Destroy the mesh and materials associated with this object.  Do not call Destroy() if 
		 * any of the materials or mesh is not an instance value.
		 */
		public void Destroy()
		{
			GameObject.DestroyImmediate(mesh);

			for(int i = 0; i < materials.Length; i++)
				GameObject.DestroyImmediate(materials[i]);
		}
	}
}