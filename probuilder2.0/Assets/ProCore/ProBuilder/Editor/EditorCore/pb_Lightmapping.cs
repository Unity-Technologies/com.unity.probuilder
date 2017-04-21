using UnityEditor;
using UnityEngine;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

/**
 * Methods used in manipulating or creating Lightmaps.
 */
public static class pb_Lightmapping
{
	/**
	 * Editor-only extension to pb_Object generates lightmap UVs.
	 */
	[System.Obsolete("GenerateUV2 is obsolete, use pb_Editor_Mesh_Utility.Optimize(this pb_Object, bool forceRebuildUV2 = false) instead.")]
	public static void GenerateUV2(this pb_Object pb) { pb.GenerateUV2(false); }

	[System.Obsolete("GenerateUV2 is obsolete, use pb_Editor_Mesh_Utility.Optimize(this pb_Object, bool forceRebuildUV2 = false) instead.")]
	public static void GenerateUV2(this pb_Object pb, bool forceUpdate)
	{
		pb.Optimize(forceUpdate);
	}

	/**
	 *	Get the UnwrapParam values from a pb_UnwrapParameters object.
	 *	Not in pb_UnwrapParameters because UnwrapParam is an Editor class.
	 */
	public static UnwrapParam GetUnwrapParam(pb_UnwrapParameters parameters)
	{
		UnwrapParam param;
		UnwrapParam.SetDefaults(out param);

		if(parameters != null)
		{
			param.angleError = Mathf.Clamp(parameters.angleError, 1f, 75f) * .01f;
			param.areaError  = Mathf.Clamp(parameters.areaError , 1f, 75f) * .01f;
			param.hardAngle  = Mathf.Clamp(parameters.hardAngle , 0f, 180f);
			param.packMargin = Mathf.Clamp(parameters.packMargin, 1f, 64) * .001f;
		}

		return param;
	}

	/**
	 * Store the previous GIWorkflowMode and set the current value to OnDemand (or leave it Legacy).
	 */
	[System.Diagnostics.Conditional("UNITY_5")]
	internal static void PushGIWorkflowMode()
	{
#if UNITY_5
		pb_Preferences_Internal.SetInt("pb_GIWorkflowMode", (int)Lightmapping.giWorkflowMode);

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
		if(!pb_Preferences_Internal.HasKey("pb_GIWorkflowMode"))
			return;

		 Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)pb_Preferences_Internal.GetInt("pb_GIWorkflowMode");
#endif
	}
}
