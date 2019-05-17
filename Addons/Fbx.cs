// todo Once we drop support for 2018.3, use optional assembly definitions
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;

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
        static Assembly s_FbxExporterAssembly = null;
        private static Assembly FbxExporterAssembly
        {
            get
            {
                if (s_FbxExporterAssembly == null)
                {
                    try
                    {
                        s_FbxExporterAssembly = System.Reflection.Assembly.Load("Unity.Formats.Fbx.Editor");
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        s_FbxExporterAssembly = null;
                    }
                }
                return s_FbxExporterAssembly;
            }
        }

        static Assembly s_FbxSdkAssembly = null;
        private static Assembly FbxSdkAssembly
        {
            get
            {
                if(s_FbxSdkAssembly == null)
                {
                    try
                    {
                        s_FbxSdkAssembly = Assembly.Load("Autodesk.Fbx");
                    }
                    catch(System.IO.FileNotFoundException)
                    {
                        s_FbxSdkAssembly = null;
                    }
                }
                return s_FbxSdkAssembly;
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
            if (FbxExporterAssembly == null || FbxSdkAssembly == null)
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
            var meshDelegate = Delegate.CreateDelegate(getMeshForComponent, Fbx.toMethod());

            registerMeshCallback.Invoke(null, new object[] { meshDelegate, true });

            /*ModelExporter.RegisterMeshCallback<ProBuilderMesh>(GetMeshForComponent, true);
            m_FbxOptions.quads = ProBuilderSettings.Get<bool>("Export::m_FbxQuads", SettingsScope.User, true);
            s_FbxIsLoaded = true;*/
        }

        static string code = @"
        using UnityEngine.ProBuilder;
        using UnityEditor.Formats.Fbx.Exporter;
        using Autodesk.Fbx;
        using UnityEngine;
        public static class __CompiledExpr__
        {
            public static bool GetMeshForComponent(ModelExporter exporter, ProBuilderMesh pmesh, FbxNode node)
            {
                Debug.Log(""Over Here"");
                return false;
            }
        }";

        static MethodInfo toMethod()
        {
            var parameters = new System.CodeDom.Compiler.CompilerParameters();
            parameters.ReferencedAssemblies.Add("mscorlib.dll");
            foreach(var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName.Name);
                if (assembly != null)
                {
                    parameters.ReferencedAssemblies.Add(assembly.Location);
                }
            }

            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            parameters.ReferencedAssemblies.Add(FbxExporterAssembly.Location);
            parameters.ReferencedAssemblies.Add(FbxSdkAssembly.Location);
            parameters.GenerateInMemory = true;

            var c = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerResults results = c.CompileAssemblyFromSource(parameters, code);
            foreach(var error in results.Errors)
            {
                Debug.Log("error: " + error.ToString());
            }
            var asm = results.CompiledAssembly;
            var compiledType = asm.GetType("__CompiledExpr__");
            return compiledType.GetMethod("GetMeshForComponent");
        }

        /*static bool GetMeshForComponent(ModelExporter exporter, ProBuilderMesh pmesh, FbxNode node)
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
        }*/
    }
}
