using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 * Renders a list a mesh and material objects to the scene view.
	 * Caller is responsible for mesh and material memory.
	 */
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class pb_MeshRenderer : MonoBehaviour
	{
		// [HideInInspector]
		public List<pb_Renderable> renderables = new List<pb_Renderable>();

		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints 
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		readonly HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		int clamp(int val, int min, int max) { return val < min ? min : val > max ? max : val; }

		void OnRenderObject()
		{
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any 
			// game view camera isn't going to have hideflags set.
			if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			int materialIndex = 0;
			for(int i = 0; i < renderables.Count; i++)
			{
				if(renderables[i].materials == null) Debug.Log("renderables[i].materials == null -> " + name);

				Material[] mats = renderables[i].materials;

				if( renderables[i].mesh == null )
				{
					Debug.Log("renderables[i] mesh is null");
					continue;
				}

				for(int n = 0; n < renderables[i].mesh.subMeshCount; n++)
				{
					materialIndex = clamp(n, 0, mats.Length-1);

					if (mats[materialIndex] == null || !mats[materialIndex].SetPass(0) )
					{
						Debug.Log("material is null");
						continue;
					}

					Graphics.DrawMeshNow(renderables[i].mesh, renderables[i].matrix, n);
				}
			}
		}

		void OnDestroy()
		{
			renderables.Clear();
		}
	}
}