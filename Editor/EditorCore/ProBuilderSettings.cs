using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderSettings
    {
        [InitializeOnLoadMethod]
        static void SaveSettingsOnExit()
        {
            EditorApplication.quitting += () =>
            {
                Log.Info("Saving on quit");
                Save();
            };
        }

        internal const string k_DefaultSettingsPath = "ProjectSettings/ProBuilderSettings.json";

        static Settings s_Instance;

        internal static Settings instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new Settings(new ISettingsRepository[]
                    {
                        new ProjectSettingsRepository(k_DefaultSettingsPath),
                        new UserSettingsRepository(),
                    });
                }

                return s_Instance;
            }
        }

        public static void Save()
        {
            instance.Save();
        }

        public static void Set<T>(string key, T value, SettingsScope scope = SettingsScope.Project)
        {
            instance.Set<T>(key, value, scope);
        }

        public static T Get<T>(string key, SettingsScope scope = SettingsScope.Project, T fallback = default(T))
        {
            return instance.Get<T>(key, scope, fallback);
        }

        public static bool ContainsKey<T>(string key, SettingsScope scope = SettingsScope.Project)
        {
            return instance.ContainsKey<T>(key, scope);
        }

        public static void Delete<T>(string key, SettingsScope scope = SettingsScope.Project)
        {
            instance.DeleteKey<T>(key, scope);
        }
    }
}
