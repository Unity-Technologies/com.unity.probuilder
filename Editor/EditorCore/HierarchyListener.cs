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

                // Case 1: Creation or Structure Change
                if (type == ObjectChangeKind.CreateGameObjectHierarchy ||
                    type == ObjectChangeKind.ChangeGameObjectStructure)
                {
                    GameObject go = GetGameObjectFromEvent(ref stream, i, type);
                    if (go != null)
                        CheckForProBuilderMeshesCreatedOrModified(go);
                }
                // Case 2: Hierarchy Structure Change
                else if (type == ObjectChangeKind.ChangeGameObjectStructureHierarchy)
                {
                    // Note 2 : This needs to be called when using a Prefab>Replace action in the menus to refresh the current
                    // ProBuilder Mesh, this is still a problem in 2023.3 as it does not automatically refresh the mesh
                    // Note 1 : that this is leaking meshes when reverting! in 2023.1+ we handle it correctly, but 2022 and
                    // 2021 have the PPtr reset to the serialized value (null) before we have any access. orphaned
                    // mesh assets are cleaned up on scene or domain reloads, so we'll live with it.
                    
                    GameObject go = GetGameObjectFromEvent(ref stream, i, type);

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
		
		private static GameObject GetGameObjectFromEvent(ref ObjectChangeEventStream stream, int index, ObjectChangeKind kind)
		{
		#if UNITY_6000_4_OR_NEWER
			switch (kind)
			{
				case ObjectChangeKind.CreateGameObjectHierarchy:
				{
					stream.GetCreateGameObjectHierarchyEvent(index, out var data);
					return UnityEditor.EditorUtility.EntityIdToObject(data.entityId) as GameObject;
				}
				case ObjectChangeKind.ChangeGameObjectStructure:
				{
					stream.GetChangeGameObjectStructureEvent(index, out var data);
					return UnityEditor.EditorUtility.EntityIdToObject(data.entityId) as GameObject;
				}
				case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
				{
					stream.GetChangeGameObjectStructureHierarchyEvent(index, out var data);
					return UnityEditor.EditorUtility.EntityIdToObject(data.entityId) as GameObject;
				}
			}
		#else
			int instanceId = 0;
			switch (kind)
			{
				case ObjectChangeKind.CreateGameObjectHierarchy:
				{
					stream.GetCreateGameObjectHierarchyEvent(index, out var data);
					instanceId = data.instanceId;
					break;
				}
				case ObjectChangeKind.ChangeGameObjectStructure:
				{
					stream.GetChangeGameObjectStructureEvent(index, out var data);
					instanceId = data.instanceId;
					break;
				}
				case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
				{
					stream.GetChangeGameObjectStructureHierarchyEvent(index, out var data);
					instanceId = data.instanceId;
					break;
				}
			}

			if (instanceId != 0)
			{
		#pragma warning disable CS0618 // Type or member is obsolete
				return UnityEditor.EditorUtility.InstanceIDToObject(instanceId) as GameObject;
		#pragma warning restore CS0618
			}
		#endif
			return null;
		}

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
