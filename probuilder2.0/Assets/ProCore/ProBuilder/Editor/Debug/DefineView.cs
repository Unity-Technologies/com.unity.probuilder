using UnityEngine;
using UnityEditor;
using System.Collections;

public class DefineView : EditorWindow
{
	[MenuItem("Tools/ProBuilder/Debug/Define View")]
	public static void Initsd()
	{
		EditorWindow.GetWindow<DefineView>().Show();
	}

	void OnGUI()
	{
		bool on = false;

#if UNITY_4_5
		GUILayout.Label("UNITY_4_5");
#endif

#if UNITY_4_6
		GUILayout.Label("UNITY_4_6");
#endif

#if UNITY_4_6_1
		GUILayout.Label("UNITY_4_6_1");
#endif

#if UNITY_5
		GUILayout.Label("UNITY_5");
#endif

#if UNITY_5_0
		GUILayout.Label("UNITY_5_0");
#endif



	}
}
