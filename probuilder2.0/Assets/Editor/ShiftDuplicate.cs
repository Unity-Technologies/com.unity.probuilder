using UnityEngine;
using UnityEditor;
using System.Collections;
 
[InitializeOnLoad]
public class ShiftDuplicate : Editor
{
 
	static ShiftDuplicate()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}
 
	~ShiftDuplicate()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}
 
	static Object go = null;
 
	static void OnSceneGUI(SceneView scn)
	{
		Event e = Event.current;
 
		if(e.modifiers == EventModifiers.Shift && e.type == EventType.MouseDrag)
		{
			if(go == null)
			{

				go = GameObject.Instantiate(Selection.activeTransform.gameObject);
				Undo.RegisterCreatedObjectUndo(go, "Shift Duplicate");
			}
		}
 
		if(e.type == EventType.MouseUp || e.type == EventType.Ignore)
			go = null;
	}
}