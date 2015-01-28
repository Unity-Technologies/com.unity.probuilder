using UnityEngine;
using System.Collections;

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
				_material = GetComponent<MeshRenderer>().sharedMaterial;
			return _material;
		}
	}

	private Mesh _mesh;
	private Mesh mesh 
	{
		get
		{
			if(_mesh == null)
				_mesh = GetComponent<MeshFilter>().sharedMesh;
			return _mesh;
		}
	}

	void OnRenderObject()
	{
		// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
		// this could probably even just check for one bit match, since chances are that any 
		// game view camera isn't going to have hideflags set.
		if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags )
			return;

		material.SetPass(0);
		Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity, 0);
	}
}
