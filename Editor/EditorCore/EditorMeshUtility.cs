using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Mesh editing helper functions that are only available in the Editor.
    /// </summary>
    public static class EditorMeshUtility
    {
        [UserSetting("Mesh Editing", "Auto Resize Colliders", "Automatically resize colliders with mesh bounds as you edit.")]
        static Pref<bool> s_AutoResizeCollisions = new Pref<bool>("editor.autoRecalculateCollisions", false, SettingsScope.Project);

        /// <value>
        /// This callback is raised after a ProBuilderMesh has been successfully optimized.
        /// </value>
        /// <seealso cref="Optimize"/>
        public static event Action<ProBuilderMesh, Mesh> meshOptimized = null;

        /// <summary>
        /// Optmizes the mesh geometry, and generates a UV2 channel (if object is marked as LightmapStatic, or generateLightmapUVs is true).
        /// </summary>
        /// <remarks>This is only applicable to meshes with triangle topology. Quad meshes are not affected by this function.</remarks>
        /// <param name="mesh">The ProBuilder mesh component to be optimized.</param>
        /// <param name="generateLightmapUVs">If the Auto UV2 preference is disabled this parameter can be used to force UV2s to be built.</param>
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
            }

            if (s_AutoResizeCollisions)
                RebuildColliders(mesh);

            if (meshOptimized != null)
                meshOptimized(mesh, umesh);

            UnityEditor.EditorUtility.SetDirty(mesh);
        }

        /// <summary>
        /// Resize any collider components on this mesh to match the size of the mesh bounds.
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
                    ((MeshCollider)collider).sharedMesh = null;
                    ((MeshCollider)collider).sharedMesh = mesh.mesh;
                }
            }
        }
    }
}
