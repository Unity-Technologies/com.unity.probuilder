using UnityEditor.SettingsManagement;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    static class Experimental
    {
        const string k_ExperimentalFeaturesEnabled = "PROBUILDER_EXPERIMENTAL_FEATURES";

        [UserSetting]
        static Pref<bool> s_experimentalFeatureEnabled = new Pref<bool>("experimental.enabled", false, SettingsScope.Project);

        [UserSetting]
        static Pref<bool> s_MeshesAreAssets = new Pref<bool>("experimental.meshesAreAssets", false, SettingsScope.Project);

        internal static bool meshesAreAssets
        {
            get { return experimentalFeaturesEnabled && s_MeshesAreAssets; }
        }

        internal static bool experimentalFeaturesEnabled
        {
#if PROBUILDER_EXPERIMENTAL_FEATURES
            get { return true; }
#else
            get { return false; }
#endif
        }

        static Experimental()
        {
            if(s_experimentalFeatureEnabled.value != experimentalFeaturesEnabled)
            {
                s_experimentalFeatureEnabled.value = experimentalFeaturesEnabled;
                ProBuilderSettings.Save();
            }
        }

        public static void AfterSettingsSaved()
        {
#if PROBUILDER_EXPERIMENTAL_FEATURES
            if(!s_experimentalFeatureEnabled.value)
                ScriptingSymbolManager.RemoveScriptingDefine(k_ExperimentalFeaturesEnabled);
#else
            if(s_experimentalFeatureEnabled.value)
                ScriptingSymbolManager.AddScriptingDefine(k_ExperimentalFeaturesEnabled);
#endif
        }

        [UserSettingBlock("Experimental")]
        static void ExperimentalFeaturesSettings(string searchContext)
        {
            var enabled = experimentalFeaturesEnabled;

            EditorGUILayout.HelpBox("Enabling Experimental Features will cause Unity to recompile scripts.", MessageType.Warning);
            s_experimentalFeatureEnabled.value = SettingsGUILayout.SearchableToggle("Experimental Features Enabled", enabled, searchContext);

            if(s_experimentalFeatureEnabled.value)
            {
                using (new SettingsGUILayout.IndentedGroup())
                {
                    s_MeshesAreAssets.value = SettingsGUILayout.SettingsToggle("Store Mesh as Asset", s_MeshesAreAssets, searchContext);

                    if (s_MeshesAreAssets.value)
                        EditorGUILayout.HelpBox("Please note that this feature is untested, and may result in instabilities or lost work. Proceed with caution!", MessageType.Warning);
                }
            }
        }
    }
}
