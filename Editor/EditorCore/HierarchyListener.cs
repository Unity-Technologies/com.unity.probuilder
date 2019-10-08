using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.ProBuilder;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Static delegates listen for hierarchy changes (duplication, delete, copy/paste) and rebuild the mesh components of pb_Objects if necessary.
    /// </summary>
    [InitializeOnLoad]
    static class HierarchyListener
    {
        static HierarchyListener()
        {
            // When a prefab is updated, this is raised.  For some reason it's
            // called twice?
 #if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += HierarchyWindowChanged;
 #else
            EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
 #endif

            // prefabInstanceUpdated is not called when dragging out of Project view,
            // or when creating a prefab or reverting.  OnHierarchyChange captures those.
            PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
        }

        static void PrefabInstanceUpdated(GameObject go)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            foreach (ProBuilderMesh pb in go.GetComponentsInChildren<ProBuilderMesh>())
            {
                EditorUtility.SynchronizeWithMeshFilter(pb);
                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }
        }

        /**
         * Used to catch prefab modifications that otherwise wouldn't be registered on the usual 'Awake' verify.
         *  - Dragging prefabs out of Project
         *  - 'Revert' prefab changes
         *  - 'Apply' prefab changes
         */
        static void HierarchyWindowChanged()
        {
            if (!EditorApplication.isPlaying)
            {
                bool meshesAreAssets = Experimental.meshesAreAssets;

                // on duplication, or copy paste, this rebuilds the mesh structures of the new objects
                foreach (ProBuilderMesh pb in Selection.transforms.GetComponents<ProBuilderMesh>())
                {
                    if (!meshesAreAssets)
                        EditorUtility.SynchronizeWithMeshFilter(pb);
                }
            }
        }
    }
}
