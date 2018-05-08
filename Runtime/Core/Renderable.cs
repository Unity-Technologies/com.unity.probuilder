using UnityEngine;
using System.Collections;

namespace UnityEngine.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// A mesh / material(s) structure. Mesh is destroyed with this object, materials are not.
	/// </summary>
	[System.Serializable]
	sealed class Renderable : ScriptableObject
	{
		public Mesh mesh;
		public Material material;
		public Transform transform;

		public static Renderable CreateInstance(Mesh InMesh, Material InMaterial, Transform transform = null)
		{
			Renderable ren = ScriptableObject.CreateInstance<Renderable>();
			ren.mesh = InMesh;
			ren.material = InMaterial;
			ren.transform = transform;
			return ren;
		}

		/// <summary>
		/// Create a new pb_Renderable with an empty mesh and no materials.
		/// </summary>
		/// <returns></returns>
		public static Renderable CreateInstance()
		{
			Renderable ren = CreateInstance(new Mesh(), (Material)null);
			ren.mesh.name = "pb_Renderable::Mesh";
			ren.mesh.hideFlags = HideFlags.DontSave;
			ren.mesh.MarkDynamic();
			ren.hideFlags = HideFlags.DontSave;

			// ren.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			// ren.mesh.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			return ren;
		}

		/// <summary>
		/// Destructor for wireframe pb_Renderables.
		/// </summary>
		/// <param name="ren"></param>
		public static void DestroyInstance(UnityEngine.Object ren)
		{
			DestroyImmediate(ren);
		}

		void OnDestroy()
		{
			if(mesh != null)
				DestroyImmediate(mesh);
		}

		public void Render()
		{
			if (mesh == null)
				return;

			for (int n = 0, c = mesh.subMeshCount; n < c; n++)
			{
				if (material == null || !material.SetPass(0))
					continue;

				Graphics.DrawMeshNow(mesh, transform != null ? transform.localToWorldMatrix : Matrix4x4.identity, n);
			}
		}
	}
}
