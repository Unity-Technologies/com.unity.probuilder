using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    public static class MeshSynchronization
    {
        static bool useMeshCache
        {
            get { return Experimental.experimentalFeaturesEnabled && Experimental.meshesAreAssets; }
        }

        static MeshSynchronization()
        {
            ProBuilderMesh.ensureSharedMeshIsOwnedByComponent += EnsureSharedMeshIsOwnedByComponent;

            ProBuilderMesh.beforeMeshAwake += (mesh) =>
            {
                // Delay validation until after the frame is run to avoid problems with accessing possibly uninitialized
                // components.
                EditorApplication.delayCall += () =>
                {
                    ValidateSharedMeshAndAssetInfo(mesh);
                };
            };
        }

        /// <summary>
        /// Ensure that this object has a valid mesh reference, and the geometry is current. If it is not valid, this
        /// function will attempt to repair the sync state.
        /// </summary>
        /// <param name="mesh">The component to test.</param>
        /// <seealso cref="ProBuilderMesh.meshSyncState"/>
        public static void SynchronizeWithMeshFilter(ProBuilderMesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            ValidateSharedMeshAndAssetInfo(mesh);
        }

        // Ensure that the mesh.assetInfo is in-sync with the current scene id, and rebuild the MeshFilter sharedMesh
        // asset if it is necessary.
        /// <summary>
        ///
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns>The <see cref="MeshSyncState"/> of this Mesh prior to validation.</returns>
        internal static MeshSyncState ValidateSharedMeshAndAssetInfo(ProBuilderMesh mesh)
        {
            if (EditorApplication.isPlaying)
                return MeshSyncState.None;

            MeshSyncState state = mesh.meshSyncState;

            bool meshNeedsRebuild = (state & MeshSyncState.Null) == MeshSyncState.Null;

            // If not using the MeshCache, instance ID mismatches are cause for rebuild (each mesh must be unique when
            // the cache is not in use).
            //
            // Known issue - when reverting prefab changes a mesh is unreferenced and leaked.
            if ((state & MeshSyncState.InstanceIDMismatch) == MeshSyncState.InstanceIDMismatch)
            {
                // Check if the old instance ID exists in the scene. If it does, then this is a duplicate event and it
                // is necessary to rebuild the mesh. If it does not exist, then the instance IDs were just cycled (as a
                // result of a scene reload, for example).
                var obj = UnityEditor.EditorUtility.InstanceIDToObject(mesh.assetInfo.instanceId);
                bool isDuplicate = obj != null && !(EditorUtility.IsPrefab(obj) || EditorUtility.IsPrefabInstance(obj));

                if (isDuplicate)
                {
                    Debug.Log(mesh.assetInfo + " is a duplicate");

                    if (useMeshCache)
                    {
                        mesh.NewAssetInfo();
                        // register with MeshCache here
                        MeshCache.Register(mesh);
                    }
                    else
                        meshNeedsRebuild = true;
                }
                else
                {
                    mesh.UpdateAssetInfoInstanceID();
                }
            }

            if(meshNeedsRebuild)
            {
                mesh.Rebuild();
                mesh.Optimize();
            }

            return state;
        }

        static void EnsureSharedMeshIsOwnedByComponent(ProBuilderMesh mesh)
        {
            MeshSyncState state = mesh.meshSyncState;

            if (!useMeshCache)
            {
                // When the MeshCache is not in use, we don't care about mesh GUIDs because there is no sharing of mesh
                // assets allowed in the first place.
                if ((state & MeshSyncState.Null) == MeshSyncState.Null
                    || (state & MeshSyncState.InstanceIDMismatch) == MeshSyncState.InstanceIDMismatch)
                {
                    var sb = new System.Text.StringBuilder();

                    sb.AppendLine("<b><color=\"#ff00ffff\">CreateNewSharedMesh</color></b>  <i>" + mesh.assetInfo.instanceId + " -> " + mesh.id + "</i>");
                    if ((state & MeshSyncState.Null) == MeshSyncState.Null)
                        sb.AppendLine("<b>MeshSyncState.Null</b>");
                    if((state & MeshSyncState.InstanceIDMismatch) == MeshSyncState.InstanceIDMismatch)
                        sb.AppendLine("<b>MeshSyncState.InstanceIDMismatch</b>");
                    if((state & MeshSyncState.MeshReferenceMismatch) == MeshSyncState.MeshReferenceMismatch)
                        sb.AppendLine("<b>MeshSyncState.GuidMismatch</b>");

                    Debug.Log(sb.ToString());

                    mesh.CreateNewSharedMesh();
                }
            }
            else
            {
                MeshCache.EnsureMeshAssetIsOwnedByComponent(mesh);
            }
        }
    }
}
