using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Contains helper functions for mesh editing that are only available in the Editor.
    /// </summary>
    public static class EditorMeshUtility
    {
        const string k_MeshCacheDirectoryName = "ProBuilderMeshCache";
        static string k_MeshCacheDirectory = "Assets/ProBuilder Data/ProBuilderMeshCache";

        [UserSetting("Mesh Editing", "Auto Resize Colliders", "Automatically resize colliders with mesh bounds as you edit.")]
        static Pref<bool> s_AutoResizeCollisions = new Pref<bool>("editor.autoRecalculateCollisions", false, SettingsScope.Project);

        /// <summary>
        /// Raised after a ProBuilderMesh has been successfully optimized.
        /// </summary>
        /// <seealso cref="Optimize"/>
        public static event Action<ProBuilderMesh, Mesh> meshOptimized = null;

        /// <summary>
        /// Optimizes the mesh geometry, and generates a UV2 channel if object is marked as <see cref="StaticEditorFlags.ContributeGI"/>
        /// or you set the `generateLightmapUVs` parameter to true.
        /// </summary>
        /// <remarks>This is only applicable to meshes with triangle topology. Quad meshes are not affected by this function.</remarks>
        /// <param name="mesh">The ProBuilder mesh component to optimize.</param>
        /// <param name="generateLightmapUVs">True to force ProBuilder to build UV2s if the Auto UV2 preference is disabled.</param>
        public static void Optimize(this ProBuilderMesh mesh, bool generateLightmapUVs = false)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Mesh umesh = mesh.mesh;

            if (umesh == null || umesh.vertexCount < 1)
                return;

            bool skipMeshProcessing = false;

            // @todo Support mesh compression for topologies other than Triangles.
            for (int i = 0; !skipMeshProcessing && i < umesh.subMeshCount; i++)
                if (umesh.GetTopology(i) != MeshTopology.Triangles)
                    skipMeshProcessing = true;

            if (!skipMeshProcessing)
            {
                bool autoLightmap = Lightmapping.autoUnwrapLightmapUV;

#if UNITY_2019_2_OR_NEWER
                bool lightmapUVs = generateLightmapUVs || (autoLightmap && mesh.gameObject.HasStaticFlag(StaticEditorFlags.ContributeGI));
#else
                bool lightmapUVs = generateLightmapUVs || (autoLightmap && mesh.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic));
#endif

                var usedInParticuleSystem = UnityEngine.ProBuilder.MeshUtility.IsUsedInParticleSystem(mesh);

                // if generating UV2, the process is to manually split the mesh into individual triangles,
                // generate uv2, then re-assemble with vertex collapsing where possible.
                // if not generating uv2, just collapse vertices.
                if (lightmapUVs)
                {
                    Vertex[] vertices = UnityEngine.ProBuilder.MeshUtility.GeneratePerTriangleMesh(umesh);

                    float time = Time.realtimeSinceStartup;

                    UnwrapParam unwrap = Lightmapping.GetUnwrapParam(mesh.unwrapParameters);

                    Vector2[] uv2 = Unwrapping.GeneratePerTriangleUV(umesh, unwrap);

                    // If GenerateUV2() takes longer than 3 seconds (!), show a warning prompting user to disable auto-uv2 generation.
                    if ((Time.realtimeSinceStartup - time) > 3f)
                        Log.Warning(string.Format("Generate UV2 for \"{0}\" took {1} seconds! You may want to consider disabling Auto-UV2 generation in the `Preferences > ProBuilder` tab.", mesh.name, (Time.realtimeSinceStartup - time).ToString("F2")));

                    if (uv2.Length == vertices.Length)
                    {
                        for (int i = 0; i < uv2.Length; i++)
                            vertices[i].uv2 = uv2[i];
                    }
                    else
                    {
                        Log.Warning("Generate UV2 failed. The returned size of UV2 array != mesh.vertexCount");
                    }

                    UnityEngine.ProBuilder.MeshUtility.CollapseSharedVertices(umesh, vertices);
                }
                else
                {
                    UnityEngine.ProBuilder.MeshUtility.CollapseSharedVertices(umesh);
                }

                if(usedInParticuleSystem)
                    UnityEngine.ProBuilder.MeshUtility.RestoreParticleSystem(mesh);
            }

            if (s_AutoResizeCollisions)
                RebuildColliders(mesh);

            if (meshOptimized != null)
                meshOptimized(mesh, umesh);

            if (Experimental.meshesAreAssets)
                TryCacheMesh(mesh);

            UnityEditor.EditorUtility.SetDirty(mesh);
        }

        internal static void TryCacheMesh(ProBuilderMesh pb)
        {
            Mesh mesh = pb.mesh;

            // check for an existing mesh in the mesh cache and update or create a new one so
            // as not to clutter the scene yaml.
            string meshAssetPath = AssetDatabase.GetAssetPath(mesh);

            // if mesh is already an asset any changes will already have been applied since
            // pb_Object is directly modifying the mesh asset
            if (string.IsNullOrEmpty(meshAssetPath))
            {
                // at the moment the asset_guid is only used to name the mesh something unique
                string guid = pb.assetGuid;

                if (string.IsNullOrEmpty(guid))
                {
                    guid = Guid.NewGuid().ToString("N");
                    pb.assetGuid = guid;
                }

                string meshCacheDirectory = GetMeshCacheDirectory(true);

                string path = string.Format("{0}/{1}.asset", meshCacheDirectory, guid);

                Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);

                // a mesh already exists in the cache for this pb_Object
                if (m != null)
                {
                    if (mesh != m)
                    {
                        // prefab instances should always point to the same mesh
                        if (EditorUtility.IsPrefabInstance(pb.gameObject) || EditorUtility.IsPrefabAsset(pb.gameObject))
                        {
                            // Debug.Log("reconnect prefab to mesh");

                            // use the most recent mesh iteration (when undoing for example)
                            UnityEngine.ProBuilder.MeshUtility.CopyTo(mesh, m);

                            UnityEngine.Object.DestroyImmediate(mesh);
                            pb.gameObject.GetComponent<MeshFilter>().sharedMesh = m;

                            // also set the MeshCollider if it exists
                            pb.Refresh(RefreshMask.Collisions);

                            return;
                        }
                        else
                        {
                            // duplicate mesh
                            // Debug.Log("create new mesh in cache from disconnect");
                            pb.assetGuid = Guid.NewGuid().ToString("N");
                            path = string.Format("{0}/{1}.asset", meshCacheDirectory, pb.assetGuid);
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

        /// <summary>
        /// Resizes any collider components on this mesh to match the size of the mesh bounds.
        /// </summary>
        /// <param name="mesh">The mesh target to rebuild collider volumes for.</param>
        public static void RebuildColliders(this ProBuilderMesh mesh)
        {
            mesh.mesh.RecalculateBounds();

            var bounds = mesh.mesh.bounds;

            foreach (var collider in mesh.GetComponents<Collider>())
            {
                Type t = collider.GetType();

                if (t == typeof(BoxCollider))
                {
                    ((BoxCollider)collider).center = bounds.center;
                    ((BoxCollider)collider).size = bounds.size;
                }
                else if (t == typeof(SphereCollider))
                {
                    ((SphereCollider)collider).center = bounds.center;
                    ((SphereCollider)collider).radius = Math.LargestValue(bounds.extents);
                }
                else if (t == typeof(CapsuleCollider))
                {
                    ((CapsuleCollider)collider).center = bounds.center;
                    Vector2 xy = new Vector2(bounds.extents.x, bounds.extents.z);
                    ((CapsuleCollider)collider).radius = Math.LargestValue(xy);
                    ((CapsuleCollider)collider).height = bounds.size.y;
                }
                else if (t == typeof(WheelCollider))
                {
                    ((WheelCollider)collider).center = bounds.center;
                    ((WheelCollider)collider).radius = Math.LargestValue(bounds.extents);
                }
                else if (t == typeof(MeshCollider))
                {
                    mesh.Refresh(RefreshMask.Collisions);
                }
            }
        }
    }
}
