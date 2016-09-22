#if SHIFT_DUPLICATE_IS_ENABLED

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

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
			if(pb_Editor.instance != null && pb_Editor.instance.editLevel == EditLevel.Geometry)
				return;

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

#endif
