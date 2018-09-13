using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

public class SettingsWindow : EditorWindow
{
	List<string> m_Categories;
	Dictionary<string, List<SimpleTuple<GUIContent, IPref>>> m_Settings;
	Dictionary<string, List<MethodInfo>> m_SettingBlocks;

	[MenuItem("Window/ProBuilder Settings")]
	static void Init()
	{
		GetWindow<SettingsWindow>();
	}

	void OnEnable()
	{
		var fields = typeof(SettingsWindow).Assembly.GetTypes()
			.SelectMany(x =>
				x.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					.Where(prop => Attribute.IsDefined(prop, typeof(UserSettingAttribute))));

		var methods = typeof(SettingsWindow).Assembly.GetTypes()
			.SelectMany(x => x.GetMethods()
				.Where(y => Attribute.IsDefined(y, typeof(UserSettingBlockAttribute))));

		m_Settings = new Dictionary<string, List<SimpleTuple<GUIContent, IPref>>>();

		m_SettingBlocks = new Dictionary<string, List<MethodInfo>>();

		foreach (var field in fields)
		{
			var attrib = (UserSettingAttribute)field.GetCustomAttribute(typeof(UserSettingAttribute));
			var pref = (IPref)field.GetValue(null);
			var category = string.IsNullOrEmpty(attrib.category) ? "Uncategorized" : attrib.category;

			List<SimpleTuple<GUIContent, IPref>> settings;

			if (m_Settings.TryGetValue(category, out settings))
				settings.Add(new SimpleTuple<GUIContent, IPref>(attrib.title, pref));
			else
				m_Settings.Add(category, new List<SimpleTuple<GUIContent, IPref>>() { new SimpleTuple<GUIContent, IPref>(attrib.title, pref) });
		}

		foreach (var method in methods)
		{
			var attrib = (UserSettingBlockAttribute)method.GetCustomAttribute(typeof(UserSettingBlockAttribute));
			var category = string.IsNullOrEmpty(attrib.category) ? "Uncategorized" : attrib.category;
			List<MethodInfo> blocks;

			if (m_SettingBlocks.TryGetValue(category, out blocks))
				blocks.Add(method);
			else
				m_SettingBlocks.Add(category, new List<MethodInfo>() { method });
		}

		m_Categories = m_Settings.Keys.Union(m_SettingBlocks.Keys).ToList();
	}

	void OnGUI()
	{
		foreach (var key in m_Categories)
		{
			GUILayout.Label(key, EditorStyles.boldLabel);

			List<MethodInfo> blocks;

			if (m_SettingBlocks.TryGetValue(key, out blocks))
				foreach (var block in blocks)
					block.Invoke(null, null);

			List<SimpleTuple<GUIContent, IPref>> settings;

			if (m_Settings.TryGetValue(key, out settings))
				foreach(var setting in settings)
					DoPreferenceField(setting.item1, setting.item2);
		}
	}

	void DoPreferenceField(GUIContent title, IPref pref)
	{
		if (pref is Pref<float>)
		{
			var cast = (Pref<float>) pref;
			cast.value = EditorGUILayout.FloatField(title, cast.value);
		}
		else if (pref is Pref<int>)
		{
			var cast = (Pref<int>) pref;
			cast.value = EditorGUILayout.IntField(title, cast.value);
		}
		else if (pref is Pref<string>)
		{
			var cast = (Pref<string>) pref;
			cast.value = EditorGUILayout.TextField(title, cast.value);
		}
		else if (pref is Pref<Color>)
		{
			var cast = (Pref<Color>) pref;
			cast.value = EditorGUILayout.ColorField(title, cast.value);
		}
		else
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(title);
			GUILayout.Label(pref.boxedValue.ToString());
			GUILayout.EndHorizontal();
		}
	}
}