using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEditor;
#if UNITY_2019_2_OR_NEWER
using System.Reflection;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

namespace UnityEngine.ProBuilder.Tests.Framework
{
    abstract class TemporaryAssetTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!Directory.Exists(TestUtility.temporarySavedAssetsDirectory))
                Directory.CreateDirectory(TestUtility.temporarySavedAssetsDirectory);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(TestUtility.temporarySavedAssetsDirectory))
                Directory.Delete(TestUtility.temporarySavedAssetsDirectory, true);

            var meta = TestUtility.temporarySavedAssetsDirectory;

            if (meta.EndsWith("/") || meta.EndsWith("\\"))
                meta = meta.Substring(0, meta.Length - 1);

            File.Delete(meta + ".meta");

            AssetDatabase.Refresh();
        }
    }

    public static class TestUtility
    {
        const MeshArrays k_DefaultMeshArraysCompare = ~MeshArrays.Lightmap;

        static readonly string k_TempDirectory = "Assets/ProBuilderUnitTestsTemp/";
        static readonly string k_TemplatesDirectory = testsRootDirectory + "/Templates/";
        static readonly string k_RedMaterialPath = testsRootDirectory + "/Materials/Red.mat";
        static readonly string k_BlueMaterialPath = testsRootDirectory + "/Materials/Blue.mat";
        static readonly string k_GreenMaterialPath = testsRootDirectory + "/Materials/Green.mat";

        public static string templatesDirectory
        {
            get { return k_TemplatesDirectory; }
        }

        public static string testsRootDirectory
        {
            get
            {
#if UNITY_2019_2_OR_NEWER
                var packageName = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()).name;
                return "Packages/" + packageName + "/Tests";
#else
                if (Directory.Exists("Packages/com.unity.probuilder/Tests"))
                    return "Packages/com.unity.probuilder/Tests";
                return "Packages/com.unity.probuilder.tests/Tests";
#endif
            }
        }

        public static string temporarySavedAssetsDirectory
        {
            get { return k_TempDirectory; }
        }

        public class BuiltInPrimitives : IDisposable, IEnumerable<ProBuilderMesh>
        {
            ProBuilderMesh[] m_Shapes;

            public static ProBuilderMesh[] GetBasicShapes()
            {
                var shapes = Enum.GetValues(typeof(ShapeType)) as ShapeType[];
                ProBuilderMesh[] primitives = new ProBuilderMesh[shapes.Length];
                for (int i = 0, c = shapes.Length; i < c; i++)
                {
                    primitives[i] = ShapeGenerator.CreateShape(shapes[i]);
                    primitives[i].GetComponent<MeshFilter>().sharedMesh.name = shapes[i].ToString();
                }
                return primitives;
            }

            public BuiltInPrimitives()
            {
                m_Shapes = GetBasicShapes();
            }

            public int Count { get { return m_Shapes.Length; } }

            public ProBuilderMesh this[int i]
            {
                get { return m_Shapes[i]; }
                set { m_Shapes[i] = value; }
            }

            public void Dispose()
            {
                for (int i = 0, c = m_Shapes.Length; i < c; i++)
                    UObject.DestroyImmediate(m_Shapes[i].gameObject);
            }

            IEnumerator<ProBuilderMesh> IEnumerable<ProBuilderMesh>.GetEnumerator()
            {
                return ((IEnumerable<ProBuilderMesh>)m_Shapes).GetEnumerator();
            }

            public IEnumerator GetEnumerator()
            {
                return m_Shapes.GetEnumerator();
            }
        }

        /// <summary>
        /// Convert a full path to one relative to the project directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static string ToAssetPath(string path)
        {
            return path.Replace("\\", "/").Replace(Application.dataPath, "Assets/");
        }

        public static void AssertSequenceEqual<T>(IList<T> left, IList<T> right)
        {
            Assert.AreEqual(left.Count, right.Count, "Count");

            for (int i = 0, c = left.Count; i < c; i++)
                Assert.AreEqual(left[i], right[i], "index " + i);
        }

        public static void AssertMeshAttributesValid(Mesh mesh)
        {
            int vertexCount = mesh.vertexCount;

            Vector3[] positions = mesh.vertices;
            Color[] colors      = mesh.colors;
            Vector3[] normals   = mesh.normals;
            Vector4[] tangents  = mesh.tangents;
            Vector2[] uv0s      = mesh.uv;
            Vector2[] uv2s      = mesh.uv2;
            List<Vector4> uv3s = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();
            mesh.GetUVs(2, uv3s);
            mesh.GetUVs(3, uv4s);

            bool _hasPositions = positions != null && positions.Count() == vertexCount;
            bool _hasColors = colors != null && colors.Count() == vertexCount;
            bool _hasNormals = normals != null && normals.Count() == vertexCount;
            bool _hasTangents = tangents != null && tangents.Count() == vertexCount;
            bool _hasUv0 = uv0s != null && uv0s.Count() == vertexCount;
            bool _hasUv2 = uv2s != null && uv2s.Count() == vertexCount;
            bool _hasUv3 = uv3s.Count() == vertexCount;
            bool _hasUv4 = uv4s.Count() == vertexCount;

            for (int i = 0; i < vertexCount; i++)
            {
                if (_hasPositions)
                {
                    Assert.IsFalse(float.IsNaN(positions[i].x), "mesh attribute \"position\" is NaN");
                    Assert.IsFalse(float.IsNaN(positions[i].y), "mesh attribute \"position\" is NaN");
                    Assert.IsFalse(float.IsNaN(positions[i].z), "mesh attribute \"position\" is NaN");
                }

                if (_hasColors)
                {
                    Assert.IsFalse(float.IsNaN(colors[i].r), "mesh attribute \"color\" is NaN");
                    Assert.IsFalse(float.IsNaN(colors[i].g), "mesh attribute \"color\" is NaN");
                    Assert.IsFalse(float.IsNaN(colors[i].b), "mesh attribute \"color\" is NaN");
                    Assert.IsFalse(float.IsNaN(colors[i].a), "mesh attribute \"color\" is NaN");
                }

                if (_hasNormals)
                {
                    Assert.IsFalse(float.IsNaN(normals[i].x), "mesh attribute \"normal\" is NaN");
                    Assert.IsFalse(float.IsNaN(normals[i].y), "mesh attribute \"normal\" is NaN");
                    Assert.IsFalse(float.IsNaN(normals[i].z), "mesh attribute \"normal\" is NaN");
                }

                if (_hasTangents)
                {
                    Assert.IsFalse(float.IsNaN(tangents[i].x), "mesh attribute \"tangent\" is NaN");
                    Assert.IsFalse(float.IsNaN(tangents[i].y), "mesh attribute \"tangent\" is NaN");
                    Assert.IsFalse(float.IsNaN(tangents[i].z), "mesh attribute \"tangent\" is NaN");
                    Assert.IsFalse(float.IsNaN(tangents[i].w), "mesh attribute \"tangent\" is NaN");
                }

                if (_hasUv0)
                {
                    Assert.IsFalse(float.IsNaN(uv0s[i].x), "mesh attribute \"uv0\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv0s[i].y), "mesh attribute \"uv0\" is NaN");
                }

                if (_hasUv2)
                {
                    Assert.IsFalse(float.IsNaN(uv2s[i].x), "mesh attribute \"uv2\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv2s[i].y), "mesh attribute \"uv2\" is NaN");
                }

                if (_hasUv3)
                {
                    Assert.IsFalse(float.IsNaN(uv3s[i].x), "mesh attribute \"uv3\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv3s[i].y), "mesh attribute \"uv3\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv3s[i].z), "mesh attribute \"uv3\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv3s[i].w), "mesh attribute \"uv3\" is NaN");
                }

                if (_hasUv4)
                {
                    Assert.IsFalse(float.IsNaN(uv4s[i].x), "mesh attribute \"uv4\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv4s[i].y), "mesh attribute \"uv4\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv4s[i].z), "mesh attribute \"uv4\" is NaN");
                    Assert.IsFalse(float.IsNaN(uv4s[i].w), "mesh attribute \"uv4\" is NaN");
                }
            }
        }

        /// <summary>
        /// Compare two meshes for value-wise equality.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool AssertAreEqual(Mesh expected, Mesh result, MeshArrays compare = k_DefaultMeshArraysCompare, string message = null)
        {
            int vertexCount = expected.vertexCount;
            int subMeshCount = expected.subMeshCount;

            Assert.AreEqual(vertexCount, result.vertexCount, expected.name + " != " + result.name + " (submesh count)");
            Assert.AreEqual(subMeshCount, result.subMeshCount, expected.name + " != " + result.name + " (submesh count)");

            Vertex[] leftVertices = expected.GetVertices();
            Vertex[] rightVertices = result.GetVertices();

            for (int i = 0; i < vertexCount; i++)
                Assert.True(leftVertices[i].Equals(rightVertices[i], compare),
                    expected.name + " != " + result.name + "\nExpected\n" + leftVertices[i].ToString("F5") + "\n---\nReceived:\n" + rightVertices[i].ToString("F5"));

            List<int> leftIndices = new List<int>();
            List<int> rightIndices = new List<int>();

            for (int i = 0; i < subMeshCount; i++)
            {
                uint indexCount = expected.GetIndexCount(i);

                Assert.AreEqual(expected.GetTopology(i), result.GetTopology(i));
                Assert.AreEqual(indexCount, result.GetIndexCount(i));

                expected.GetIndices(leftIndices, i);
                result.GetIndices(rightIndices, i);

                for (int n = 0; n < indexCount; n++)
                    Assert.AreEqual(leftIndices[n], rightIndices[n], message);
            }

            return true;
        }

        /// <summary>
        /// Compare two meshes for value-wise inequality.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool AssertMeshesAreEqual(Mesh expected, Mesh result)
        {
            int vertexCount = expected.vertexCount;
            int subMeshCount = expected.subMeshCount;

            string prefix = string.Format("Expected {0}, Was {1}\n",
                expected == null ? "null" : expected.name,
                result == null ? "null" : result.name);

            Assert.AreEqual(vertexCount, result.vertexCount, prefix + "Vertex count");
            Assert.AreEqual(subMeshCount, result.subMeshCount, prefix + "Submesh count");

            Vertex[] leftVertices = expected.GetVertices();
            Vertex[] rightVertices = result.GetVertices();

            for (int i = 0; i < vertexCount; i++)
                Assert.IsTrue(leftVertices[i].Equals(rightVertices[i]), string.Format("{0} Vertices are not equal.\nExpected:\n{1}\nReceived:\n{2}",
                    prefix,
                    leftVertices[i].ToString("F2"),
                    rightVertices[i].ToString("F2")));

            List<int> leftIndices = new List<int>();
            List<int> rightIndices = new List<int>();

            for (int i = 0; i < subMeshCount; i++)
            {
                uint indexCount = expected.GetIndexCount(i);

                Assert.AreEqual(expected.GetTopology(i), result.GetTopology(i), prefix + "Mesh topology");
                Assert.AreEqual(indexCount, result.GetIndexCount(i), prefix + "Submesh index count");

                expected.GetIndices(leftIndices, i);
                result.GetIndices(rightIndices, i);

                for (int n = 0; n < indexCount; n++)
                    Assert.AreEqual(leftIndices[n], rightIndices[n], prefix + "Index mismatch");
            }

            return true;
        }

        /// <summary>
        /// Compare two meshes for value-wise inequality.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool MeshesAreEqual(Mesh expected, Mesh result)
        {
            int vertexCount = expected.vertexCount;
            int subMeshCount = expected.subMeshCount;

            if (vertexCount != result.vertexCount)
                return false;

            if (subMeshCount != result.subMeshCount)
                return false;

            Vertex[] leftVertices = expected.GetVertices();
            Vertex[] rightVertices = result.GetVertices();

            for (int i = 0; i < vertexCount; i++)
                if (!leftVertices[i].Equals(rightVertices[i]))
                    return false;

            List<int> leftIndices = new List<int>();
            List<int> rightIndices = new List<int>();

            for (int i = 0; i < subMeshCount; i++)
            {
                uint indexCount = expected.GetIndexCount(i);

                if (expected.GetTopology(i) != result.GetTopology(i))
                    return false;

                if (indexCount != result.GetIndexCount(i))
                    return false;

                expected.GetIndices(leftIndices, i);
                result.GetIndices(rightIndices, i);

                for (int n = 0; n < indexCount; n++)
                    if (leftIndices[n] != rightIndices[n])
                        return false;
            }

            return true;
        }

        public static string GetTemplatePath<T>(string assetName, int methodOffset = 0)
        {
            StackTrace trace = new StackTrace(1 + methodOffset, true);
            StackFrame calling = trace.GetFrame(0);

            string filePath = calling.GetFileName();

            if (string.IsNullOrEmpty(filePath))
            {
                UnityEngine.Debug.LogError(
                    "Cannot generate mesh templates directory path from calling method. Please use the explicit SaveMeshTemplate overload.");
                return null;
            }

            // Get the calling file path relative to the `Tests/` directory
            string fullFilePath = Path.GetFullPath(filePath).Replace("\\", "/");
            string fullTestRootPath = Path.GetFullPath(testsRootDirectory).Replace("\\", "/");
            string relativeTemplatePath = fullFilePath.Replace(fullTestRootPath, "");
            string relativeTemplateDir = Path.GetDirectoryName(relativeTemplatePath).Replace("\\", "/").TrimStart('/');

            string methodName = calling.GetMethod().Name;

            return string.Format("{0}/{1}/{2}/{3}/{4}.asset",
                typeof(T).ToString(),
                relativeTemplateDir,
                Path.GetFileNameWithoutExtension(filePath),
                methodName,
                assetName);
        }

        /// <summary>
        /// Get a mesh saved from the same path with name. Use SaveAssetTemplate to automatically generate this path.
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAssetTemplate<T>(string name) where T : UObject
        {
            string assetPath = templatesDirectory + GetTemplatePath<T>(name, 1);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsFalse(asset == null, "Failed loading asset template " + name + " at path " + assetPath);
            return asset;
        }

        public static T GetAssetTemplateWithPath<T>(string pathRelativeToTemplatesDirectory) where T : UObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(pathRelativeToTemplatesDirectory);
            Assert.IsFalse(asset == null, "Failed loading asset template: " + pathRelativeToTemplatesDirectory);
            return asset;
        }

        /// <summary>
        /// Utility for saving test asset templates with an automatically generated path from the calling file name and
        /// method. See also GetAssetTemplate.
        /// </summary>
        /// <remarks>
        /// See CreateBasicShapes for a simple example of use.
        /// </remarks>
        /// <param name="asset"></param>
        /// <param name="name"></param>
        /// <param name="methodOffset"></param>
        /// <typeparam name="T"></typeparam>
        public static void SaveAssetTemplate<T>(T asset, string name = null, int methodOffset = 0) where T : UObject
        {
            string templatePath = GetTemplatePath<T>(string.IsNullOrEmpty(name) ? asset.name : name, methodOffset + 1);
            SaveAssetTemplateAtPath(asset, templatePath);
        }

        /// <summary>
        /// Path is relative to the "Tests/Templates/" directory. Optional flag disables overwriting.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="path"></param>
        static void SaveAssetTemplateAtPath<T>(T asset, string path, bool overwrite = true) where T : UObject
        {
            if (!path.EndsWith(".asset"))
                path += ".asset";

            string assetPath = string.Format("{0}{1}", templatesDirectory, path);
            string fullDirectoryPath = Path.GetDirectoryName(assetPath);

            if (string.IsNullOrEmpty(fullDirectoryPath))
            {
                UnityEngine.Debug.LogError("Could not save asset at path: " + assetPath);
                return;
            }

            if (!Directory.Exists(fullDirectoryPath))
                Directory.CreateDirectory(fullDirectoryPath);

            if (AssetDatabase.LoadAssetAtPath<UObject>(assetPath) != null)
            {
                if (!overwrite)
                {
                    UnityEngine.Debug.LogError("Will not overwrite existing asset at path: " + assetPath);
                    return;
                }

                if (!AssetDatabase.DeleteAsset(assetPath))
                {
                    UnityEngine.Debug.LogError("Failed to delete existing asset at path: " + assetPath);
                    return;
                }
            }

            AssetDatabase.CreateAsset(asset, assetPath);
        }

        public static string SaveAssetTemporary<T>(UObject asset) where T : UObject
        {
            if (!Directory.Exists(temporarySavedAssetsDirectory))
                Directory.CreateDirectory(temporarySavedAssetsDirectory);

            string path = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", temporarySavedAssetsDirectory, asset.name));
            AssetDatabase.CreateAsset(asset, path);
            return path;
        }

        public static void ClearTempAssets()
        {
            if (!Directory.Exists(temporarySavedAssetsDirectory))
                return;

            Directory.Delete(temporarySavedAssetsDirectory);
        }

        public static Material redMaterial
        {
            get { return AssetDatabase.LoadAssetAtPath<Material>(k_RedMaterialPath); }
        }

        public static Material blueMaterial
        {
            get { return AssetDatabase.LoadAssetAtPath<Material>(k_BlueMaterialPath); }
        }

        public static Material greenMaterial
        {
            get { return AssetDatabase.LoadAssetAtPath<Material>(k_GreenMaterialPath); }
        }
    }
}
