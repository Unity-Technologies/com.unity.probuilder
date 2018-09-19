using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.Assertions;
using UnityEngine.ProBuilder;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder.MeshOperations;

class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d", false, 1000)]
	static void MenuInit()
	{

		GetWindow<TempMenuItems>();
	}

	Vector2 m_Scroll;

	void OnGUI()
	{
		SelectMode all = SelectMode.Any;
		SelectMode faceAndEdge = SelectMode.Face | SelectMode.Edge;
		var allMatchesFace = SelectMode.Face.ContainsFlag(all);
		var allMatchesFaceAndEdge = SelectMode.Face.ContainsFlag(faceAndEdge);
		var faceMatchesFace = SelectMode.Face.ContainsFlag(SelectMode.Face);

		m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
		foreach (var item in EditorToolbarLoader.GetActions())
		{
			GUILayout.Label(item.menuTitle + "(" + ProBuilderEditor.selectMode.ContainsFlag(item.validSelectModes) + ")   [" + item.validSelectModes + "]");
		}
		EditorGUILayout.EndScrollView();
	}

	public static void SaveMeshTemplate(Mesh mesh)
	{
//		StackTrace trace = new StackTrace(1, true);
//		for (int i = 0; i < trace.FrameCount; i++)
//		{
//			StackFrame first = trace.GetFrame(i);
//			UnityEngine.Debug.Log(first.GetFileName() + ": " + first.GetMethod());
//		}
	}
}
