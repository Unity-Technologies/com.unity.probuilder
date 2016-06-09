using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System;
using System.IO;
using System.Collections;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Helper functions that are only available in the Editor.
	 */
	public static class pb_EditorMeshUtility
	{
		// @todo make customizable
		const string PB_MESH_CACHE = "Assets/ProCore/ProBuilder/ProBuilderMeshCache";

		/**
		 * Optmizes the mesh geometry, and generates a UV2 channel (if automatic lightmap generation is enabled).
		 * Also sets the pb_Object to 'Dirty' so that changes are stored.
		 */
		public static void Optimize(this pb_Object InObject, bool forceRebuildUV2 = false)
		{
			Mesh mesh = InObject.msh;

			if(mesh == null || mesh.vertexCount < 1)
				return;

			// if generating UV2, the process is to manually split the mesh into individual triangles,
			// generate uv2, then re-assemble with vertex collapsing where possible.
			// if not generating uv2, just collapse vertices.
			if(!pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation) || forceRebuildUV2)
			{
				pb_Vertex[] vertices = pb_MeshUtility.GeneratePerTriangleMesh(mesh);

				float time = Time.realtimeSinceStartup;				
				Vector2[] uv2 = Unwrapping.GeneratePerTriangleUV(mesh);

				// If GenerateUV2() takes longer than 3 seconds (!), show a warning prompting user
				// to disable auto-uv2 generation.
				if( (Time.realtimeSinceStartup - time) > 3f )
					Debug.LogWarning(string.Format("Generate UV2 for \"{0}\" took {1} seconds!  You may want to consider disabling Auto-UV2 generation in the `Preferences > ProBuilder` tab.", InObject.name, (Time.realtimeSinceStartup - time).ToString("F2")));

				if(uv2.Length == vertices.Length)
				{
					for(int i = 0; i < uv2.Length; i++)
					{
						vertices[i].uv2 = uv2[i];
						vertices[i].hasUv2 = true;
					}
				}
				else
				{
					Debug.LogWarning("Generate UV2 failed - the returned size of UV2 array != mesh.vertexCount");
				}

				pb_MeshUtility.CollapseSharedVertices(mesh, vertices);
			}
			else
			{
				pb_MeshUtility.CollapseSharedVertices(mesh);
			}

			// UnityEngine.Mesh.Optimize
			mesh.Optimize();

			if(pb_Preferences_Internal.GetBool(pb_Constant.pbMeshesAreAssets))
				TryCacheMesh(InObject);

			EditorUtility.SetDirty(InObject);
		}

		public static void TryCacheMesh(pb_Object pb)
		{
			Mesh mesh = pb.msh;

			// check for an existing mesh in the mesh cache and update or create a new one so
			// as not to clutter the scene yaml.
			string mesh_path = AssetDatabase.GetAssetPath(mesh);

			// if mesh is already an asset any changes will already have been applied since 
			// pb_Object is directly modifying the mesh asset
			if(string.IsNullOrEmpty(mesh_path))
			{
				// at the moment the asset_guid is only used to name the mesh something unique
				string guid = pb.asset_guid;

				if(string.IsNullOrEmpty(guid))
				{
					guid = Guid.NewGuid().ToString("N");
					pb.asset_guid = guid;
				}

				string path = string.Format("{0}/{1}.asset", PB_MESH_CACHE, guid);

				if(!Directory.Exists(PB_MESH_CACHE))
					Directory.CreateDirectory(PB_MESH_CACHE);

				Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
				
				// a mesh already exists in the cache for this pb_Object
				if(m != null)
				{
					if(mesh != m)
					{
						// prefab instances should always point to the same mesh
						if(pb_Editor_Utility.IsPrefabInstance(pb.gameObject) || pb_Editor_Utility.IsPrefabRoot(pb.gameObject))
						{
							// Debug.Log("reconnect prefab to mesh");
			
							// use the most recent mesh iteration (when undoing for example)
							pb_MeshUtility.CopyTo(mesh, m);

							GameObject.DestroyImmediate(mesh);
							pb.gameObject.GetComponent<MeshFilter>().sharedMesh = m;

							// also set the MeshCollider if it exists
							MeshCollider mc = pb.gameObject.GetComponent<MeshCollider>();
							if(mc != null) mc.sharedMesh = m;
							return;
						}
						else
						{
							// duplicate mesh
							// Debug.Log("create new mesh in cache from disconnect");
							pb.asset_guid = Guid.NewGuid().ToString("N");
							path = string.Format("{0}/{1}.asset", PB_MESH_CACHE, pb.asset_guid);
						}
					}	
					else
					{
						Debug.LogWarning("Mesh found in cache and scene mesh references match, but pb.asset_guid doesn't point to asset.  Please report the circumstances leading to this event to Karl.");
					}
				}
				// else
				// {
				// 	Debug.Log("no cache found, creating new");
				// }

				AssetDatabase.CreateAsset(mesh, path);
			}
		}

		public static Mesh GetCachedMesh(pb_Object pb)
		{
			string guid = pb.asset_guid;
			string path = string.Format("{0}/{1}.asset", PB_MESH_CACHE, guid);
			Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
			return m;
		}
	}
}
