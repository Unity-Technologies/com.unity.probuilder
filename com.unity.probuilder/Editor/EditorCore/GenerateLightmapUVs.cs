using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	/// <summary>
	/// Menu items for generating UV2s for selected or scene pb_Object meshes.
	/// </summary>
	/// <inheritdoc />
	sealed class GenerateLightmapUVs : Editor
	{
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Generate UV2 - Selection", true, PreferenceKeys.menuActions + 20)]
		public static bool VerifyGenerateUV2Selection()
		{
			return InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Generate UV2 - Selection", false, PreferenceKeys.menuActions + 20)]
		public static void MenuGenerateUV2Selection()
		{
			ProBuilderMesh[] sel = Selection.transforms.GetComponents<ProBuilderMesh>();

			if( !Menu_GenerateUV2(sel) )
				return;	// user canceled

			if(sel.Length > 0)
				EditorUtility.ShowNotification("Generated UV2 for " + sel.Length + " Meshes");
			else
				EditorUtility.ShowNotification("Nothing Selected");
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Generate UV2 - Scene", false, PreferenceKeys.menuActions + 20)]
		public static void MenuGenerateUV2Scene()
		{
			ProBuilderMesh[] sel = (ProBuilderMesh[])FindObjectsOfType(typeof(ProBuilderMesh));

			if( !Menu_GenerateUV2(sel) )
				return;

			if(sel.Length > 0)
				EditorUtility.ShowNotification("Generated UV2 for " + sel.Length + " Meshes");
			else
				EditorUtility.ShowNotification("No ProBuilder Objects Found");
		}

		static bool Menu_GenerateUV2(ProBuilderMesh[] selected)
		{
			for(int i = 0; i < selected.Length; i++)
			{
				if(selected.Length > 3)
				{
					if( UnityEditor.EditorUtility.DisplayCancelableProgressBar(
						"Generating UV2 Channel",
						"pb_Object: " + selected[i].name + ".",
						(((float)i+1) / selected.Length)))
					{
						UnityEditor.EditorUtility.ClearProgressBar();
						Debug.LogWarning("User canceled UV2 generation.  " + (selected.Length-i) + " pb_Objects left without lightmap UVs.");
						return false;
					}
				}

				// True parameter forcibly generates UV2.  Otherwise if pbDisableAutoUV2Generation is true then UV2 wouldn't be built.
				selected[i].Optimize(true);
			}

			UnityEditor.EditorUtility.ClearProgressBar();
			return true;
		}
	}
}
