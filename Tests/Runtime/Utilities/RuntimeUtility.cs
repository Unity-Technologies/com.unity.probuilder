using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEngine.ProBuilder.Tests
{
    public static class RuntimeUtility
    {
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
                    Object.DestroyImmediate(m_Shapes[i].gameObject);
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

        const MeshArrays k_DefaultMeshArraysCompare = ~MeshArrays.Lightmap;

        public static void AssertSequenceEqual<T>(IList<T> left, IList<T> right)
        {
            Assert.AreEqual(left.Count, right.Count, "Count");

            for (int i = 0, c = left.Count; i < c; i++)
                Assert.AreEqual(left[i], right[i], "index " + i);
        }

        public static void AssertMeshIsValid(ProBuilderMesh mesh)
        {
            Assert.That(mesh, Is.Not.Null);
            Assert.That(mesh.vertexCount > 0);
            Assert.That(mesh.positionsInternal.Length, Is.EqualTo(mesh.vertexCount));
            Assert.That(mesh.faceCount, Is.GreaterThan(0));
        }

        public static void AssertMeshAttributesValid(Mesh mesh)
        {
            int vertexCount = mesh.vertexCount;

            Vector3[] positions = mesh.vertices;
            Color[] colors = mesh.colors;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Vector2[] uv0s = mesh.uv;
            Vector2[] uv2s = mesh.uv2;
            List<Vector4> uv3s = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();
            mesh.GetUVs(2, uv3s);
            mesh.GetUVs(3, uv4s);

            bool _hasPositions = positions != null && positions.Length == vertexCount;
            bool _hasColors = colors != null && colors.Length == vertexCount;
            bool _hasNormals = normals != null && normals.Length == vertexCount;
            bool _hasTangents = tangents != null && tangents.Length == vertexCount;
            bool _hasUv0 = uv0s != null && uv0s.Length == vertexCount;
            bool _hasUv2 = uv2s != null && uv2s.Length == vertexCount;
            bool _hasUv3 = uv3s.Count == vertexCount;
            bool _hasUv4 = uv4s.Count == vertexCount;

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
        public static bool AssertAreEqual(Mesh expected, Mesh result, MeshArrays compare = k_DefaultMeshArraysCompare,
            string message = null)
        {
            int vertexCount = expected.vertexCount;
            int subMeshCount = expected.subMeshCount;

            Assert.AreEqual(vertexCount, result.vertexCount, expected.name + " != " + result.name + " (submesh count)");
            Assert.AreEqual(subMeshCount, result.subMeshCount,
                expected.name + " != " + result.name + " (submesh count)");

            Vertex[] leftVertices = expected.GetVertices();
            Vertex[] rightVertices = result.GetVertices();

            for (int i = 0; i < vertexCount; i++)
                Assert.True(leftVertices[i].Equals(rightVertices[i], compare),
                    expected.name + " != " + result.name + "\nExpected\n" + leftVertices[i].ToString("F5") +
                    "\n---\nReceived:\n" + rightVertices[i].ToString("F5"));

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

        public static string GetResourcesPath<T>(string assetName, int methodOffset = 0)
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

            string methodName = calling.GetMethod().Name;

            return string.Format("{0}/{1}/{2}/{3}",
                typeof(T).ToString(),
                Path.GetFileNameWithoutExtension(filePath),
                methodName,
                assetName);
        }

        public static SimpleTuple<ProBuilderMesh, Face> CreateCubeWithNonContiguousMergedFace()
        {
            var cube = ShapeFactory.Instantiate<Cube>();

            Assume.That(cube, Is.Not.Null);

            int index = 1;
            Face a = cube.faces[0], b = cube.faces[index++];
            Assume.That(a, Is.Not.Null);
            Assume.That(b, Is.Not.Null);

            while (FacesAreAdjacent(cube, a, b) && index < cube.faceCount)
                b = cube.faces[index++];

            Assume.That(FacesAreAdjacent(cube, a, b), Is.False);

            var res = MergeElements.Merge(cube, new Face[] { a, b });

            return new SimpleTuple<ProBuilderMesh, Face>(cube, res);
        }

        static bool FacesAreAdjacent(ProBuilderMesh mesh, Face a, Face b)
        {
            for (int i = 0, c = a.edgesInternal.Length; i < c; i++)
            {
                var ea = mesh.GetSharedVertexHandleEdge(a.edgesInternal[i]);

                for (int n = 0; n < b.edgesInternal.Length; n++)
                {
                    var eb = mesh.GetSharedVertexHandleEdge(b.edgesInternal[n]);

                    if (ea == eb)
                        return true;
                }
            }

            return false;
        }

        static Material s_RedMaterial = null;
        public static Material redMaterial
        {
            get
            {
                if (s_RedMaterial == null)
                {
                    s_RedMaterial = new Material(Shader.Find("Standard"));
                    s_RedMaterial.color = new Color32(0xFF, 0x00, 0x51, 0xFF);
                }

                return s_RedMaterial;
            }
        }

        static Material s_BlueMaterial = null;
        public static Material blueMaterial
        {
            get
            {
                if (s_BlueMaterial == null)
                {
                    s_BlueMaterial = new Material(Shader.Find("Standard"));
                    s_BlueMaterial.color = new Color32(0x00, 0x8D, 0xFF, 0xFF);
                }

                return s_BlueMaterial;
            }
        }

        static Material s_GreenMaterial = null;
        public static Material greenMaterial
        {
            get
            {
                if (s_GreenMaterial == null)
                {
                    s_GreenMaterial = new Material(Shader.Find("Standard"));
                    s_GreenMaterial.color = new Color32(0x37, 0xFF, 0x00, 0xFF);
                }

                return s_GreenMaterial;
            }
        }
    }
}
