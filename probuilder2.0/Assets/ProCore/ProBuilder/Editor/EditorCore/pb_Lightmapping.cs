#if !(UNITY_4_6 || UNITY_4_7) && !UNITY_5_OR_NEWER
#define UNITY_5_OR_NEWER
#endif

using UnityEditor;
using UnityEngine;
using ProBuilder2.Common;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Methods used in manipulating or creating Lightmaps.
	 */
	[InitializeOnLoad]
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

		static pb_Lightmapping()
		{
#if UNITY_5_OR_NEWER
			Lightmapping.completed += OnLightmappingCompleted;
#endif
		}

		private static void OnLightmappingCompleted()
		{
			if (!pb_PreferencesInternal.GetBool("pb_Lightmapping::showMissingLightmapUvWarning", false))
				return;

			IEnumerable<pb_Entity> missingUv2 = GameObject.FindObjectsOfType<pb_Entity>().Where(x => x.entityType == EntityType.Detail && !x.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic));
			int count = missingUv2.Count();
			if (count > 0)
				pb_Log.Warning("{0} ProBuilder {1} not included in lightmap bake due to missing UV2.\nYou can turn off this warning in Preferences/ProBuilder.", count, count == 1 ? "mesh" : "meshes");
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
		[System.Diagnostics.Conditional("UNITY_5_OR_NEWER")]
		internal static void PushGIWorkflowMode()
		{
#if UNITY_5_OR_NEWER
			pb_PreferencesInternal.SetInt("pb_GIWorkflowMode", (int)Lightmapping.giWorkflowMode);

			if(Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.Legacy)
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#endif
		}

		/**
		 * Return GIWorkflowMode to it's prior state.
		 */
		[System.Diagnostics.Conditional("UNITY_5_OR_NEWER")]
		internal static void PopGIWorkflowMode()
		{
#if UNITY_5_OR_NEWER
			// if no key found (?), don't do anything.
			if(!pb_PreferencesInternal.HasKey("pb_GIWorkflowMode"))
				return;

			 Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)pb_PreferencesInternal.GetInt("pb_GIWorkflowMode");
#endif
		}
	}
}
