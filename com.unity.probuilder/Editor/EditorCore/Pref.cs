using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    sealed class Pref<T> : UserSetting<T>
    {
        public Pref(string key, T value, SettingsScope scope = SettingsScope.Project)
            : base(ProBuilderSettings.instance, key, value, scope)
        {}

        public Pref(Settings settings, string key, T value, SettingsScope scope = SettingsScope.Project)
            : base(settings, key, value, scope) {}
    }
}
