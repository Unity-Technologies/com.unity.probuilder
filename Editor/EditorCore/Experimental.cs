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
            get { return s_experimentalFeatureEnabled.value; }
        }

        [UserSettingBlock("Experimental")]
        static void ExperimentalFeaturesSettings(string searchContext)
        {
            EditorGUI.BeginChangeCheck();

            s_experimentalFeatureEnabled.value = SettingsGUILayout.SearchableToggle("Experimental Features Enabled", s_experimentalFeatureEnabled, searchContext);

            if (EditorGUI.EndChangeCheck())
            {
                if(s_experimentalFeatureEnabled.value)
                    ScriptingSymbolManager.AddScriptingDefine(k_ExperimentalFeaturesEnabled);
                else
                    ScriptingSymbolManager.RemoveScriptingDefine(k_ExperimentalFeaturesEnabled);
            }

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
