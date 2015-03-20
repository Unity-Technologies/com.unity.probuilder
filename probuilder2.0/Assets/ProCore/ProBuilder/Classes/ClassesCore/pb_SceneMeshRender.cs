using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common
{
	[ExecuteInEditMode]
	public class pb_SceneMeshRender : MonoBehaviour
	{
		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints 
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		private Material _material;
		private Material material 
		{
			get
			{
				if(_material == null)
				{
					MeshRenderer mr = GetComponent<MeshRenderer>();
					if(mr != null) _material = mr.sharedMaterial;
				}

				return _material;
			}
		}

		private Mesh _mesh;
		private Mesh mesh 
		{
			get
			{
				if(_mesh == null)
				{
					MeshFilter mf = GetComponent<MeshFilter>();
					_mesh = mf.sharedMesh;
				}

				return _mesh;
			}
		}

		void OnDestroy()
		{
			if(_mesh) DestroyImmediate(_mesh);
			if(_material) DestroyImmediate(_material);
		}

		void OnRenderObject()
		{
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any 
			// game view camera isn't going to have hideflags set.
			if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			Mesh msh = mesh;
			Material mat = material;

			if(mat == null || msh == null)
			{
				DestroyImmediate(gameObject);
				return;
			}

			material.SetPass(0);
			Graphics.DrawMeshNow(msh, Vector3.zero, Quaternion.identity, 0);
		}
	}
}