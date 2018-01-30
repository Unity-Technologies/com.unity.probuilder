using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

public class ListAssemblyTypes : EditorWindow
{
	[MenuItem("Window/List Assembly Types")]
	static void Init()
	{
		EditorWindow.GetWindow<ListAssemblyTypes>(true, "Loaded Assemblies", true);
	}

	Dictionary<int, bool> m_IsExpanded = new Dictionary<int, bool>();
	Assembly[] m_Assemblies = null;
	Vector2 m_Scroll = Vector2.zero;
	string m_Filter = null;

	void OnGUI()
	{
		if(m_Assemblies == null)
		{
			m_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
		}

		m_Filter = EditorGUILayout.TextField("Filter", m_Filter);

		m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

		foreach (Assembly asm in m_Assemblies)
		{
			if (!string.IsNullOrEmpty(m_Filter) && !AssemblyContainsKeyworld(asm, m_Filter))
				continue;

			int asmHash = asm.GetHashCode();
			bool isExpanded = false;
			if (!m_IsExpanded.TryGetValue(asmHash, out isExpanded))
				m_IsExpanded.Add(asmHash, isExpanded);

			EditorGUI.BeginChangeCheck();

			isExpanded = EditorGUILayout.Foldout(isExpanded, asm.FullName);

			GUI.skin.label.richText = true;

			if (isExpanded)
			{
				foreach(Type t in asm.GetTypes())
				{
					GUI.color = string.IsNullOrEmpty(m_Filter) || t.FullName.Contains(m_Filter) ? Color.white : Color.gray;
					GUILayout.Label(string.Format("<color=#808080ff>\u2022</color> {0}", t.ToString()));
				}
			}

			GUI.color = Color.white;

			if (EditorGUI.EndChangeCheck())
			{
				m_IsExpanded[asmHash] = isExpanded;
			}
		}

		EditorGUILayout.EndScrollView();
	}

	bool AssemblyContainsKeyworld(Assembly asm, string word)
	{
		if (asm.FullName.Contains(word))
			return true;

		if (asm.GetTypes().Any(x => x.FullName.Contains(word)))
			return true;

		return false;
	}
}
