using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(VisualizeMesh))]
public class VisualizeMeshEditor : Editor
{
	Mesh m;
	VisualizeMesh vm;
	void OnEnable()
	{
		vm = ((VisualizeMesh)target);
	}

	void OnSceneGUI()
	{
		m = vm.GetComponent<MeshFilter>().sharedMesh;
		if(m == null) return;

		Handles.BeginGUI();
		Vector3 p = Vector3.zero;

		if(vm.showTriangles)
		{
			int i = 0;
			foreach(Vector3 v in m.vertices)
			{
				p = ((VisualizeMesh)target).transform.TransformPoint(v);

				Handles.Label(p, (i++).ToString());
			}
		}

		Handles.EndGUI();
	}
}
