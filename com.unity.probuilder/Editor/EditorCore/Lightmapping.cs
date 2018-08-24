using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Methods used in manipulating or creating Lightmaps.
	/// </summary>
	[InitializeOnLoad]
	static class Lightmapping
	{
		static Lightmapping()
		{
			UnityEditor.Lightmapping.completed += OnLightmappingCompleted;
		}

		/// <summary>
		/// Toggles the LightmapStatic bit of an objects Static flags.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="isEnabled"></param>
		public static void SetLightmapStaticFlagEnabled(ProBuilderMesh pb, bool isEnabled)
		{
			Entity ent = pb.GetComponent<Entity>();

			if (ent != null && ent.entityType == EntityType.Detail)
			{
				StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(pb.gameObject);

				if (isEnabled != (flags & StaticEditorFlags.LightmapStatic) > 0)
				{
					flags ^= StaticEditorFlags.LightmapStatic;
					GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);
				}
			}
		}

		static void OnLightmappingCompleted()
		{
			if (!PreferencesInternal.GetBool(PreferenceKeys.pbShowMissingLightmapUvWarning, false))
				return;

			var missingUv2 = Object.FindObjectsOfType<ProBuilderMesh>().Where(x => !x.HasArrays(MeshArrays.Lightmap) && x.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic));

			int count = missingUv2.Count();

			if (count > 0)
				Log.Warning("{0} ProBuilder {1} included in lightmap bake with missing UV2.\nYou can turn off this warning in Preferences/ProBuilder.", count, count == 1 ? "mesh" : "meshes");
		}

		/// <summary>
		/// Build Lightmap UVs for each mesh in the selection that is missing the UV2 array.
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="showProgressBar"></param>
		public static int RebuildMissingLightmapUVs(IEnumerable<ProBuilderMesh> selection, bool showProgressBar = false)
		{
			int count = 0;
			float total = selection.Count(x => x.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic) && !x.HasArrays(MeshArrays.Lightmap));

			foreach (var mesh in selection)
			{
				if (!mesh.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic) || mesh.HasArrays(MeshArrays.Texture1))
					continue;

				if (showProgressBar)
				{
					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Generate Lightmap UVs", "Unwrapping UVs for mesh: " + mesh.name, count / total))
						break;
				}

				count++;
				mesh.Optimize(true);
			}

			UnityEditor.EditorUtility.ClearProgressBar();

			return count;
		}

		/**
		 *	Get the UnwrapParam values from a pb_UnwrapParameters object.
		 *	Not in pb_UnwrapParameters because UnwrapParam is an Editor class.
		 */
		public static UnwrapParam GetUnwrapParam(UnwrapParameters parameters)
		{
			UnwrapParam param = new UnwrapParam();

			if(parameters != null)
			{
				param.angleError = Mathf.Clamp(parameters.angleError, 1f, 75f) * .01f;
				param.areaError  = Mathf.Clamp(parameters.areaError , 1f, 75f) * .01f;
				param.hardAngle  = Mathf.Clamp(parameters.hardAngle , 0f, 180f);
				param.packMargin = Mathf.Clamp(parameters.packMargin, 1f, 64) * .001f;
			}
			else
			{
				param.angleError = Mathf.Clamp(UnwrapParameters.k_AngleError, 1f, 75f) * .01f;
				param.areaError  = Mathf.Clamp(UnwrapParameters.k_AreaError , 1f, 75f) * .01f;
				param.hardAngle  = Mathf.Clamp(UnwrapParameters.k_HardAngle , 0f, 180f);
				param.packMargin = Mathf.Clamp(UnwrapParameters.k_PackMargin, 1f, 64) * .001f;
			}

			return param;
		}

		/**
		 * Store the previous GIWorkflowMode and set the current value to OnDemand (or leave it Legacy).
		 */
		internal static void PushGIWorkflowMode()
		{
			PreferencesInternal.SetInt("pb_GIWorkflowMode", (int)UnityEditor.Lightmapping.giWorkflowMode);

			if(UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.Legacy)
				UnityEditor.Lightmapping.giWorkflowMode = UnityEditor.Lightmapping.GIWorkflowMode.OnDemand;
		}

		/**
		 * Return GIWorkflowMode to it's prior state.
		 */
		internal static void PopGIWorkflowMode()
		{
			// if no key found (?), don't do anything.
			if(!PreferencesInternal.HasKey("pb_GIWorkflowMode"))
				return;

			UnityEditor.Lightmapping.giWorkflowMode = (UnityEditor.Lightmapping.GIWorkflowMode)PreferencesInternal.GetInt("pb_GIWorkflowMode");
		}
	}
}
