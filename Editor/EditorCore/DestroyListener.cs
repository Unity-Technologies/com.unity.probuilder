using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Listens for pb_Object::OnDestroy events and deletes or ignores meshes depending on whether or not the mesh is an asset.
    /// </summary>
    [InitializeOnLoad]
    static class DestroyListener
    {
        static DestroyListener()
        {
            ProBuilderMesh.meshWillBeDestroyed -= OnDestroyObject;
            ProBuilderMesh.meshWillBeDestroyed += OnDestroyObject;
        }

        static void OnDestroyObject(ProBuilderMesh mesh)
        {
            if (Experimental.meshesAreAssets)
            {
                // on entering / exiting play mode unity instances everything and destroys the scene
                bool isPlaying = EditorApplication.isPlaying;
                bool orWillPlay = EditorApplication.isPlayingOrWillChangePlaymode;

                if (isPlaying || orWillPlay)
                    return;

                Mesh asset = mesh.mesh;

                if (asset == null)
                    return;

                if (!EditorUtility.IsPrefab(mesh))
                {
                    // if the asset is not in the mesh cache, still destroy it
                    if(!MeshCache.Release(mesh))
                        Object.DestroyImmediate(asset);
                }
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(mesh.mesh);

                // If the pb_Object is backed by a Mesh asset don't destroy it.
                if (string.IsNullOrEmpty(path))
                    Object.DestroyImmediate(mesh.mesh);
            }
        }
    }
}
