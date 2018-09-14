using System;
using System.IO;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
	[Serializable]
	sealed class Settings
	{
		public enum Scope
		{
			Project,
			User
		}

		const string k_SettingsPath = "ProjectSettings/ProBuilderSettings.json";

#if PRETTY_PRINT_JSON
		const bool k_PrettyPrintJson = true;
#else
		const bool k_PrettyPrintJson = false;
#endif

		static Settings s_Instance;

		[SerializeField]
		SettingsDictionary m_Dictionary = new SettingsDictionary();

		static Settings instance
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = new Settings();
					Load();
				}

				return s_Instance;
			}
		}

		Settings() { }

		public static void Save()
		{
			File.WriteAllText(k_SettingsPath, EditorJsonUtility.ToJson(instance, k_PrettyPrintJson));
		}

		public static void Load()
		{
			if (File.Exists(k_SettingsPath))
			{
				var json = File.ReadAllText(k_SettingsPath);
				EditorJsonUtility.FromJsonOverwrite(json, instance);
			}
		}

		internal static SettingsDictionary dictionary
		{
			get { return instance.m_Dictionary; }
		}

		static string GetEditorPrefKey<T>(string key)
		{
			return GetEditorPrefKey(typeof(T).AssemblyQualifiedName, key);
		}

		static string GetEditorPrefKey(string assemblyQualifiedTypeName, string key)
		{
			return "ProBuilder::" + assemblyQualifiedTypeName + "::" + key;
		}

		static void SetEditorPref<T>(string key, T value)
		{
			var k = GetEditorPrefKey<T>(key);
			EditorPrefs.SetString(k, ValueWrapper<T>.Serialize(value));
		}

		static T GetEditorPref<T>(string key, T fallback = default(T))
		{
			var k = GetEditorPrefKey<T>(key);
			if(EditorPrefs.HasKey(k))
				return ValueWrapper<T>.Deserialize(EditorPrefs.GetString(k));
			return fallback;
		}

		public static void Set<T>(string key, T value, Scope scope = Scope.Project)
		{
			switch (scope)
			{
				case Scope.Project:
					instance.m_Dictionary.Set<T>(key, value);
				break;

				case Scope.User:
					SetEditorPref<T>(key, value);
				break;
			}
		}

		public static void Set(string type, string key, string json, Scope scope = Scope.Project)
		{
			switch (scope)
			{
				case Scope.Project:
					instance.m_Dictionary.Set(type, key, json);
					break;

				case Scope.User:
					EditorPrefs.SetString(GetEditorPrefKey(type, key), json);
					break;
			}
		}

		public static T Get<T>(string key, Scope scope = Scope.Project, T fallback = default(T))
		{
			if(scope == Scope.Project)
				return instance.m_Dictionary.Get<T>(key, fallback);

			return GetEditorPref(key, fallback);
		}

		public static bool ContainsKey<T>(string key, Scope scope = Scope.Project)
		{
			if(scope == Scope.Project)
				return instance.m_Dictionary.ContainsKey<T>(key);

			return EditorPrefs.HasKey(GetEditorPrefKey<T>(key));
		}
	}
}
