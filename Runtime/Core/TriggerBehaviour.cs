using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A MonoBehaviour that automatically enables and disables the parent GameObject on exiting and enterin playmode, respectively.
    /// </summary>
    [DisallowMultipleComponent]
    sealed class TriggerBehaviour : EntityBehaviour
    {
        public override void Initialize()
        {
            var collision = gameObject.GetComponent<Collider>();

            if (!collision)
                collision = gameObject.AddComponent<MeshCollider>();

            var meshCollider = collision as MeshCollider;

            if (meshCollider)
                meshCollider.convex = true;

            collision.isTrigger = true;

            SetMaterial(BuiltinMaterials.triggerMaterial);
        }

        public override void OnEnterPlayMode()
        {
            var r = GetComponent<Renderer>();

            if (r != null)
                r.enabled = false;
        }

        public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var r = GetComponent<Renderer>();

            if (r != null)
                r.enabled = false;
        }
    }
}
