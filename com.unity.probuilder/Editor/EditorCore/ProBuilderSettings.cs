using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
	static class ProBuilderSettings
	{
		internal const string k_DefaultSettingsPath = "ProjectSettings/ProBuilderSettings.json";

		static Settings s_Instance;

		internal static Settings instance
		{
			get
			{
				if (s_Instance == null)
					s_Instance = new Settings(k_DefaultSettingsPath);

				return s_Instance;
			}
		}

		public static void Save()
		{
			instance.Save();
		}

		public static void Load()
		{
			instance.Load();
		}

		public static void Reload()
		{
			instance.Reload();
		}

		public static void Set<T>(string key, T value, SettingScope scope = SettingScope.Project)
		{
			instance.Set<T>(key, value, scope);
		}

		public static T Get<T>(string key, SettingScope scope = SettingScope.Project, T fallback = default(T))
		{
			return instance.Get<T>(key, scope, fallback);
		}

		public static bool ContainsKey<T>(string key, SettingScope scope = SettingScope.Project)
		{
			return instance.ContainsKey<T>(key, scope);
		}

		public static void Delete<T>(string key, SettingScope scope = SettingScope.Project)
		{
			instance.Delete<T>(key, scope);
		}
	}
}
