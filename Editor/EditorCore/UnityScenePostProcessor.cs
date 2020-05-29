using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.ProBuilder.MeshOperations;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// When building the project, remove all references to <see cref="ProBuilderMesh"/> and <see cref="EntityBehaviour"/>.
    /// </summary>
    static class UnityScenePostProcessor
    {
        [UserSetting("General", "Script Stripping", "If true, when building an executable all ProBuilder scripts will be stripped from your built product.")]
        static Pref<bool> m_ScriptStripping = new Pref<bool>("editor.stripProBuilderScriptsOnBuild", true);

        [PostProcessScene]
        public static void OnPostprocessScene()
        {
            var invisibleFaceMaterial = Resources.Load<Material>("Materials/InvisibleFace");

            // Hide nodraw faces if present.
            foreach (var pb in Object.FindObjectsOfType<ProBuilderMesh>())
            {
                if (pb.GetComponent<MeshRenderer>() == null)
                    continue;

                if (pb.GetComponent<MeshRenderer>().sharedMaterials.Any(x => x != null && x.name.Contains("NoDraw")))
                {
                    Material[] mats = pb.GetComponent<MeshRenderer>().sharedMaterials;

                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i].name.Contains("NoDraw"))
                            mats[i] = invisibleFaceMaterial;
                    }

                    pb.GetComponent<MeshRenderer>().sharedMaterials = mats;
                }
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            foreach (var entity in Resources.FindObjectsOfTypeAll<EntityBehaviour>())
            {
                if (entity.manageVisibility)
                    entity.OnEnterPlayMode();
            }

            foreach (var mesh in Object.FindObjectsOfType<ProBuilderMesh>())
            {
                EditorUtility.SynchronizeWithMeshFilter(mesh);

                if (mesh.mesh == null)
                    continue;

                GameObject gameObject = mesh.gameObject;
                var entity = ProcessLegacyEntity(gameObject);

#if ENABLE_DRIVEN_PROPERTIES
                // clear editor-only HideFlags and serialization ignores
                mesh.ClearDrivenProperties();
                var filter = gameObject.DemandComponent<MeshFilter>();
                filter.hideFlags = HideFlags.None;
                mesh.mesh.hideFlags = HideFlags.None;

                // Reassign the MeshFilter and MeshCollider properties _after_ clearing HideFlags and driven properties
                // to ensure that they are dirtied for serialization and thus included in the build
                filter.sharedMesh = mesh.mesh;
                if (mesh.TryGetComponent(out MeshCollider collider))
                    collider.sharedMesh = mesh.mesh;
#endif

                // early out if we're not planning to remove the ProBuilderMesh component
                if (m_ScriptStripping == false)
                    continue;

                mesh.preserveMeshAssetOnDestroy = true;
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(entity);
            }
        }

        static Entity ProcessLegacyEntity(GameObject go)
        {
            // Entity is deprecated - remove someday
            Entity entity = go.GetComponent<Entity>();

            if (entity == null)
                return null;

            if (entity.entityType == EntityType.Collider || entity.entityType == EntityType.Trigger)
                go.GetComponent<MeshRenderer>().enabled = false;

            return entity;
        }
    }
}
