using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
	[Serializable]
	sealed class ValueWrapper<T>
	{
#if PRETTY_PRINT_JSON
		const bool k_PrettyPrintJson = true;
#else
		const bool k_PrettyPrintJson = false;
#endif

		[SerializeField]
		T m_Value;

		public static string Serialize(T value)
		{
			var obj = new ValueWrapper<T>() { m_Value = value };
			return EditorJsonUtility.ToJson(obj, k_PrettyPrintJson);
		}

		public static T Deserialize(string json)
		{
			var value = (object)Activator.CreateInstance<ValueWrapper<T>>();
			EditorJsonUtility.FromJsonOverwrite(json, value);
			return ((ValueWrapper<T>)value).m_Value;
		}
	}

	[Serializable]
	sealed class SettingsDictionary : ISerializationCallbackReceiver
	{
		[Serializable]
		struct SettingsKeyValuePair
		{
			public string type;
			public string key;
			public string value;
		}

#pragma warning disable 0649
		[SerializeField]
		List<SettingsKeyValuePair> m_DictionaryValues;
#pragma warning restore 0649

		internal Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

		public bool ContainsKey<T>(string key)
		{
			var type = typeof(T).AssemblyQualifiedName;
			return dictionary.ContainsKey(type) && dictionary[type].ContainsKey(key);
		}

		public void Set<T>(string key, T value)
		{
			if(string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var type = typeof(T).AssemblyQualifiedName;

			Set(type, key, ValueWrapper<T>.Serialize(value));
		}

		public void Set(string type, string key, string value)
		{
			Dictionary<string, string> entries;

			if (!dictionary.TryGetValue(type, out entries))
				dictionary.Add(type, entries = new Dictionary<string, string>());

			if (entries.ContainsKey(key))
				entries[key] = value;
			else
				entries.Add(key, value);
		}

		public T Get<T>(string key, T fallback = default)
		{
			if(string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var type = typeof(T).AssemblyQualifiedName;
			Dictionary<string, string> entries;

			if (dictionary.TryGetValue(type, out entries) && entries.ContainsKey(key))
			{
				try
				{
					return ValueWrapper<T>.Deserialize(entries[key]);
				}
				catch
				{
					return fallback;
				}
			}

			return fallback;
		}

		public void OnBeforeSerialize()
		{
			m_DictionaryValues.Clear();

			foreach (var type in dictionary)
			{
				foreach (var entry in type.Value)
				{
					m_DictionaryValues.Add(new SettingsKeyValuePair()
					{
						type = type.Key,
						key = entry.Key,
						value = entry.Value
					});
				}
			}
		}

		public void OnAfterDeserialize()
		{
			dictionary.Clear();

			foreach (var entry in m_DictionaryValues)
			{
				Dictionary<string, string> entries;

				if (dictionary.TryGetValue(entry.type, out entries))
					entries.Add(entry.key, entry.value);
				else
					dictionary.Add(entry.type, new Dictionary<string, string>() { { entry.key, entry.value } } );
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	sealed class UserSettingAttribute : Attribute
	{
		string m_Category;
		GUIContent m_Title;

		public string category
		{
			get { return m_Category; }
		}

		public GUIContent title
		{
			get { return m_Title; }
		}

		public UserSettingAttribute(string category, GUIContent title)
		{
			m_Category = category;
			m_Title = title;
		}

		public UserSettingAttribute(string category, string title)
		{
			m_Category = category;
			m_Title = new GUIContent(title);
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	sealed class UserSettingBlockAttribute : Attribute
	{
		string m_Category;

		public string category
		{
			get { return m_Category; }
		}

		public UserSettingBlockAttribute(string category)
		{
			m_Category = category;
		}
	}

	interface IPref
	{
		string key { get; }
		Type type { get; }
		object boxedValue { get; }
	}

	sealed class Pref<T> : IPref
	{
		bool m_Initialized;
		string m_Key;
		T m_Value;
		Settings.Scope m_Scope;

		public Pref(string key, T value, Settings.Scope scope = Settings.Scope.Project)
		{
			m_Key = key;
			m_Value = value;
			m_Scope = scope;
		}

		public string key
		{
			get { return m_Key; }
		}

		public Type type
		{
			get { return typeof(T); }
		}

		public object boxedValue
		{
			get { return value; }
		}

		public T value
		{
			get
			{
				if (!m_Initialized)
				{
					m_Initialized = true;

					if(Settings.ContainsKey<T>(m_Key, m_Scope))
						m_Value = Settings.Get<T>(m_Key, m_Scope);
				}

				return m_Value;
			}

			set
			{
				if (Equals(m_Value, value))
					return;

				m_Value = value;
				Settings.Set<T>(key, m_Value, m_Scope);
			}
		}

		public static implicit operator T(Pref<T> pref)
		{
			return pref.value;
		}
	}

	[Serializable]
	sealed class Settings
	{
		public enum Scope
		{
			Project,
			Global
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

		static T GetEditorPref<T>(string key, T fallback = default)
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

				case Scope.Global:
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

				case Scope.Global:
					EditorPrefs.SetString(GetEditorPrefKey(type, key), json);
					break;
			}
		}

		public static T Get<T>(string key, Scope scope = Scope.Project, T fallback = default)
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
