using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor.AssetImporters;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.SceneManagement;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// When building the project, remove all references to <see cref="ProBuilderMesh"/> and <see cref="EntityBehaviour"/>.
    /// </summary>
    class UnityScenePostProcessor :
#if UNITY_6000_7_OR_NEWER
        AssetPostprocessor
#else
        IProcessSceneWithReport
#endif
    {
        [UserSetting("General", "Script Stripping", "If true, when building an executable all ProBuilder scripts will be stripped from your built product.")]
        static Pref<bool> m_ScriptStripping = new Pref<bool>("editor.stripProBuilderScriptsOnBuild", true);

        const string k_DrivenPropertiesDependencyKey = "ProBuilder/DrivenProperties";
        const string k_ScriptStrippingDependencyKey = "ProBuilder/ScriptStripping";
        const string k_MeshesAreAssetsDependencyKey = "ProBuilder/MeshesAreAssets";

        [InitializeOnLoadMethod]
        static void InitializeDependencies()
        {
#if ENABLE_DRIVEN_PROPERTIES
            AssetDatabase.RegisterCustomDependency(k_DrivenPropertiesDependencyKey, Hash128.Compute(1));
#else
            AssetDatabase.RegisterCustomDependency(k_DrivenPropertiesDependencyKey, Hash128.Compute(0));
#endif
            ProBuilderSettings.instance.afterSettingsSaved += UpdateDependencies;
            UpdateDependencies();
        }

        static void UpdateDependencies()
        {
            AssetDatabase.RegisterCustomDependency(k_ScriptStrippingDependencyKey, Hash128.Compute(m_ScriptStripping.value ? 1 : 0));
            AssetDatabase.RegisterCustomDependency(k_MeshesAreAssetsDependencyKey, Hash128.Compute(Experimental.meshesAreAssets ? 1 : 0));
        }

#if UNITY_6000_7_OR_NEWER
        public override uint GetVersion() => 1;

        void OnProcessScene(Scene scene, UnityEditor.Build.Content.SceneImportContext sceneContext)
        {
            ProcessScene(scene, sceneContext.loadingReason == ProcessSceneMode.PlayMode, false, context);
        }
#else

        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            ProcessScene(scene, EditorApplication.isPlayingOrWillChangePlaymode, true, null);
        }
#endif

        /// <summary>
        /// Setup ProBuilderMesh and renderers components in the scene for playmode loading and builds.
        /// </summary>
        /// <param name="scene">The scene to process</param>
        /// <param name="isPlaymode">True if loading a scene in playmode. When true, This will skip the mesh update and components stripping.</param>
        /// <param name="canUpdateAssets">
        /// True if modifying assets outside the scene is allowed.
        /// This should always be false when building with Unity 6.7 and newer to prevent incremental build cache-miss.
        /// When false, it will log an error for each non-sync ProBuilderMesh in the scene.
        /// </param>
        /// <param name="context">AssetImportContext used to register build dependencies for incremental builds.</param>
        public static void ProcessScene(Scene scene, bool isPlaymode, bool canUpdateAssets = false, AssetImportContext context = null)
        {
            var invisibleFaceMaterial = Resources.Load<Material>("Materials/InvisibleFace");

            var pbMeshes = FindComponentsOfType<ProBuilderMesh>(scene);

            // Hide nodraw faces if present.
            foreach (var pb in pbMeshes)
            {
                if (pb.GetComponent<MeshRenderer>() == null || UnityEditor.EditorUtility.IsPersistent(pb))
                    continue;

                Material[] mats = pb.GetComponent<MeshRenderer>().sharedMaterials;
                bool hasMaterialChanged = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].name.Contains("NoDraw"))
                    {
                        mats[i] = invisibleFaceMaterial;
                        hasMaterialChanged = true;
                    }
                }
                if(hasMaterialChanged)
                    pb.GetComponent<MeshRenderer>().sharedMaterials = mats;
            }

            if (isPlaymode)
                return;

            var renderersToStrip = new List<Renderer>();

            var entities = FindComponentsOfType<EntityBehaviour>(scene);
            foreach (var entity in entities)
            {
                if (entity.manageVisibility)
                    entity.OnEnterPlayMode();

                if ((entity is TriggerBehaviour || entity is ColliderBehaviour)
                    && entity.gameObject.TryGetComponent(out MeshRenderer renderer)
                    && !UnityEditor.EditorUtility.IsPersistent(entity))
                    renderersToStrip.Add(renderer);
            }

            foreach (var mesh in pbMeshes)
            {
                if (UnityEditor.EditorUtility.IsPersistent(mesh))
                    continue;

                context?.DependsOnCustomDependency(k_MeshesAreAssetsDependencyKey);
                EditorUtility.SynchronizeWithMeshFilter(mesh, canUpdateAssets);

                if (mesh.mesh == null)
                    continue;

                GameObject gameObject = mesh.gameObject;
                var entity = ProcessLegacyEntity(gameObject);

                context?.DependsOnCustomDependency(k_DrivenPropertiesDependencyKey);
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
                context?.DependsOnCustomDependency(k_ScriptStrippingDependencyKey);
                if (m_ScriptStripping == false)
                    continue;

                StripProBuilderScripts.DestroyProBuilderMeshAndDependencies(gameObject, mesh, true);
            }

            foreach (var renderer in renderersToStrip)
            {
                if (renderer != null)
                {
                    Object.DestroyImmediate(renderer);
                }
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

        static List<T> FindComponentsOfType<T>(Scene scene) where T : Component
        {
            List<T> components = new List<T>();
            List<T> finder = new List<T>();

            foreach (var o in scene.GetRootGameObjects())
            {
                o.GetComponentsInChildren(true, finder);
                components.AddRange(finder);
            }
            return components;
        }
    }
}
