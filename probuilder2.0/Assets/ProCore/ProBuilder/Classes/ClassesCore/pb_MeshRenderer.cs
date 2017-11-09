using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/// <summary>
	/// Renders a list a mesh and material objects to the scene view.
	/// </summary>
	/// <remarks>
	/// Caller is responsible for mesh and material memory.
	/// </remarks>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	class pb_MeshRenderer : pb_MonoBehaviourSingleton<pb_MeshRenderer>
	{
		[SerializeField] private HashSet<pb_Renderable> m_Renderables = new HashSet<pb_Renderable>();

		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		readonly HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		int clamp(int val, int min, int max) { return val < min ? min : val > max ? max : val; }

		public static void Add(pb_Renderable renderable)
		{
			instance.m_Renderables.Add(renderable);
		}

		public static void Remove(pb_Renderable renderable)
		{
			if(instance.m_Renderables.Contains(renderable))
				instance.m_Renderables.Remove(renderable);
		}

		void OnRenderObject()
		{
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any
			// game view camera isn't going to have hideflags set.
			if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			int materialIndex = 0;

			foreach(pb_Renderable renderable in m_Renderables)
			{
				if(renderable.materials == null) Debug.Log("renderable.materials == null -> " + name);

				Material[] mats = renderable.materials;

				if( renderable.mesh == null )
				{
					Debug.Log("renderable mesh is null");
					continue;
				}

				for(int n = 0; n < renderable.mesh.subMeshCount; n++)
				{
					materialIndex = clamp(n, 0, mats.Length-1);

					if (mats[materialIndex] == null || !mats[materialIndex].SetPass(0) )
					{
						Debug.Log("material is null");
						continue;
					}

					Graphics.DrawMeshNow(renderable.mesh, renderable.transform != null ? renderable.transform.localToWorldMatrix : Matrix4x4.identity, n);
				}
			}
		}

		void OnDestroy()
		{
			foreach(pb_Renderable ren in m_Renderables)
				GameObject.DestroyImmediate(ren);
		}
	}
}
