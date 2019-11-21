using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [FilePathAttribute("Library/ProBuilder/MeshDatabase.asset", FilePathAttribute.Location.ProjectFolder)]
    class MeshCache : ScriptableSingleton<MeshCache>, ISerializationCallbackReceiver
    {
        // todo Release mesh from MeshCache.m_MeshLibrary when deleting
        // todo Update meshLibrary when instance IDs are changed

        [Serializable]
        struct MeshAndReferenceList
        {
            public Mesh mesh;
            public List<ProBuilderGuid> references;
        }

        const string k_MeshCacheDirectoryName = "ProBuilderMeshCache";
        static string k_MeshCacheDirectory = "Assets/ProBuilder Data/ProBuilderMeshCache";

        [SerializeField]
        MeshAndReferenceList[] m_MeshLibrarySerialized;

        internal Dictionary<Mesh, List<ProBuilderGuid>> m_MeshLibrary = new Dictionary<Mesh, List<ProBuilderGuid>>();

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
                m_MeshLibrary.Add(kvp.mesh, new List<ProBuilderGuid>(kvp.references));
        }

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

        internal static int GetReferenceCount(ProBuilderMesh mesh)
        {
            var asset = mesh.mesh;
            if (asset == null || !instance.m_MeshLibrary.ContainsKey(asset))
                return -1;
            return instance.m_MeshLibrary[asset].Count;
        }

        // Remove null or mesh assets with no references.
        internal static void CleanUp()
        {
            var rep = new Dictionary<Mesh, List<ProBuilderGuid>>();
            var library = instance.m_MeshLibrary;

            /**
             * Remove null keys
             */
            foreach (var kvp in library)
            {
                // because Mesh represents an unmanaged object, this situation is possible (tests "unity" null)
                if(kvp.Key != null)
                    rep.Add(kvp.Key, kvp.Value);
            }

            int oldKeyCount = library.Count;
            instance.m_MeshLibrary = library = rep;

            /**
             * Remove unreferenced meshes
             */
            var files = Directory.EnumerateFiles(k_MeshCacheDirectory, "*.asset");
            var remove = new List<string>();

            foreach (var file in files)
            {
                var path = FileUtility.GetRelativePath(file);
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                // Not a mesh, what is it doing in our *mesh* cache?
                if (mesh == null)
                    continue;
                if (!library.ContainsKey(mesh))
                    remove.Add(path);
            }

            for (int i = 0, c = remove.Count; i < c; i++)
                AssetDatabase.DeleteAsset(remove[i]);

            Debug.Log($"removed {oldKeyCount - library.Count} null keys\nremoved {remove.Count} orphaned meshes");

        }

        internal static void EnsureGuidIsUniqueToKey(ProBuilderGuid guid, Mesh key)
        {
            var remove = new List<KeyValuePair<Mesh, ProBuilderGuid>>();

            foreach (var pair in instance.m_MeshLibrary)
            {
                if (pair.Key == key)
                    continue;

                if (pair.Value.Contains(guid))
                    remove.Add(new KeyValuePair<Mesh, ProBuilderGuid>(pair.Key, guid));
            }

            Debug.Log($"removed {remove.Count} mis-linked guids");

            foreach (var pair in remove)
                Release(pair.Key, pair.Value);
        }

        internal static void Register(ProBuilderMesh mesh)
        {
            var asset = mesh.mesh;

            if (asset == null)
                throw new ArgumentNullException("mesh", "attempting to register a ProBuilderMesh with null asset");

            if (instance.m_MeshLibrary.ContainsKey(asset))
            {
                Debug.Log($"<color=green><b>register existing</b></color> {mesh.assetInfo}");

                var list = instance.m_MeshLibrary[asset];
                if(!list.Contains(mesh.assetInfo.guid))
                    list.Add(mesh.assetInfo.guid);
            }
            else
            {
                Debug.Log($"<color=purple><b>register new key</b></color> {mesh.assetInfo}");
                instance.m_MeshLibrary.Add(mesh.mesh, new List<ProBuilderGuid>() { mesh.assetInfo.guid });
            }
        }

        internal static void Remove(ProBuilderMesh mesh)
        {
            var asset = mesh.mesh;
            if (asset == null)
                return;
            Debug.Log($"remove {mesh.assetInfo}");
            var references = instance.m_MeshLibrary[asset];
            references.Remove(mesh.assetInfo.guid);
        }

        /// <summary>
        /// Destroy the asset associated with a <see cref="ProBuilderMesh"/> key. If the MeshCache is not aware of the key,
        /// the asset will not be destroyed.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns>Returns true if the asset was found in the cache and successfully destroyed. Returns false if the key is not contained in the cache.</returns>
        internal static bool Release(ProBuilderMesh mesh)
        {
            var asset = mesh.mesh;

            if (asset == null)
                return false;

            SelectionUtility.Remove(mesh);

            return Release(asset, mesh.assetInfo.guid);
        }

        internal static bool Release(Mesh key, ProBuilderGuid guid)
        {
            List<ProBuilderGuid> references;

            if (instance.m_MeshLibrary.TryGetValue(key, out references))
            {
                bool isMeshAssociatedToAsset =  references.Contains(guid);

#if DEVELOPER_MODE
                if (!references.Contains(guid))
                    Debug.LogError($"Attempting to destroy an un-associated mesh asset! {key} {guid}");
#endif

                // If no references to this key remain, destroy the asset
                if (references.Remove(guid) && references.Count < 1)
                {
                    Debug.Log($"<b><color=\"#ff0000\">MeshCache::Release(Destroy)</color> {key}, {references.Count}</b>\n<b>releasing guid:</b> {guid}");

                    instance.m_MeshLibrary.Remove(key);
                    var path = AssetDatabase.GetAssetPath(key);

                    if(!string.IsNullOrEmpty(path))
                        AssetDatabase.DeleteAsset(path);
                    else
                        DestroyImmediate(key, true);
                }
                else
                    Debug.Log($"<b><color=\"#ffff00\">MeshCache::Release(Decrement)</color> {key}, {references.Count}</b>\n<b>releasing guid:</b> {guid}");

                return true;
            }

            Debug.LogError($"Attempting to destroy un-registered mesh asset: {key} {guid}");

            return false;
        }

        internal static bool Contains(Mesh asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset");
            return instance.m_MeshLibrary.ContainsKey(asset);
        }

        internal static void EnsureMeshAssetIsOwnedByComponent(ProBuilderMesh mesh)
        {
            if (mesh.mesh == null)
                mesh.CreateNewSharedMesh();

            var asset = mesh.mesh;
            var guid = mesh.assetInfo.guid;

            if(instance.m_MeshLibrary.ContainsKey(asset))
            {
                var references = instance.m_MeshLibrary[asset];

                if (references.Count < 2)
                    return;

                if (references.Contains(guid))
                {
                    Debug.Log($"remove reference {mesh.assetInfo}");
                    references.Remove(guid);
                }

                Undo.RecordObject(mesh.filter, "Create new Mesh Asset");
                mesh.CreateNewSharedMesh();
            }

            Register(mesh);
        }

        internal static void TryCacheMesh(ProBuilderMesh mesh)
        {
            Mesh asset = mesh.mesh;

            // check for an existing mesh in the mesh cache and update or create a new one so
            // as not to clutter the scene yaml.
            string assetPath = AssetDatabase.GetAssetPath(asset);

            // if mesh is already an asset any changes will already have been applied since
            // pb_Object is directly modifying the mesh asset
            if (string.IsNullOrEmpty(assetPath))
            {
                string meshCacheDirectory = GetMeshCacheDirectory(true);
                string path = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", meshCacheDirectory, asset.name));
                Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);

                // a mesh already exists in the cache for this pb_Object
                if (m != null)
                {
                    if (asset != m)
                    {
                        // prefab instances should always point to the same mesh
                        if (EditorUtility.IsPrefabInstance(mesh.gameObject) || EditorUtility.IsPrefabAsset(mesh.gameObject))
                        {
                            // Debug.Log("reconnect prefab to mesh");
                            // use the most recent mesh iteration (when undoing for example)
                            UnityEngine.ProBuilder.MeshUtility.CopyTo(asset, m);
                            DestroyImmediate(asset);
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

                AssetDatabase.CreateAsset(asset, path);
            }
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
