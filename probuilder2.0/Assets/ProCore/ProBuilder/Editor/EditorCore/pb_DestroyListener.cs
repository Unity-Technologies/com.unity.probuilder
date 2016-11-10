using UnityEngine;
using UnityEditor;

using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Static singleton listens for pb_Object::OnDestroy events and deletes or ignores meshes
	 *	depending on whether or not the mesh is an asset.
	 */
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
			if(pb_Preferences_Internal.GetBool(pb_Constant.pbMeshesAreAssets))
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
						pb_SelectionUtility.Remove(pb);
						AssetDatabase.DeleteAsset(cache_path);
					}
					else
					{
						GameObject.DestroyImmediate(pb.msh);
					}
				}
			}
			else
			{
				GameObject.DestroyImmediate(pb.msh);
			}
		}
	}
}
