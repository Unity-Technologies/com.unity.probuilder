using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common
{
	/**
	 * A mesh / material(s) structure.
	 */
	[System.Serializable]
	public class pb_Renderable : ScriptableObject
	{
		public Mesh mesh;
		public Material[] materials;
		public Matrix4x4 matrix = Matrix4x4.identity;

		public static pb_Renderable CreateInstance(Mesh InMesh, Material[] InMaterials)
		{
			pb_Renderable ren = ScriptableObject.CreateInstance<pb_Renderable>();
			ren.mesh = InMesh;
			ren.materials = InMaterials; 
			return ren;
		}

		public static pb_Renderable CreateInstance(Mesh InMesh, Material InMaterial)
		{
			pb_Renderable ren = ScriptableObject.CreateInstance<pb_Renderable>();
			ren.mesh = InMesh;
			ren.materials = new Material[] { InMaterial };
			return ren;
		}

		/**
		 * Destroy the mesh and materials associated with this object.  Do not call Destroy() if 
		 * any of the materials or mesh is not an instance value.
		 */
		public void OnDestroy()
		{
			GameObject.DestroyImmediate(mesh);

			if(materials != null)
			{
				for(int i = 0; i < materials.Length; i++)
				{
					if(materials[i] != null)
						GameObject.DestroyImmediate(materials[i]);
				}
			}
		}
	}
}