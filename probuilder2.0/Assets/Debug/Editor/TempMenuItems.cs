using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System;
using System.Reflection;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		EditorWindow.GetWindow<TempMenuItems>().Show();
	}

	pb_PreferencesObject m_Preferences;

	void OnEnable()
	{
		m_Preferences = pb_FileUtil.LoadRelative<pb_PreferencesObject>("Preferences/ProBuilderPrefs.asset");
	}

	void OnDisable()
	{
		EditorUtility.SetDirty(m_Preferences);
	}

	string m_FloatKey, m_IntKey;
	float m_FloatValue;
	int m_IntValue;

	void OnGUI()
	{

		foreach(var kvp in m_Preferences)
		{
			GUILayout.Label(kvp.Key.ToString(), EditorStyles.boldLabel);

			foreach(var entry in kvp.Value)
			{
				GUILayout.Label("Key: " + entry.Key);
				GUILayout.Label("Value: " + entry.Value);
			}
		}

		GUILayout.BeginHorizontal();

			m_FloatKey = EditorGUILayout.TextField("key", m_FloatKey);

			m_FloatValue = EditorGUILayout.FloatField("", m_FloatValue);

			if(GUILayout.Button("Add"))
			{
				m_Preferences.Set(m_FloatKey, m_FloatValue);
				EditorUtility.SetDirty(m_Preferences);
			}
		GUILayout.EndHorizontal();

		// GUILayout.Label("Stored Ints", EditorStyles.boldLabel);

		// foreach(var kvp in m_Preferences.intValues)
		// 	GUILayout.Label(kvp.Key + " : " + kvp.Value);

		if(GUILayout.Button("to json"))
			Debug.Log(JsonUtility.ToJson(m_Preferences));

	}
}
