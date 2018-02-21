using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	static class pb_PreferencesUpdater
	{
		static readonly pb_VersionInfo k_ProBuilder_3_0_2 = new pb_VersionInfo(3,0,2);

		/// <summary>
		/// Set the editor pref version, and check if any preferences need to be updated or reset.
		/// </summary>
		public static void CheckEditorPrefsVersion()
		{
			// this exists to force update preferences when updating packages
			var stored = new pb_VersionInfo(pb_PreferencesInternal.GetString(pb_Constant.pbEditorPrefVersion)).MajorMinorPatch;
			var current = pb_Version.Current.MajorMinorPatch;

			if (!stored.Equals(current))
			{
				pb_PreferencesInternal.SetString(pb_Constant.pbEditorPrefVersion, current.ToString("M.m.p"), pb_PreferenceLocation.Global);

				if (stored < k_ProBuilder_3_0_2)
				{
					pb_Log.Info("Updated mesh handle graphic preferences to 3.0.2.");

					pb_PreferencesInternal.DeleteKey(pb_Constant.pbUseUnityColors);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbWireframeColor);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedFaceColor);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedFaceDither);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbUnselectedEdgeColor);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedEdgeColor);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbUnselectedVertexColor);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedVertexColor);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbVertexHandleSize);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbLineHandleSize);
					pb_PreferencesInternal.DeleteKey(pb_Constant.pbWireframeSize);
				}
			}
		}
	}
}
