using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common
{
	/**
	 * A mesh / material(s) structure.  Mesh is destroyed with this object, materials are not.
	 */
	[System.Serializable]
	public class pb_Renderable : ScriptableObject
	{
		public Mesh mesh;
		public Material[] materials;
		public Transform transform;

		public static pb_Renderable CreateInstance(Mesh InMesh, Material[] InMaterials, Transform transform = null)
		{
			pb_Renderable ren = ScriptableObject.CreateInstance<pb_Renderable>();
			ren.mesh = InMesh;
			ren.materials = InMaterials;
			ren.transform = transform;
			return ren;
		}

		public static pb_Renderable CreateInstance(Mesh InMesh, Material InMaterial, Transform transform = null)
		{
			pb_Renderable ren = ScriptableObject.CreateInstance<pb_Renderable>();
			ren.mesh = InMesh;
			ren.materials = new Material[] { InMaterial };
			ren.transform = transform;
			return ren;
		}

		/**
		 * Create a new pb_Renderable with an empty mesh and no materials.
		 */
		public static pb_Renderable CreateInstance()
		{
			pb_Renderable ren = CreateInstance(new Mesh(), (Material)null);
			ren.mesh.name = "pb_Renderable::Mesh";
			ren.mesh.hideFlags = HideFlags.DontSave;
			ren.mesh.MarkDynamic();
			ren.hideFlags = HideFlags.DontSave;

			// ren.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			// ren.mesh.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			return ren;
		}

		/**
		 * Destructor for wireframe pb_Renderables.
		 */
		public static void DestroyInstance(UnityEngine.Object ren)
		{
			GameObject.DestroyImmediate(ren);
		}

		void OnDestroy()
		{
			if(mesh != null)
				GameObject.DestroyImmediate(mesh);
		}
	}
}
