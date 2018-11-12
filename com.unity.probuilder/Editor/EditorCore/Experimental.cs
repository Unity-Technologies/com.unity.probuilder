using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	static class Experimental
	{
		[UserSetting]
		static Pref<bool> s_ExperimentalFeatures = new Pref<bool>("experimental.featuresEnabled", false, SettingsScope.User);

		[UserSetting]
		static Pref<bool> s_MeshesAreAssets = new Pref<bool>("experimental.meshesAreAssets", false, SettingsScope.Project);

		const string k_OverrideHandleSettings = "PROBUILDER_ENABLE_HANDLE_OVERRIDE";

		[UserSetting]
		static Pref<PivotPoint> s_PivotModePivotEquivalent = new Pref<PivotPoint>("debug.pivotModePivotEquivalent", PivotPoint.IndividualOrigins);

		internal static PivotPoint pivotModePivotEquivalent
		{
			get { return s_PivotModePivotEquivalent; }
		}

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

			var overrideHandleSettings = ScriptingSymbolManager.ContainsDefine(k_OverrideHandleSettings);

			EditorGUI.BeginChangeCheck();

			overrideHandleSettings = EditorGUILayout.Toggle("Override Handle Settings", overrideHandleSettings);

			if (EditorGUI.EndChangeCheck())
			{
				if(overrideHandleSettings)
					ScriptingSymbolManager.AddScriptingDefine(k_OverrideHandleSettings);
				else
					ScriptingSymbolManager.RemoveScriptingDefine(k_OverrideHandleSettings);
			}

			using (new SettingsGUILayout.IndentedGroup(!overrideHandleSettings))
			{
				EditorGUI.BeginChangeCheck();
				s_PivotModePivotEquivalent.value = (PivotPoint) EditorGUILayout.EnumPopup("PivotMode.Pivot", s_PivotModePivotEquivalent);
				if (EditorGUI.EndChangeCheck())
					ProBuilderSettings.Save();
			}
		}
	}
}
