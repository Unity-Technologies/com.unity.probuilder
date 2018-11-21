using System;
using UnityEditor;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderSettingsProvider
    {
        const string k_PreferencesPath = "Preferences/ProBuilder";

#if UNITY_2018_3_OR_NEWER
        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new UserSettingsProvider(k_PreferencesPath,
                    ProBuilderSettings.instance,
                    new[] { typeof(ProBuilderSettingsProvider).Assembly });

            ProBuilderSettings.instance.afterSettingsSaved += () =>
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.ReloadSettings();
                };

            return provider;
        }

#else

        [NonSerialized]
        static UserSettingsProvider s_SettingsProvider;

        [PreferenceItem("ProBuilder")]
        static void ProBuilderPreferencesGUI()
        {
            if (s_SettingsProvider == null)
            {
                s_SettingsProvider = new UserSettingsProvider(ProBuilderSettings.instance, new[] { typeof(ProBuilderSettingsProvider).Assembly });

                ProBuilderSettings.instance.afterSettingsSaved += () =>
                    {
                        if (ProBuilderEditor.instance != null)
                            ProBuilderEditor.ReloadSettings();
                    };
            }

            s_SettingsProvider.OnGUI(null);
        }

#endif
    }
}
