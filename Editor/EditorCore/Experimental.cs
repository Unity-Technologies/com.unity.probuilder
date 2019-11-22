using UnityEditor.SettingsManagement;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    static class Experimental
    {
        const string k_ExperimentalFeaturesEnabled = "PROBUILDER_EXPERIMENTAL_FEATURES";

        [UserSetting]
        static Pref<bool> s_MeshesAreAssets = new Pref<bool>("experimental.meshesAreAssets", false, SettingsScope.Project);

        internal static bool meshesAreAssets
        {
            get { return experimentalFeaturesEnabled && s_MeshesAreAssets; }
            set { s_MeshesAreAssets.value = value; }
        }

        internal static bool experimentalFeaturesEnabled
        {
            get
            {
#if PROBUILDER_EXPERIMENTAL_FEATURES
                return true;
#else
                return false;
#endif
            }

            set
            {
#if PROBUILDER_EXPERIMENTAL_FEATURES
                if(!value)
                    ScriptingSymbolManager.RemoveScriptingDefine(k_ExperimentalFeaturesEnabled);
#else
                if(value)
                    ScriptingSymbolManager.AddScriptingDefine(k_ExperimentalFeaturesEnabled);
#endif
            }
        }

        [UserSettingBlock("Experimental")]
        static void ExperimentalFeaturesSettings(string searchContext)
        {
            var enabled = experimentalFeaturesEnabled;

            EditorGUI.BeginChangeCheck();

            enabled = SettingsGUILayout.SearchableToggle("Experimental Features Enabled", enabled, searchContext);

            if (EditorGUI.EndChangeCheck())
            {
                if(enabled)
                    ScriptingSymbolManager.AddScriptingDefine(k_ExperimentalFeaturesEnabled);
                else
                    ScriptingSymbolManager.RemoveScriptingDefine(k_ExperimentalFeaturesEnabled);
            }

            if(enabled)
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
