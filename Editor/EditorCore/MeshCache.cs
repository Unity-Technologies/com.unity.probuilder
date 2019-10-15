using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [FilePathAttribute("Assets/ProBuilder/MeshDatabase.asset", FilePathAttribute.Location.ProjectFolder)]
    class MeshCache : ScriptableSingleton<MeshCache>, ISerializationCallbackReceiver
    {
        // todo Release mesh from MeshCache.m_MeshLibrary when deleting
        // todo Update meshLibrary when instance IDs are changed

        internal static void InternalReset()
        {
            instance.m_MeshLibrary.Clear();
            Save();
        }

        internal static void Save()
        {
            Debug.Log(JsonUtility.ToJson(instance, true));
            instance.Save(true);
        }

        const string k_MeshCacheDirectoryName = "ProBuilderMeshCache";
        static string k_MeshCacheDirectory = "Assets/ProBuilder Data/ProBuilderMeshCache";

        [Serializable]
        struct MeshAndReferenceList
        {
            public Mesh mesh;
            public List<ProBuilderGuid> references;
        }

        [SerializeField]
        MeshAndReferenceList[] m_MeshLibrarySerialized;

        Dictionary<Mesh, List<ProBuilderGuid>> m_MeshLibrary = new Dictionary<Mesh, List<ProBuilderGuid>>();

        static List<ProBuilderGuid> s_MeshReferences;

        internal static void Register(ProBuilderMesh mesh)
        {
            if (instance.m_MeshLibrary.TryGetValue(mesh.mesh, out s_MeshReferences))
                s_MeshReferences.Add(mesh.assetInfo.guid);
            else
                instance.m_MeshLibrary.Add(mesh.mesh, new List<ProBuilderGuid>() { mesh.assetInfo.guid });
        }

        internal static void EnsureMeshAssetIsOwnedByComponent(ProBuilderMesh mesh)
        {
            if (mesh.mesh == null)
                mesh.CreateNewSharedMesh();

            var meshAsset = mesh.mesh;
            var guid = mesh.assetInfo.guid;

            if (instance.m_MeshLibrary.TryGetValue(meshAsset, out s_MeshReferences))
            {
                if (s_MeshReferences.Count < 2)
                    return;

                if (s_MeshReferences.Contains(guid))
                    s_MeshReferences.Remove(guid);

                mesh.CreateNewSharedMesh();
            }

            if (instance.m_MeshLibrary.TryGetValue(meshAsset, out s_MeshReferences))
                s_MeshReferences.Add(guid);
            else
                instance.m_MeshLibrary.Add(meshAsset, new List<ProBuilderGuid>() { guid });
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
                Guid guid = mesh.assetInfo.guid;

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
                            DestroyImmediate(meshAsset);
                            mesh.gameObject.GetComponent<MeshFilter>().sharedMesh = m;

                            // also set the MeshCollider if it exists
                            MeshCollider mc = mesh.gameObject.GetComponent<MeshCollider>();
                            if (mc != null) mc.sharedMesh = m;
                            return;
                        }
                        else
                        {
                            // duplicate mesh
                            Debug.LogError("duplicate mesh found in TryCacheMesh");
//                            mesh.assetGuid = Guid.NewGuid().ToString("N");
//                            path = string.Format("{0}/{1}.asset", meshCacheDirectory, mesh.assetGuid);
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
            mesh = pb.assetInfo.mesh != null ? pb.assetInfo.mesh : pb.mesh;

            if (mesh != null)
            {
                string meshPath = AssetDatabase.GetAssetPath(mesh);

                if (!string.IsNullOrEmpty(meshPath))
                {
                    path = meshPath;
                    return true;
                }
            }

            // Legacy path
            string meshCacheDirectory = GetMeshCacheDirectory(false);
            Guid guid = pb.assetInfo.guid;// assetGuid;

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

        public void OnBeforeSerialize()
        {
            m_MeshLibrarySerialized = new MeshAndReferenceList[m_MeshLibrary.Count];
            int n = 0;
            foreach(var kvp in m_MeshLibrary)
                m_MeshLibrarySerialized[n++] = new MeshAndReferenceList() { mesh = kvp.Key, references = kvp.Value };
        }

        public void OnAfterDeserialize()
        {
            m_MeshLibrary.Clear();
            foreach (var kvp in m_MeshLibrarySerialized)
                m_MeshLibrary.Add(kvp.mesh, kvp.references);
        }
    }
}
