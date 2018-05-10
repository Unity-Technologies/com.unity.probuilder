using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	static class PreferencesUpdater
	{
		static readonly SemVer k_ProBuilder_3_0_2 = new SemVer(3,0,2);

		/// <summary>
		/// Set the editor pref version, and check if any preferences need to be updated or reset.
		/// </summary>
		public static void CheckEditorPrefsVersion()
		{
			// this exists to force update preferences when updating packages
			var stored = new SemVer(PreferencesInternal.GetString(PreferenceKeys.pbEditorPrefVersion)).MajorMinorPatch;
			var current = Version.currentInfo.MajorMinorPatch;

			if (!stored.Equals(current))
			{
				PreferencesInternal.SetString(PreferenceKeys.pbEditorPrefVersion, current.ToString("M.m.p"), PreferenceLocation.Global);

				if (stored < k_ProBuilder_3_0_2)
				{
					Log.Info("Updated mesh handle graphic preferences to 3.0.2.");

					PreferencesInternal.DeleteKey(PreferenceKeys.pbUseUnityColors);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbWireframeColor);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedFaceColor);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedFaceDither);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbUnselectedEdgeColor);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedEdgeColor);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbUnselectedVertexColor);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedVertexColor);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbVertexHandleSize);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbLineHandleSize);
					PreferencesInternal.DeleteKey(PreferenceKeys.pbWireframeSize);
				}
			}
		}
	}
}
