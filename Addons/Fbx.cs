// todo Once we drop support for 2018.3, use optional assembly definitions
#if PROBUILDER_FBX_2_0_1_OR_NEWER
using System;
using UnityEditor;
using Autodesk.Fbx;
using UnityEditor.ProBuilder;
using UnityEditor.Formats.Fbx.Exporter;

namespace UnityEngine.ProBuilder.Addons.FBX
{
    /// <summary>
    /// ProBuilder-specific options when exporting FBX files with the Unity FBX Exporter.
    /// </summary>
    class FbxOptions
    {
        /// <summary>
        /// Export mesh topology as quads if possible.
        /// </summary>
        #pragma warning disable 649
        public bool quads;
        #pragma warning restore 649
    }

    /// <summary>
    /// Provides some additional functionality when the FbxSdk and FbxExporter packages are available in the project.
    /// </summary>
    [InitializeOnLoad]
    static class Fbx
    {
        static bool s_FbxIsLoaded = false;

        static readonly Type[] k_ProBuilderTypes = new Type[]
        {
            typeof(BezierShape),
            typeof(PolyShape),
            typeof(Entity)
        };

        public static bool fbxEnabled { get { return s_FbxIsLoaded; } }

        static FbxOptions m_FbxOptions = new FbxOptions() {
            quads = true
        };

        static Fbx()
        {
            TryLoadFbxSupport();
        }

        static void TryLoadFbxSupport()
        {
            if (s_FbxIsLoaded)
                return;
            ModelExporter.RegisterMeshCallback<ProBuilderMesh>(GetMeshForComponent, true);
            m_FbxOptions.quads = ProBuilderSettings.Get<bool>("Export::m_FbxQuads", SettingsScope.User, true);
            s_FbxIsLoaded = true;
        }

        static bool GetMeshForComponent(ModelExporter exporter, ProBuilderMesh pmesh, FbxNode node)
        {
            Mesh mesh = new Mesh();
            MeshUtility.Compile(pmesh, mesh, m_FbxOptions.quads ? MeshTopology.Quads : MeshTopology.Triangles);
            exporter.ExportMesh(mesh, node, pmesh.GetComponent<MeshRenderer>().sharedMaterials);
            Object.DestroyImmediate(mesh);

            // probuilder can't handle mesh assets that may be externally reloaded, just strip pb stuff for now.
            foreach (var type in k_ProBuilderTypes)
            {
                var component = pmesh.GetComponent(type);
                if (component != null)
                    Object.DestroyImmediate(component);
            }

            pmesh.preserveMeshAssetOnDestroy = true;
            Object.DestroyImmediate(pmesh);

            return true;
        }
    }
}
#endif
