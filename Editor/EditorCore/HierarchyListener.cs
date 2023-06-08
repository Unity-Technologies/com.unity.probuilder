using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Static delegates listen for hierarchy changes (duplication, delete, copy/paste) and rebuild the mesh components
    /// of pb_Objects if necessary.
    /// </summary>
    [InitializeOnLoad]
    static class HierarchyListener
    {
        static HierarchyListener()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
            #if UNITY_2023_1_OR_NEWER
            PrefabUtility.prefabInstanceReverted += PrefabInstanceUpdated;
            PrefabUtility.prefabInstanceReverting += PrefabInstanceReverting;
            #endif
            ProBuilderMesh.meshWasInitialized += OnMeshInitialized;

            #if UNITY_2020_2_OR_NEWER
            ObjectChangeEvents.changesPublished += ObjectEventChangesPublished;
            #else
            EditorApplication.hierarchyChanged += HierarchyWindowChanged;
            #endif
        }

        #if UNITY_2020_2_OR_NEWER
        static void ObjectEventChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0, c = stream.length; i < c; ++i)
            {
                // ProBuilderMesh was created via duplicate, copy paste
                if (stream.GetEventType(i) == ObjectChangeKind.CreateGameObjectHierarchy)
                {
                    stream.GetCreateGameObjectHierarchyEvent(i, out CreateGameObjectHierarchyEventArgs data);
                    GameObjectCreatedOrStructureModified(data.instanceId);
                }
                // ProBuilderMesh was created by adding from component menu or pasting component
                else if (stream.GetEventType(i) == ObjectChangeKind.ChangeGameObjectStructure)
                {
                    stream.GetChangeGameObjectStructureEvent(i, out var data);
                    GameObjectCreatedOrStructureModified(data.instanceId);
                }
                #if !UNITY_2023_1_OR_NEWER
                // for handling prefab revert pre-2023.1
                else if (stream.GetEventType(i) == ObjectChangeKind.ChangeGameObjectStructureHierarchy)
                {
                    // note that this is leaking meshes when reverting! in 2023.1+ we handle it correctly, but 2022 and
                    // 2021 have the PPtr reset to the serialized value (null) before we have any access. orphaned
                    // mesh assets are cleaned up on scene or domain reloads, so we'll live with it. the alternative is
                    // to find all mesh assets, determine which aren't referenced by any component and owned by
                    // probuilder, then destroy. it's not without risk, as we would be relying on string comparison
                    // of names to assume that scene mesh assets were created by probuilder.
                    stream.GetChangeGameObjectStructureHierarchyEvent(i, out var data);

                    if (UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId) is GameObject go)
                    {
                        var meshes = go.GetComponentsInChildren<ProBuilderMesh>();
                        foreach (var mesh in meshes)
                            EditorUtility.SynchronizeWithMeshFilter(mesh);
                    }

                    ProBuilderEditor.Refresh();
                }
                #endif
            }
        }
        #else
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
            
            ProBuilderEditor.Refresh();
        }
        #endif

        static void GameObjectCreatedOrStructureModified(int instanceId)
        {
            // if the created object is a probuilder mesh, check if it is a copy of an existing instance.
            // if so, we need to create a new mesh asset.
            if (UnityEditor.EditorUtility.InstanceIDToObject(instanceId) is GameObject go
                && go.TryGetComponent<ProBuilderMesh>(out var mesh))
                OnObjectCreated(mesh);
        }

        // used by tests
        internal static void OnObjectCreated(ProBuilderMesh mesh)
        {
            if (mesh.versionIndex != ProBuilderMesh.k_UnitializedVersionIndex &&
                mesh.nonSerializedVersionIndex == ProBuilderMesh.k_UnitializedVersionIndex)
            {
                mesh.MakeUnique();
                Undo.RegisterCreatedObjectUndo(mesh.mesh, "Paste ProBuilderMesh");
                mesh.Optimize();
            }
        }

        static void OnMeshInitialized(ProBuilderMesh mesh)
        {
            mesh.Optimize();
        }

        static void OnAfterAssemblyReload()
        {
            // The inspector icon for ProBuilderMesh is set in the component metadata. However, this also serves as the
            // scene view gizmo icon, which we do not want. To avoid drawing an icon for every mesh in the Scene View,
            // we simply tell the AnnotationManager to not render the icon. This _does_ put ProBuilderMesh in the
            // "Recently Changed" list, but only when it is modified the first time.
            // The alternative method of setting an icon is to place it in a folder named "Editor Default Resources/Icons",
            // however that requires that the resources directory be in "Assets", which we do not want to do.
            EditorApplication.delayCall += () => EditorUtility.SetGizmoIconEnabled(typeof(ProBuilderMesh), false);
        }

        static void PrefabInstanceReverting(GameObject go)
        {
            // revert will leak meshes unless we clean up before ProBuilderMesh.m_Mesh is set to null
            foreach (var mesh in go.GetComponentsInChildren<ProBuilderMesh>())
                mesh.DestroyUnityMesh();
        }

        static void PrefabInstanceUpdated(GameObject go)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            foreach (var mesh in go.GetComponentsInChildren<ProBuilderMesh>())
            {
                EditorUtility.SynchronizeWithMeshFilter(mesh);
                #if !UNITY_2020_2_OR_NEWER
                mesh.ToMesh();
                mesh.Refresh();
                mesh.Optimize();
                #endif
            }

            ProBuilderEditor.Refresh();
        }
    }
}
