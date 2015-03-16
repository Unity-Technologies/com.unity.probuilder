using UnityEditor;
using UnityEngine;

/**
 * Methods used in manipulating or creating Lightmaps.
 */
public static class pb_Lightmapping
{
	/**
	 * Editor-only extension to pb_Object generates lightmap UVs.
	 */
	public static void GenerateUV2_2(this pb_Object pb) { pb.GenerateUV2_2(false); }

	public static void GenerateUV2_2(this pb_Object pb, bool forceUpdate)
	{
		if(pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation) && !forceUpdate)
			return;

		// SetUVParams(8f, 15f, 15f, 20f);
		UnwrapParam param;
		UnwrapParam.SetDefaults(out param);
		
		param.angleError = Mathf.Clamp(pb.angleError, 1f, 75f) * .01f;
		param.areaError  = Mathf.Clamp(pb.areaError , 1f, 75f) * .01f;
		param.hardAngle  = Mathf.Clamp(pb.hardAngle , 0f, 180f);
		param.packMargin = Mathf.Clamp(pb.packMargin, 1f, 64) * .001f;

		Unwrapping.GenerateSecondaryUVSet(pb.GetComponent<MeshFilter>().sharedMesh, param);

		EditorUtility.SetDirty(pb);
	}

	/**
	 * Store the previous GIWorkflowMode and set the current value to OnDemand (or leave it Legacy).
	 */
	[System.Diagnostics.Conditional("UNITY_5")]
	internal static void PushGIWorkflowMode()
	{
#if UNITY_5
		EditorPrefs.SetInt("pb_GIWorkflowMode", (int)Lightmapping.giWorkflowMode);

		if(Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.Legacy)
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#endif
	}

	/**
	 * Return GIWorkflowMode to it's prior state.
	 */
	[System.Diagnostics.Conditional("UNITY_5")]
	internal static void PopGIWorkflowMode()
	{
#if UNITY_5
		// if no key found (?), don't do anything.
		if(!EditorPrefs.HasKey("pb_GIWorkflowMode"))
			return;

		 Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)EditorPrefs.GetInt("pb_GIWorkflowMode");
#endif
	}
}
