#if FBX_EXPORTER

using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;

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
        static FbxOptions s_FbxOptions = new FbxOptions() {
            quads = true
        };

        static Fbx()
        {
            TryLoadFbxSupport();
        }

        static void TryLoadFbxSupport()
        {
            ModelExporter.RegisterMeshCallback<ProBuilderMesh>(GetMeshForPBComponent, true);
            s_FbxOptions.quads = ProBuilderSettings.Get<bool>("Export::m_FbxQuads", SettingsScope.User, true);
        }

        static bool GetMeshForPBComponent(ModelExporter exporter, ProBuilderMesh pmesh, Autodesk.Fbx.FbxNode node)
        {
            Mesh mesh = new Mesh();
            MeshUtility.Compile(pmesh, mesh, s_FbxOptions.quads ? MeshTopology.Quads : MeshTopology.Triangles);

            var pMeshRenderer = pmesh.GetComponent<MeshRenderer>();
            var sharedMaterials = pMeshRenderer ? pMeshRenderer.sharedMaterials : null;

            exporter.ExportMesh(mesh, node, sharedMaterials);

            Object.DestroyImmediate(mesh);

            //Need to have ExportOptions accessible to remove this reflection
            var exporterType = exporter.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "get_ExportOptions").Invoke(exporter, null).GetType();

            if(exporterType == typeof(ConvertToPrefabSettingsSerialize))
            {
                // probuilder can't handle mesh assets that may be externally reloaded, just strip pb stuff for now.
                StripProBuilderScripts.DoStrip(pmesh);
            }

            return true;
        }
    }
}

#endif
