using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

sealed class ProBuilderSettingsProvider : SettingsProvider
{
	List<string> m_Categories;
	Dictionary<string, List<SimpleTuple<GUIContent, IPref>>> m_Settings;
	Dictionary<string, List<MethodInfo>> m_SettingBlocks;

	[SettingsProvider]
	static SettingsProvider CreateSettingsProvider()
	{
		return new ProBuilderSettingsProvider("Preferences/ProBuilder");
	}

	public ProBuilderSettingsProvider(string path, SettingsScopes scopes = SettingsScopes.Any)
		: base(path, scopes)
	{
		SearchForSettingAttributes();
	}

	void SearchForSettingAttributes()
	{
		var fields = GetType().Assembly.GetTypes()
			.SelectMany(x =>
				x.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					.Where(prop => Attribute.IsDefined(prop, typeof(UserSettingAttribute))));

		var methods = GetType().Assembly.GetTypes()
			.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(y => Attribute.IsDefined(y, typeof(UserSettingBlockAttribute))));

		m_Settings = new Dictionary<string, List<SimpleTuple<GUIContent, IPref>>>();

		m_SettingBlocks = new Dictionary<string, List<MethodInfo>>();

		foreach (var field in fields)
		{
			if (!field.IsStatic)
			{
				Log.Warning("Cannot create setting entries for instance fields. Skipping \"" + field.Name + "\".");
				continue;
			}

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
		m_Categories.Sort();
	}

	public override void OnGUI(string searchContext)
	{
		EditorGUIUtility.labelWidth = 200;

		foreach (var key in m_Categories)
		{
			GUILayout.Label(key, EditorStyles.boldLabel);

			List<SimpleTuple<GUIContent, IPref>> settings;

			if (m_Settings.TryGetValue(key, out settings))
				foreach(var setting in settings)
					DoPreferenceField(setting.item1, setting.item2);

			List<MethodInfo> blocks;

			if (m_SettingBlocks.TryGetValue(key, out blocks))
				foreach (var block in blocks)
					block.Invoke(null, null);

			GUILayout.Space(8);
		}

		EditorGUIUtility.labelWidth = 0;
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
		else if (pref is Pref<bool>)
		{
			var cast = (Pref<bool>) pref;
			cast.value = EditorGUILayout.Toggle(title, cast.value);
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
		else if (typeof(Enum).IsAssignableFrom(pref.type))
		{
			Enum val = (Enum) pref.GetValue();
			EditorGUI.BeginChangeCheck();
			if(pref.type.GetCustomAttribute<FlagsAttribute>() != null)
				val = EditorGUILayout.EnumFlagsField(title, val);
			else
				val = EditorGUILayout.EnumPopup(title, val);
			if (EditorGUI.EndChangeCheck())
				pref.SetValue(val);
		} else if (typeof(UnityEngine.Object).IsAssignableFrom(pref.type))
		{
			var obj = (UnityEngine.Object) pref.GetValue();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.ObjectField(title, obj, pref.type, false);
			if(EditorGUI.EndChangeCheck())
				pref.SetValue(obj);
		}
		else
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(title, GUILayout.Width(EditorGUIUtility.labelWidth - EditorStyles.label.margin.right * 2));
			var obj = pref.GetValue();
			GUILayout.Label(obj == null ? "null" : pref.GetValue().ToString());
			GUILayout.EndHorizontal();
		}
	}
}