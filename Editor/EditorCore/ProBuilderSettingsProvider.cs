using System;
using UnityEditor;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderSettingsProvider
    {
        const string k_PreferencesPath = "Preferences/ProBuilder";

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new UserSettingsProvider(k_PreferencesPath,
                ProBuilderSettings.instance,
                new[] { typeof(ProBuilderSettingsProvider).Assembly });

            ProBuilderSettings.instance.afterSettingsSaved += () =>
            {
                if(ProBuilderEditor.instance != null)
                    ProBuilderEditor.ReloadSettings();
            };

            return provider;
        }
    }
}
