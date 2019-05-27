// todo Once we drop support for 2018.3, use optional assembly definitions
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEditor.ProBuilder;

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
        private static Assembly FbxExporterAssembly
        {
            get
            {
                try
                {
                    return Assembly.Load("Unity.Formats.Fbx.Editor");
                }
                catch (System.IO.FileNotFoundException)
                {
                    return null;
                }
            }
        }
        
        static readonly Type[] k_ProBuilderTypes = new Type[]
        {
            typeof(BezierShape),
            typeof(PolyShape),
            typeof(Entity)
        };

        static FbxOptions m_FbxOptions = new FbxOptions() {
            quads = true
        };

        static Fbx()
        {
            TryLoadFbxSupport();
        }

        static void TryLoadFbxSupport()
        {
            if (FbxExporterAssembly == null)
            {
                return;
            }

            var modelExporter = FbxExporterAssembly.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter");
            var registerMeshCallback = modelExporter.GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "RegisterMeshCallback").First(x => x.ContainsGenericParameters);
            registerMeshCallback = registerMeshCallback.MakeGenericMethod(typeof(ProBuilderMesh));

            var getMeshForComponent = FbxExporterAssembly.GetTypes()
               .Where(t => t.BaseType == typeof(MulticastDelegate) && t.Name.StartsWith("GetMeshForComponent"))
               .First(t => t.ContainsGenericParameters);
            
            getMeshForComponent = getMeshForComponent.MakeGenericType(typeof(ProBuilderMesh));
            var meshDelegate = Delegate.CreateDelegate(getMeshForComponent, typeof(Fbx).GetMethod("GetMeshForComponent", BindingFlags.NonPublic | BindingFlags.Static));

            registerMeshCallback.Invoke(null, new object[] { meshDelegate, true });
            
            m_FbxOptions.quads = ProBuilderSettings.Get<bool>("Export::m_FbxQuads", SettingsScope.User, true);
        }

        static bool GetMeshForComponent(object exporter, ProBuilderMesh pmesh, object node)
        {
            Mesh mesh = new Mesh();
            MeshUtility.Compile(pmesh, mesh, m_FbxOptions.quads ? MeshTopology.Quads : MeshTopology.Triangles);

            // using reflection to call: exporter.ExportMesh(mesh, node, pmesh.GetComponent<MeshRenderer>().sharedMaterials)
            var pMeshRenderer = pmesh.GetComponent<MeshRenderer>();
            var sharedMaterials = pMeshRenderer ? pMeshRenderer.sharedMaterials : null;
            var exportMeshMethod = exporter.GetType().GetMethod("ExportMesh", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Mesh), node.GetType(), typeof(Material[]) }, null);
            exportMeshMethod.Invoke(exporter, new object[] { mesh, node, sharedMaterials });

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
