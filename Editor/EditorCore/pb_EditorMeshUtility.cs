using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Delegate to be raised when a ProBuilder component is compiled to a UnityEngine mesh.
	/// </summary>
	/// <param name="pb"></param>
	/// <param name="mesh"></param>
	public delegate void OnMeshCompiled(pb_Object pb, Mesh mesh);

	/// <summary>
	/// Delegate raised when a pb_Object is to be optimized (collapses coincident vertices). Return true to override the optimization step, false if ProBuilder should optimize the mesh internally.
	/// </summary>
	/// <param name="pb"></param>
	public delegate bool SkipMeshOptimization(pb_Object pb);

	/// <summary>
	/// Mesh editing helper functions that are only available in the Editor.
	/// </summary>
	public static class pb_EditorMeshUtility
	{
		const string k_MeshCacheDirectoryName = "ProBuilderMeshCache";
		static string k_MeshCacheDirectory = "Assets/ProBuilder Data/ProBuilderMeshCache";

		/// <summary>
		/// Subscribe to this event to be notified when ProBuilder is going to optimize a mesh (collapsing coincident vertices to a single vertex). Return true to override this process, false to let ProBuilder optimize the mesh.
		/// </summary>
		public static event SkipMeshOptimization onCheckSkipMeshOptimization = null;

		/// <summary>
		/// Callback raised when a pb_Object is built to Unity mesh.
		/// </summary>
		public static event OnMeshCompiled onMeshCompiled = null;

		/// <summary>
		/// Optmizes the mesh geometry, and generates a UV2 channel (if automatic lightmap generation is enabled).\
		/// This also sets the pb_Object to 'Dirty' so that changes are stored.
		/// </summary>
		/// <remarks>This is only applicable to Triangle meshes. Ie, Quad meshes are not affected by this function.</remarks>
		/// <param name="InObject">The pb_Object component to be compiled.</param>
		/// <param name="forceRebuildUV2">If Auto UV2 is off this parameter can be used to force UV2s to be built.</param>
		public static void Optimize(this pb_Object InObject, bool forceRebuildUV2 = false)
		{
			Mesh mesh = InObject.msh;

			if(mesh == null || mesh.vertexCount < 1)
				return;

			bool skipMeshProcessing = false;

			if (onCheckSkipMeshOptimization != null)
				skipMeshProcessing = onCheckSkipMeshOptimization(InObject);

			// @todo Support mesh compression for topologies other than Triangles.
			for(int i = 0; !skipMeshProcessing && i < mesh.subMeshCount; i++)
				if(mesh.GetTopology(i) != MeshTopology.Triangles)
					skipMeshProcessing = true;

			bool hasUv2 = false;

			if(!skipMeshProcessing)
			{
				// if generating UV2, the process is to manually split the mesh into individual triangles,
				// generate uv2, then re-assemble with vertex collapsing where possible.
				// if not generating uv2, just collapse vertices.
				if(!pb_PreferencesInternal.GetBool(pb_Constant.pbDisableAutoUV2Generation) || forceRebuildUV2)
				{
					pb_Vertex[] vertices = pb_MeshUtility.GeneratePerTriangleMesh(mesh);

					float time = Time.realtimeSinceStartup;

					UnwrapParam unwrap = pb_Lightmapping.GetUnwrapParam(InObject.unwrapParameters);

					Vector2[] uv2 = Unwrapping.GeneratePerTriangleUV(mesh, unwrap);

					// If GenerateUV2() takes longer than 3 seconds (!), show a warning prompting user
					// to disable auto-uv2 generation.
					if( (Time.realtimeSinceStartup - time) > 3f )
						pb_Log.Warning(string.Format("Generate UV2 for \"{0}\" took {1} seconds!  You may want to consider disabling Auto-UV2 generation in the `Preferences > ProBuilder` tab.", InObject.name, (Time.realtimeSinceStartup - time).ToString("F2")));

					if(uv2.Length == vertices.Length)
					{
						for(int i = 0; i < uv2.Length; i++)
						{
							vertices[i].uv2 = uv2[i];
							vertices[i].hasUv2 = true;
						}

						hasUv2 = true;
					}
					else
					{
						pb_Log.Warning("Generate UV2 failed - the returned size of UV2 array != mesh.vertexCount");
					}

					pb_MeshCompiler.CollapseSharedVertices(mesh, vertices);
				}
				else
				{
					pb_MeshCompiler.CollapseSharedVertices(mesh);
				}
			}

			if(pb_PreferencesInternal.GetBool(pb_Constant.pbManageLightmappingStaticFlag, false))
				pb_Lightmapping.SetLightmapStaticFlagEnabled(InObject, hasUv2);

			if(onMeshCompiled != null)
				onMeshCompiled(InObject, mesh);

			if(pb_PreferencesInternal.GetBool(pb_Constant.pbMeshesAreAssets))
				TryCacheMesh(InObject);

			EditorUtility.SetDirty(InObject);
		}

		internal static void TryCacheMesh(pb_Object pb)
		{
			Mesh mesh = pb.msh;

			// check for an existing mesh in the mesh cache and update or create a new one so
			// as not to clutter the scene yaml.
			string meshAssetPath = AssetDatabase.GetAssetPath(mesh);

			// if mesh is already an asset any changes will already have been applied since
			// pb_Object is directly modifying the mesh asset
			if(string.IsNullOrEmpty(meshAssetPath))
			{
				// at the moment the asset_guid is only used to name the mesh something unique
				string guid = pb.asset_guid;

				if(string.IsNullOrEmpty(guid))
				{
					guid = Guid.NewGuid().ToString("N");
					pb.asset_guid = guid;
				}

				string meshCacheDirectory = GetMeshCacheDirectory(true);

				string path = string.Format("{0}/{1}.asset", meshCacheDirectory, guid);

				Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);

				// a mesh already exists in the cache for this pb_Object
				if(m != null)
				{
					if(mesh != m)
					{
						// prefab instances should always point to the same mesh
						if(pb_EditorUtility.IsPrefabInstance(pb.gameObject) || pb_EditorUtility.IsPrefabRoot(pb.gameObject))
						{
							// Debug.Log("reconnect prefab to mesh");

							// use the most recent mesh iteration (when undoing for example)
							pb_MeshUtility.CopyTo(mesh, m);

							UnityEngine.Object.DestroyImmediate(mesh);
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
							path = string.Format("{0}/{1}.asset", meshCacheDirectory, pb.asset_guid);
						}
					}
					else
					{
						Debug.LogWarning("Mesh found in cache and scene mesh references match, but pb.asset_guid doesn't point to asset.  Please report the circumstances leading to this event to Karl.");
					}
				}

				AssetDatabase.CreateAsset(mesh, path);
			}
		}

		internal static bool GetCachedMesh(pb_Object pb, out string path, out Mesh mesh)
		{
			if (pb.msh != null)
			{
				string meshPath = AssetDatabase.GetAssetPath(pb.msh);

				if (!string.IsNullOrEmpty(meshPath))
				{
					path = meshPath;
					mesh = pb.msh;

					return true;
				}
			}

			string meshCacheDirectory = GetMeshCacheDirectory(false);
			string guid = pb.asset_guid;

			path = string.Format("{0}/{1}.asset", meshCacheDirectory, guid);
			mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

			return mesh != null;
		}

		static string GetMeshCacheDirectory(bool initializeIfMissing = false)
		{
			if (Directory.Exists(k_MeshCacheDirectory))
				return k_MeshCacheDirectory;

			string[] results = Directory.GetDirectories("Assets", k_MeshCacheDirectoryName, SearchOption.AllDirectories);

			if (results.Length < 1)
			{
				if (initializeIfMissing)
				{
					k_MeshCacheDirectory = pb_FileUtil.GetLocalDataDirectory() + "/" + k_MeshCacheDirectoryName;
					Directory.CreateDirectory(k_MeshCacheDirectory);
				}
				else
				{
					k_MeshCacheDirectory = null;
				}
			}
			else
			{
				k_MeshCacheDirectory = results.First();
			}

			return k_MeshCacheDirectory;
		}
	}
}
