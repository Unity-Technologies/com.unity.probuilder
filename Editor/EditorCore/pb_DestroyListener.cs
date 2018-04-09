using UnityEngine;
using UnityEditor;

using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Listens for pb_Object::OnDestroy events and deletes or ignores meshes depending on whether or not the mesh is an asset.
	/// </summary>
	[InitializeOnLoad]
	static class pb_DestroyListener
	{
		static pb_DestroyListener()
		{
			pb_Object.onDestroyObject -= OnDestroyObject;
			pb_Object.onDestroyObject += OnDestroyObject;
		}

		static void OnDestroyObject(pb_Object pb)
		{
			if(pb_PreferencesInternal.GetBool(pb_Constant.pbMeshesAreAssets))
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
					if( pb_EditorMeshUtility.GetCachedMesh(pb, out cache_path, out cache_mesh) )
					{
						// on entering / exiting play mode unity instances everything and destroys the scene,
						// which nukes the mesh cache.  don't do this.
						bool isPlaying = EditorApplication.isPlaying;
						bool orWillPlay = EditorApplication.isPlayingOrWillChangePlaymode;

						if( isPlaying || orWillPlay )
							return;

						pb_SelectionUtility.Remove(pb);
						AssetDatabase.DeleteAsset(cache_path);
					}
					else
					{
						Object.DestroyImmediate(pb.msh);
					}
				}
			}
			else
			{
				string path = AssetDatabase.GetAssetPath(pb.msh);

				// If the pb_Object is backed by a Mesh asset don't destroy it.
				if(string.IsNullOrEmpty(path))
					Object.DestroyImmediate(pb.msh);
			}
		}
	}
}
