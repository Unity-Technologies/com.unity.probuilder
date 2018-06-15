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

		static void OnDestroyObject(ProBuilderMesh pb)
		{
			if(PreferencesInternal.GetBool(PreferenceKeys.pbMeshesAreAssets))
			{
				PrefabType type = PrefabUtility.GetPrefabType(pb.gameObject);

				if( type == PrefabType.Prefab ||
					type == PrefabType.PrefabInstance ||
					type == PrefabType.DisconnectedPrefabInstance )
				{
					// Debug.Log("will not destroy prefab mesh");
				}
				else
				{
					string cache_path;
					Mesh cache_mesh;

					// if it is cached but not a prefab instance or root, destroy the mesh in the cache
					// otherwise go ahead and destroy as usual
					if( EditorMeshUtility.GetCachedMesh(pb, out cache_path, out cache_mesh) )
					{
						// on entering / exiting play mode unity instances everything and destroys the scene,
						// which nukes the mesh cache.  don't do this.
						bool isPlaying = EditorApplication.isPlaying;
						bool orWillPlay = EditorApplication.isPlayingOrWillChangePlaymode;

						if( isPlaying || orWillPlay )
							return;

						SelectionUtility.Remove(pb);
						AssetDatabase.DeleteAsset(cache_path);
					}
					else
					{
						Object.DestroyImmediate(pb.mesh);
					}
				}
			}
			else
			{
				string path = AssetDatabase.GetAssetPath(pb.mesh);

				// If the pb_Object is backed by a Mesh asset don't destroy it.
				if(string.IsNullOrEmpty(path))
					Object.DestroyImmediate(pb.mesh);
			}
		}
	}
}
