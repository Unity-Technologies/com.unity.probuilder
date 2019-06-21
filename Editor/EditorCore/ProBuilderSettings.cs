using System.IO;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderSettings
    {
        const string k_PackageName = "com.unity.probuilder";
        internal const string k_DeprecatedSettingsPath = "ProjectSettings/ProBuilderSettings.json";
        static Settings s_Instance;

        [InitializeOnLoadMethod]
        static void SaveSettingsOnExit()
        {
            EditorApplication.quitting += Save;
        }

        internal static Settings instance
        {
            get
            {
                if (s_Instance == null)
                {
                    CheckForOldSettings();
                    s_Instance = new Settings(k_PackageName);
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

        static void CheckForOldSettings()
        {
            var newSettingsPath = PackageSettingsRepository.GetSettingsPath(k_PackageName);

            // Do not overwrite new settings if they exist. VCS can restore old settings file after a successful upgrade,
            // which will already be in use. In that case, just leave the old settings alone.
            if (!File.Exists(newSettingsPath) && File.Exists(k_DeprecatedSettingsPath))
            {
                try
                {
                    var directory = Path.GetDirectoryName(newSettingsPath);

                    if(!Directory.Exists(directory))
                        Directory.CreateDirectory(Path.GetDirectoryName(newSettingsPath));

                    File.Move(k_DeprecatedSettingsPath, newSettingsPath);
                }
                catch (System.Exception e)
                {
                    Log.Info("Failed moving ProBuilder settings file to new path.\n{0}", e.ToString());
                }
            }
        }
    }
}
