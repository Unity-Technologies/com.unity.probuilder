using UnityEngine;
using UnityEditor;
using ProBuilder.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;

public class pb_SceneExplorer : EditorWindow
{
	[MenuItem("Window/Scene Explorer")]
	public static void MenuInitSceneExplorer()
	{
		GetWindow<pb_SceneExplorer>(true, "Scene Explorer", true).Show();
	}

	void OnEnable()
	{
		m_SearchField = new SearchField();
		OnHierarchyChange();
	}

	pb_Tuple<string, bool, Object[]>[] m_Objects = new pb_Tuple<string, bool, Object[]>[]
	{
		new pb_Tuple<string, bool, Object[]>("textures: ", false, null),
		new pb_Tuple<string, bool, Object[]>("audioclips: ", false, null),
		new pb_Tuple<string, bool, Object[]>("meshes: ", false, null),
		new pb_Tuple<string, bool, Object[]>("materials: ", false, null),
		new pb_Tuple<string, bool, Object[]>("gameobjects: ", false, null),
		new pb_Tuple<string, bool, Object[]>("components: ", false, null)
	};

	Vector2 m_Scroll = Vector2.zero;
	SearchField m_SearchField;
	[SerializeField]
	string m_SearchPattern;
	int m_Count = 0;

	void OnGUI()
	{
		if(EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			return;

		if(GUILayout.Button("Clean Unused Assets"))
		{
			EditorUtility.UnloadUnusedAssetsImmediate();
			OnHierarchyChange();
		}

		Rect searchRect = GUILayoutUtility.GetRect(new GUIContent(""), EditorStyles.textField, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));

		EditorGUI.BeginChangeCheck();
		m_SearchPattern = m_SearchField.OnGUI(searchRect, m_SearchPattern);
		if(EditorGUI.EndChangeCheck())
			OnHierarchyChange();

		m_Scroll = GUILayout.BeginScrollView(m_Scroll);

		GUILayout.Label("All: " + m_Count, EditorStyles.boldLabel);

		GUILayout.Space(12);

		GUI.skin.label.richText = true;

		bool doReload = false;

		EditorGUI.BeginChangeCheck();

		for (int i = 0; i < m_Objects.Length; i++)
		{
			m_Objects[i].Item2 = EditorGUILayout.Foldout(m_Objects[i].Item2, m_Objects[i].Item1 + m_Objects[i].Item3.Length);

			if(m_Objects[i].Item2)
				DrawObjectArray(m_Objects[i].Item3);
		}

		if (EditorGUI.EndChangeCheck())
			doReload = true;

		GUILayout.EndScrollView();

		if (doReload)
		{
			OnHierarchyChange();
			EditorGUIUtility.ExitGUI();
		}
	}

	void DrawObjectArray(Object[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("<color=#808080ff>\u2022</color> {0}", array[i] != null ? array[i].ToString() : "null"));
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("x", EditorStyles.miniButtonRight))
				Object.DestroyImmediate(array[i]);
			GUILayout.EndHorizontal();
		}
	}

	void OnHierarchyChange()
	{
		Object[] textures = Resources.FindObjectsOfTypeAll(typeof(Texture));
		Object[] audioclips = Resources.FindObjectsOfTypeAll(typeof(AudioClip));
		Object[] meshes = Resources.FindObjectsOfTypeAll(typeof(Mesh));
		Object[] materials = Resources.FindObjectsOfTypeAll(typeof(Material));
		Object[] gameobjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
		Object[] components = Resources.FindObjectsOfTypeAll(typeof(Component));

		if (m_SearchPattern == null)
			m_SearchPattern = "";

		m_Objects[0].Item3 = textures.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[1].Item3 = audioclips.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[2].Item3 = meshes.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[3].Item3 = materials.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[4].Item3 = gameobjects.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[5].Item3 = components.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();

		m_Count = m_Objects[0].Item3.Length +
		          m_Objects[1].Item3.Length +
		          m_Objects[2].Item3.Length +
		          m_Objects[3].Item3.Length +
		          m_Objects[4].Item3.Length +
		          m_Objects[5].Item3.Length;

		Repaint();
	}
}
