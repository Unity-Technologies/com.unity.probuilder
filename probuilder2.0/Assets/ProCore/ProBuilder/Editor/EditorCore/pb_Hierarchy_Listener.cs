using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorCommon;

[InitializeOnLoad]
public class pb_Hierarchy_Listener : Editor
{
	static pb_Hierarchy_Listener()
	{
		EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;

		// prefabInstanceUpdated is not called when dragging out of Project view,
		// or when creating a prefab
		// PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
	}
	
	~pb_Hierarchy_Listener()
	{
		EditorApplication.hierarchyWindowChanged -= HierarchyWindowChanged;
	}

	/**
	 * Used to catch prefab modifications that otherwise wouldn't be registered on the usual 'Awake' verify.
	 *	- Dragging prefabs out of Project
	 *	- 'Apply' prefab changes
	 */
	static void HierarchyWindowChanged()
	{
		bool prefabModified = false;

		if(!EditorApplication.isPlaying)
		{
			foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))
			{
				/**
				 * If it's a prefab instance, reconstruct submesh structure.
				 */
				if(	PrefabUtility.GetPrefabType(pb.gameObject) == PrefabType.PrefabInstance )
				{
					prefabModified = true;

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
				}
			}
		}

		if(prefabModified)
		{
			if(pb_Editor.instance != null)
				pb_Editor.instance.UpdateSelection(true);

			SceneView.RepaintAll();
		}
	}
}
