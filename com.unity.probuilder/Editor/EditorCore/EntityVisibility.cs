using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    ///     Responsible for managing the visibility of entity types in the scene.
    /// </summary>
    [InitializeOnLoad]
    internal static class EntityVisibility
    {
        static EntityVisibility()
        {
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += x => { OnPlayModeStateChanged(); };
#else
            EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
#endif
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (EditorApplication.isPlaying)
            {
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (var entityBehaviour in rootGameObject.GetComponentsInChildren<EntityBehaviour>())
                    {
                        if (entityBehaviour.manageVisibility)
                            entityBehaviour.OnSceneLoaded(scene, mode);
                    }
                }
            }
        }

        /// <summary>
        ///     Set the visibility of an entity type in the Scene view.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="isVisible"></param>
        public static void SetEntityVisibility(EntityType entityType, bool isVisible)
        {
            foreach (var entity in Object.FindObjectsOfType<Entity>())
                if (entity.entityType == entityType)
                {
                    var mr = entity.GetComponent<MeshRenderer>();
                    if (mr != null) mr.enabled = isVisible;
                }
        }

        /// <summary>
        ///     Registered to EditorApplication.onPlaymodeStateChanged
        /// </summary>
        private static void OnPlayModeStateChanged()
        {
            var isPlaying = EditorApplication.isPlaying;
            var orWillPlay = EditorApplication.isPlayingOrWillChangePlaymode;

            // if these two don't match, that means it's the call prior to actually engaging
            // whatever state. when entering play mode it doesn't make a difference, but on
            // exiting it's the difference between a scene reload and the reloaded scene.
            if (isPlaying != orWillPlay)
                return;

            var isEntering = isPlaying && orWillPlay;

            foreach (var entityBehaviour in Resources.FindObjectsOfTypeAll<EntityBehaviour>())
                if (entityBehaviour.manageVisibility)
                    if (isEntering)
                        entityBehaviour.OnEnterPlayMode();

            if (!isEntering)
                return;
        }
    }
}
