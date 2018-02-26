using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.EditorCore;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Static delegates listen for hierarchy changes (duplication, delete, copy/paste) and rebuild the mesh components of pb_Objects if necessary.
	/// </summary>
	[InitializeOnLoad]
	static class pb_HierarchyListener
	{
		static pb_HierarchyListener()
		{
			// When a prefab is updated, this is raised.  For some reason it's
			// called twice?
 #if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged += HierarchyWindowChanged;
 #else
			EditorApplication.hierarchyWindowChanged -= HierarchyWindowChanged;
			EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
 #endif

			// prefabInstanceUpdated is not called when dragging out of Project view,
			// or when creating a prefab or reverting.  OnHierarchyChange captures those.
			PrefabUtility.prefabInstanceUpdated -= PrefabInstanceUpdated;
			PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
		}

		static void PrefabInstanceUpdated(GameObject go)
		{
			if(EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			foreach(pb_Object pb in go.GetComponentsInChildren<pb_Object>())
			{
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}
		}

		/**
		 * Used to catch prefab modifications that otherwise wouldn't be registered on the usual 'Awake' verify.
		 *	- Dragging prefabs out of Project
		 *	- 'Revert' prefab changes
		 *	- 'Apply' prefab changes
		 */
		static void HierarchyWindowChanged()
		{
			if(!EditorApplication.isPlaying)
			{
				bool meshesAreAssets = pb_PreferencesInternal.GetBool(pb_Constant.pbMeshesAreAssets);

				// on duplication, or copy paste, this rebuilds the mesh structures of the new objects
				foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
				{
					if(!meshesAreAssets)
						pb_EditorUtility.VerifyMesh(pb);
				}

				pb_Editor.Refresh();
			}
		}
	}
}
