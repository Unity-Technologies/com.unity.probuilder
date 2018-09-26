#define PRETTY_PRINT_JSON

using System;
using System.IO;
using UnityEngine;

namespace UnityEditor.Settings
{
	public enum SettingScope
	{
		/// <value>
		/// Setting will be stored in ProjectSettings/k_SettingsPath.
		/// </value>
		Project,

		/// <value>
		/// Setting will be stored in EditorPrefs.
		/// </value>
		User
	}

	[Serializable]
	public sealed class Settings
	{
		[SerializeField]
		string m_SettingsPath;

#if PRETTY_PRINT_JSON
		const bool k_PrettyPrintJson = true;
#else
		const bool k_PrettyPrintJson = false;
#endif

		[SerializeField]
		SettingsDictionary m_Dictionary = new SettingsDictionary();

		Settings() {}

		public Settings(string settingsPath)
		{
			m_SettingsPath = settingsPath;
			Load();
		}

		internal string settingsPath
		{
			get { return m_SettingsPath; }
			set { m_SettingsPath = value; }
		}

		public void Save()
		{
			File.WriteAllText(m_SettingsPath, EditorJsonUtility.ToJson(this, k_PrettyPrintJson));
		}

		public void Load()
		{
			if (File.Exists(m_SettingsPath))
			{
				m_Dictionary = null;
				var json = File.ReadAllText(m_SettingsPath);
				EditorJsonUtility.FromJsonOverwrite(json, this);
			}
		}

		public void Reload()
		{
			m_Dictionary = null;
			m_Dictionary = new SettingsDictionary();
			Load();
		}

		internal SettingsDictionary dictionary
		{
			get { return m_Dictionary; }
		}

		static string GetEditorPrefKey<T>(string key)
		{
			return GetEditorPrefKey(typeof(T).FullName, key);
		}

		static string GetEditorPrefKey(string fullName, string key)
		{
			return fullName + "::" + key;
		}

		static void SetEditorPref<T>(string key, T value)
		{
			var k = GetEditorPrefKey<T>(key);

			if(typeof(T) == typeof(string))
			 	EditorPrefs.SetString(k, (string) (object) value);
			else if(typeof(T) == typeof(bool))
			 	EditorPrefs.SetBool(k, (bool) (object) value);
			else if(typeof(T) == typeof(float))
			 	EditorPrefs.SetFloat(k, (float) (object) value);
			else if(typeof(T) == typeof(int))
			 	EditorPrefs.SetInt(k, (int) (object) value);
			else
				EditorPrefs.SetString(k, ValueWrapper<T>.Serialize(value));
		}

		static T GetEditorPref<T>(string key, T fallback = default(T))
		{
			var k = GetEditorPrefKey<T>(key);

			if(!EditorPrefs.HasKey(k))
				return fallback;

			var o = (object) fallback;

			if(typeof(T) == typeof(string))
				o = EditorPrefs.GetString(k, (string) o);
			else if(typeof(T) == typeof(bool))
				o = EditorPrefs.GetBool(k, (bool) o);
			else if(typeof(T) == typeof(float))
				o = EditorPrefs.GetFloat(k, (float) o);
			else if(typeof(T) == typeof(int))
				o = EditorPrefs.GetInt(k, (int) o);
			else
				return ValueWrapper<T>.Deserialize(EditorPrefs.GetString(k));

			return (T) o;
		}

		public void Set<T>(string key, T value, SettingScope scope = SettingScope.Project)
		{
			switch (scope)
			{
				case SettingScope.Project:
					m_Dictionary.Set<T>(key, value);
				break;

				case SettingScope.User:
					SetEditorPref<T>(key, value);
				break;
			}
		}

		public T Get<T>(string key, SettingScope scope = SettingScope.Project, T fallback = default(T))
		{
			if(scope == SettingScope.Project)
				return m_Dictionary.Get<T>(key, fallback);

			return GetEditorPref(key, fallback);
		}

		public bool ContainsKey<T>(string key, SettingScope scope = SettingScope.Project)
		{
			if(scope == SettingScope.Project)
				return m_Dictionary.ContainsKey<T>(key);

			return EditorPrefs.HasKey(GetEditorPrefKey<T>(key));
		}

		public void Delete<T>(string key, SettingScope scope = SettingScope.Project)
		{
			if (scope == SettingScope.Project)
			{
				m_Dictionary.Remove<T>(key);
			}
			else
			{
				var k = GetEditorPrefKey<T>(key);

				if(EditorPrefs.HasKey(k))
					EditorPrefs.DeleteKey(k);
			}
		}

		public void DeleteProjectSettings()
		{
			if (File.Exists(m_SettingsPath))
				File.Delete(m_SettingsPath);
		}
	}
}
