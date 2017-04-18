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

	pb_PreferenceDictionary m_Preferences;

	void OnEnable()
	{
		m_Preferences = pb_FileUtil.LoadRequiredRelative<pb_PreferenceDictionary>("Data/ProBuilderPrefs.asset");
	}

	void OnDisable()
	{
		if(m_Preferences != null)
			EditorUtility.SetDirty(m_Preferences);
	}

	string m_FloatKey, m_IntKey, m_ColorKey;
	Color m_ColorValue;
	float m_FloatValue;
	int m_IntValue;

	void OnGUI()
	{
		foreach(var typeDic in m_Preferences)
		{
			foreach(var kvp in typeDic)
				GUILayout.Label(kvp.ToString());
		}

		GUILayout.BeginHorizontal();

			m_FloatKey = EditorGUILayout.TextField("key", m_FloatKey);

			m_FloatValue = EditorGUILayout.FloatField("", m_FloatValue);

			if(GUILayout.Button("Add"))
			{
				m_Preferences.SetFloat(m_FloatKey, m_FloatValue);
				EditorUtility.SetDirty(m_Preferences);
			}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();

			m_ColorKey = EditorGUILayout.TextField("key", m_ColorKey);
			m_ColorValue = EditorGUILayout.ColorField("", m_ColorValue);

			if(GUILayout.Button("Add"))
			{
				m_Preferences.SetColor(m_ColorKey, m_ColorValue);
				EditorUtility.SetDirty(m_Preferences);
			}
		GUILayout.EndHorizontal();

		if(GUILayout.Button("to json"))
			Debug.Log(JsonUtility.ToJson(m_Preferences));
	}

}
