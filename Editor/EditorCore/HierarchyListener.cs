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
                // Debug.Log($"event type {i} {stream.GetEventType(i)}");

                // ProBuilderMesh was created via duplicate, copy paste
                if (stream.GetEventType(i) == ObjectChangeKind.CreateGameObjectHierarchy)
                {
                    stream.GetCreateGameObjectHierarchyEvent(i, out CreateGameObjectHierarchyEventArgs data);
                    // Debug.Log($"CreateGameObjectHierarchy {UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId)} {data.instanceId}");

                    // if the created object is a probuilder mesh, check if it is a copy of an existing instance.
                    // if so, we need to create a new mesh asset.
                    if (UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId) is GameObject go
                        && go.TryGetComponent<ProBuilderMesh>(out var mesh))
                        OnObjectCreated(mesh);
                }
                // ProBuilderMesh was created by adding from component menu or pasting component
                else if (stream.GetEventType(i) == ObjectChangeKind.ChangeGameObjectStructure)
                {
                    stream.GetChangeGameObjectStructureEvent(i, out var data);
                    Debug.Log($"ChangeGameObjectStructure {UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId)} {data.instanceId}");
                }
            }
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
            foreach(var mesh in go.GetComponentsInChildren<ProBuilderMesh>())
                mesh.DestroyUnityMesh();
        }

        static void PrefabInstanceUpdated(GameObject go)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            foreach (var mesh in go.GetComponentsInChildren<ProBuilderMesh>())
                EditorUtility.SynchronizeWithMeshFilter(mesh);

            ProBuilderEditor.Refresh();
        }
    }
}
