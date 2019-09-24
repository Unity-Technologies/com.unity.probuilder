using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [FilePathAttribute("Library/ProBuilder/MeshDatabase.asset", FilePathAttribute.Location.ProjectFolder)]
    class MeshCache : ScriptableSingleton<MeshCache>
    {
        const string k_MeshCacheDirectoryName = "ProBuilderMeshCache";
        static string k_MeshCacheDirectory = "Assets/ProBuilder Data/ProBuilderMeshCache";

        Dictionary<Mesh, List<ProBuilderMesh>> m_MeshLibrary = new Dictionary<Mesh, List<ProBuilderMesh>>();
        static List<ProBuilderMesh> s_MeshReferences;

        internal static void EnsureMeshAssetIsOwnedByComponent(ProBuilderMesh mesh)
        {
            if (mesh.mesh == null)
                mesh.CreateNewSharedMesh();

            var meshAsset = mesh.mesh;

            if (instance.m_MeshLibrary.TryGetValue(meshAsset, out s_MeshReferences))
            {
                if (s_MeshReferences.Count < 2)
                    return;

                if (s_MeshReferences.Contains(mesh))
                    s_MeshReferences.Remove(mesh);

                mesh.CreateNewSharedMesh();
            }

            if (instance.m_MeshLibrary.TryGetValue(meshAsset, out s_MeshReferences))
                s_MeshReferences.Add(mesh);
            else
                instance.m_MeshLibrary.Add(meshAsset, new List<ProBuilderMesh>() { mesh });
        }

        internal static void TryCacheMesh(ProBuilderMesh mesh)
        {
            Mesh meshAsset = mesh.mesh;

            // check for an existing mesh in the mesh cache and update or create a new one so
            // as not to clutter the scene yaml.
            string meshAssetPath = AssetDatabase.GetAssetPath(meshAsset);

            // if mesh is already an asset any changes will already have been applied since
            // pb_Object is directly modifying the mesh asset
            if (string.IsNullOrEmpty(meshAssetPath))
            {
                // at the moment the asset_guid is only used to name the mesh something unique
                string guid = mesh.assetGuid;

                if (string.IsNullOrEmpty(guid))
                {
                    guid = Guid.NewGuid().ToString("N");
                    mesh.assetGuid = guid;
                }

                string meshCacheDirectory = GetMeshCacheDirectory(true);

                string path = string.Format("{0}/{1}.asset", meshCacheDirectory, guid);

                Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);

                // a mesh already exists in the cache for this pb_Object
                if (m != null)
                {
                    if (meshAsset != m)
                    {
                        // prefab instances should always point to the same mesh
                        if (EditorUtility.IsPrefabInstance(mesh.gameObject) || EditorUtility.IsPrefabAsset(mesh.gameObject))
                        {
                            // Debug.Log("reconnect prefab to mesh");

                            // use the most recent mesh iteration (when undoing for example)
                            UnityEngine.ProBuilder.MeshUtility.CopyTo(meshAsset, m);

                            UnityEngine.Object.DestroyImmediate(meshAsset);
                            mesh.gameObject.GetComponent<MeshFilter>().sharedMesh = m;

                            // also set the MeshCollider if it exists
                            MeshCollider mc = mesh.gameObject.GetComponent<MeshCollider>();
                            if (mc != null) mc.sharedMesh = m;
                            return;
                        }
                        else
                        {
                            // duplicate mesh
                            // Debug.Log("create new mesh in cache from disconnect");
                            mesh.assetGuid = Guid.NewGuid().ToString("N");
                            path = string.Format("{0}/{1}.asset", meshCacheDirectory, mesh.assetGuid);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Mesh found in cache and scene mesh references match, but pb.asset_guid doesn't point to asset.  Please report the circumstances leading to this event to Karl.");
                    }
                }

                AssetDatabase.CreateAsset(meshAsset, path);
            }
        }

        internal static bool GetCachedMesh(ProBuilderMesh pb, out string path, out Mesh mesh)
        {
            if (pb.mesh != null)
            {
                string meshPath = AssetDatabase.GetAssetPath(pb.mesh);

                if (!string.IsNullOrEmpty(meshPath))
                {
                    path = meshPath;
                    mesh = pb.mesh;

                    return true;
                }
            }

            string meshCacheDirectory = GetMeshCacheDirectory(false);
            string guid = pb.assetGuid;

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
                    k_MeshCacheDirectory = FileUtility.GetLocalDataDirectory() + "/" + k_MeshCacheDirectoryName;
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
