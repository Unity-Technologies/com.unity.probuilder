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
            PrefabUtility.prefabInstanceReverted += PrefabInstanceUpdated;
            PrefabUtility.prefabInstanceReverting += PrefabInstanceReverting;
            ProBuilderMesh.meshWasInitialized += OnMeshInitialized;
            ObjectChangeEvents.changesPublished += ObjectEventChangesPublished;
        }

        static void ObjectEventChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0, c = stream.length; i < c; ++i)
            {
                var type = stream.GetEventType(i);

                if (type == ObjectChangeKind.CreateGameObjectHierarchy)
                {
                    stream.GetCreateGameObjectHierarchyEvent(i, out var data);
                    var go = GetGameObject(data);
                    if (go != null) CheckForProBuilderMeshesCreatedOrModified(go);
                }
                else if (type == ObjectChangeKind.ChangeGameObjectStructure)
                {
                    stream.GetChangeGameObjectStructureEvent(i, out var data);
                    var go = GetGameObject(data);
                    if (go != null) CheckForProBuilderMeshesCreatedOrModified(go);
                }
                else if (type == ObjectChangeKind.ChangeGameObjectStructureHierarchy)
                {
                    // Note 2 : This needs to be called when using a Prefab>Replace action in the menus to refresh the current
                    // ProBuilder Mesh, this is still a problem in 2023.3 as it does not automatically refresh the mesh
                    // Note 1 : that this is leaking meshes when reverting! in 2023.1+ we handle it correctly, but 2022 and
                    // 2021 have the PPtr reset to the serialized value (null) before we have any access. orphaned
                    // mesh assets are cleaned up on scene or domain reloads, so we'll live with it.
                    stream.GetChangeGameObjectStructureHierarchyEvent(i, out var data);
                    var go = GetGameObject(data);

                    if (go != null)
                    {
                        var meshes = go.GetComponentsInChildren<ProBuilderMesh>();
                        foreach (var mesh in meshes)
                            EditorUtility.SynchronizeWithMeshFilter(mesh);
                    }
                    ProBuilderEditor.Refresh();
                }
            }
        }

#if UNITY_6000_4_OR_NEWER
        static GameObject GetGameObject(CreateGameObjectHierarchyEventArgs data) => UnityEditor.EditorUtility.EntityIdToObject(data.entityId) as GameObject;
        static GameObject GetGameObject(ChangeGameObjectStructureEventArgs data) => UnityEditor.EditorUtility.EntityIdToObject(data.entityId) as GameObject;
        static GameObject GetGameObject(ChangeGameObjectStructureHierarchyEventArgs data) => UnityEditor.EditorUtility.EntityIdToObject(data.entityId) as GameObject;
#else
        static GameObject GetGameObject(CreateGameObjectHierarchyEventArgs data) => InstanceIDToObject(data.instanceId);
        static GameObject GetGameObject(ChangeGameObjectStructureEventArgs data) => InstanceIDToObject(data.instanceId);
        static GameObject GetGameObject(ChangeGameObjectStructureHierarchyEventArgs data) => InstanceIDToObject(data.instanceId);

        static GameObject InstanceIDToObject(int instanceId)
        {
#pragma warning disable 0618 // Marked obsolete on 6.3
            return UnityEditor.EditorUtility.InstanceIDToObject(instanceId) as GameObject;
#pragma warning restore 0618
        }
#endif

        static void CheckForProBuilderMeshesCreatedOrModified(GameObject go)
        {
            if(go.TryGetComponent<ProBuilderMesh>(out var mesh))
                OnObjectCreated(mesh);
            var childCount = go.transform.childCount;
            if (childCount > 0)
            {
                for(int childIndex = 0; childIndex < childCount; ++childIndex)
                    CheckForProBuilderMeshesCreatedOrModified(go.transform.GetChild(childIndex).gameObject);
            }
        }

        // used by tests
        internal static void OnObjectCreated(ProBuilderMesh mesh)
        {
            if (mesh.versionIndex != ProBuilderMesh.k_UnitializedVersionIndex &&
                mesh.nonSerializedVersionIndex == ProBuilderMesh.k_UnitializedVersionIndex)
            {
                mesh.MakeUnique();
                Undo.RegisterCompleteObjectUndo( mesh.gameObject, "Paste ProBuilderMesh");
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
                mesh.Rebuild();
                mesh.Optimize();
            }

            ProBuilderEditor.Refresh();
        }
    }
}
