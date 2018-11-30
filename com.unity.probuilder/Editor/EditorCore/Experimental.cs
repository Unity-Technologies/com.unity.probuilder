using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    static class Experimental
    {
        [UserSetting]
        static Pref<bool> s_ExperimentalFeatures = new Pref<bool>("experimental.featuresEnabled", false, SettingsScope.User);

        [UserSetting]
        static Pref<bool> s_MeshesAreAssets = new Pref<bool>("experimental.meshesAreAssets", false, SettingsScope.Project);

        internal static bool meshesAreAssets
        {
            get { return s_ExperimentalFeatures && s_MeshesAreAssets; }
        }

        internal static bool experimentalFeaturesEnabled
        {
            get { return s_ExperimentalFeatures; }
        }

        [UserSettingBlock("Experimental")]
        static void ExperimentalFeaturesSettings(string searchContext)
        {
            s_ExperimentalFeatures.value = SettingsGUILayout.SettingsToggle("Experimental Features Enabled", s_ExperimentalFeatures, searchContext);

            if (s_ExperimentalFeatures.value)
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
