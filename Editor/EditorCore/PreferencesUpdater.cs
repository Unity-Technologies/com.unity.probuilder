using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProBuilder.Core;

namespace UnityEditor.ProBuilder
{
	static class PreferencesUpdater
	{
		static readonly pb_VersionInfo k_ProBuilder_3_0_2 = new pb_VersionInfo(3,0,2);

		/// <summary>
		/// Set the editor pref version, and check if any preferences need to be updated or reset.
		/// </summary>
		public static void CheckEditorPrefsVersion()
		{
			// this exists to force update preferences when updating packages
			var stored = new pb_VersionInfo(PreferencesInternal.GetString(pb_Constant.pbEditorPrefVersion)).MajorMinorPatch;
			var current = pb_Version.Current.MajorMinorPatch;

			if (!stored.Equals(current))
			{
				PreferencesInternal.SetString(pb_Constant.pbEditorPrefVersion, current.ToString("M.m.p"), PreferenceLocation.Global);

				if (stored < k_ProBuilder_3_0_2)
				{
					pb_Log.Info("Updated mesh handle graphic preferences to 3.0.2.");

					PreferencesInternal.DeleteKey(pb_Constant.pbUseUnityColors);
					PreferencesInternal.DeleteKey(pb_Constant.pbWireframeColor);
					PreferencesInternal.DeleteKey(pb_Constant.pbSelectedFaceColor);
					PreferencesInternal.DeleteKey(pb_Constant.pbSelectedFaceDither);
					PreferencesInternal.DeleteKey(pb_Constant.pbUnselectedEdgeColor);
					PreferencesInternal.DeleteKey(pb_Constant.pbSelectedEdgeColor);
					PreferencesInternal.DeleteKey(pb_Constant.pbUnselectedVertexColor);
					PreferencesInternal.DeleteKey(pb_Constant.pbSelectedVertexColor);
					PreferencesInternal.DeleteKey(pb_Constant.pbVertexHandleSize);
					PreferencesInternal.DeleteKey(pb_Constant.pbLineHandleSize);
					PreferencesInternal.DeleteKey(pb_Constant.pbWireframeSize);
				}
			}
		}
	}
}
