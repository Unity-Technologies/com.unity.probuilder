using UnityEngine;
using UnityEditor;
using ProCore.Common;
using System.Collections;

public class SettingsInterface : EditorWindow
{
	[MenuItem("Tools/SixBySeven Shared  Interface")]
	public static void init()
	{
		EditorWindow.GetWindow(typeof(SettingsInterface), true, "Settings", true);
	}

	public void OnGUI()
	{
		SharedProperties.snapEnabled = EditorGUILayout.Toggle("Snap Enabled", SharedProperties.snapEnabled);
		SharedProperties.snapValue = EditorGUILayout.FloatField("Snap Value", SharedProperties.snapValue);
		SharedProperties.useAxisConstraints = EditorGUILayout.Toggle("Use Axis Constraints", SharedProperties.useAxisConstraints);

		GUILayout.Label("Snap Enabled: " + SharedProperties.snapEnabled);
		GUILayout.Label("Snap Value: " + SharedProperties.snapValue);
		GUILayout.Label("Use Axis Constraints: " + SharedProperties.useAxisConstraints);
	}
}
