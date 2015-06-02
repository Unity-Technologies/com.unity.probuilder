using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.Interface;

[CustomEditor(typeof(MeshVisualizer))]
public class MeshVisualizerEditor : Editor {

	Mesh mesh;
	Transform transform;

	void OnEnable()
	{
		transform = ((MeshVisualizer)target).transform;
		mesh = transform.gameObject.GetComponent<MeshFilter>().sharedMesh;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	GUIContent gc = new GUIContent();

	void OnSceneGUI()
	{
		int[] t = mesh.GetIndices(0);
		Vector3[] v = mesh.vertices;

		Dictionary<pb_IntVec3, List<int>> verts = new Dictionary<pb_IntVec3, List<int>>();

		for(int i = 0; i < t.Length; i++)
		{
			List<int> val;
			if( verts.TryGetValue(v[t[i]], out val) )
				val.Add(i);//t[i]);
			else
				verts.Add(v[t[i]], new List<int>() { i });//t[i] });
		}

		Handles.BeginGUI();

		foreach(KeyValuePair<pb_IntVec3, List<int>> kvp in verts)
		{
			Vector3 world = transform.TransformPoint(kvp.Key);
			Vector2 screen = HandleUtility.WorldToGUIPoint(world);

			gc.text = kvp.Value.ToFormattedString(",");
			DrawSceneLabel(gc, screen);
		}
		Handles.EndGUI();
	}

	void DrawSceneLabel(GUIContent content, Vector2 position)
	{
		float width = EditorStyles.boldLabel.CalcSize(content).x;
		float height = EditorStyles.label.CalcHeight(content, width) + 4;

		pb_GUI_Utility.DrawSolidColor( new Rect(position.x, position.y, width, height), Color.black);
		GUI.Label( new Rect(position.x, position.y, width, height), content, EditorStyles.boldLabel );
	}
}
