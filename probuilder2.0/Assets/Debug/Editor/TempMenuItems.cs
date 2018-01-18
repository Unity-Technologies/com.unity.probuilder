using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using ProBuilder.EditorCore;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{

//		profiler.Begin("Refresh(force = true)");
//
//		for (int i = 0; i < 100; i++)
//		{
//			pb_Editor.Refresh(true);
//		}
//
//		profiler.End();
	}

	void OnGUI()
	{
	}

	static int m_Selected = 0;

	static void DoSceneThing(SceneView view)
	{
//		Handles.ClearHandles();
//		HandleUtility.s_CustomPickDistance = HandleUtility.kPickDistance;
//		EditorGUIUtility.ResetGUIState();
//		pb_Reflection.Invoke(null, typeof(EditorGUIUtility), "ResetGUIState");
//		GUI.color = Color.white;

		Handles.BeginGUI();

		m_Selected = GUILayout.Toolbar(m_Selected, new string[]
		{
			"Test",
			"Second",
			"Third"
		});

		if (GUILayout.Button("Exit"))
			SceneView.onSceneGUIDelegate -= DoSceneThing;

		Handles.EndGUI();
	}

	public static void SaveMeshTemplate(Mesh mesh)
	{
		StackTrace trace = new StackTrace(1, true);
		for (int i = 0; i < trace.FrameCount; i++)
		{
			StackFrame first = trace.GetFrame(i);
			UnityEngine.Debug.Log(first.GetFileName() + ": " + first.GetMethod());
		}
	}
}
