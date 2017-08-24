using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Diagnostics;

namespace FbxExporters.UnitTests
{
    public class ExportPerformanceTest : ExporterTestBase
    {

        private const int NumMeshesToCombine = 15;

        // Export should not take longer than this, otherwise test fails
        private const long PerformanceThresholdMilliseconds = 30000;
        private Stopwatch m_stopwatch;
        private GameObject m_toExport;

        [SetUp]
        public void Init()
        {
            m_stopwatch = new Stopwatch ();
            m_toExport = CreateGameObjectToExport ();
        }

        [TearDown]
        public override void Term()
        {
            base.Term ();
            GameObject.DestroyImmediate (m_toExport);
        }


        /// <summary>
        /// Creates a GameObject containing a very large mesh to export.
        /// </summary>
        /// <returns>The game object to export.</returns>
        private GameObject CreateGameObjectToExport ()
        {
            CombineInstance[] combine = new CombineInstance[NumMeshesToCombine];
            GameObject spheres = GameObject.CreatePrimitive (PrimitiveType.Sphere);

            Transform sphereTransform = spheres.transform;
            MeshFilter sphereMeshFilter = spheres.GetComponent<MeshFilter> ();
            Assert.IsNotNull (sphereMeshFilter);

            for (int i = 0; i < NumMeshesToCombine; i++) {
                combine [i].mesh = sphereMeshFilter.sharedMesh;
                sphereTransform.position = new Vector3 (i, i, i);
                combine [i].transform = sphereTransform.localToWorldMatrix;
            }

            sphereMeshFilter.sharedMesh = new Mesh ();
            sphereMeshFilter.sharedMesh.name = "Spheres Mesh";
            sphereMeshFilter.sharedMesh.CombineMeshes (combine);
            return spheres;
        }

        [Test]
        public void TestPerformance ()
        {
            Assert.IsNotNull (m_toExport);

            var filename = GetRandomFbxFilePath();

            UnityEngine.Debug.unityLogger.logEnabled = false;

            m_stopwatch.Reset ();
            m_stopwatch.Start ();
            var fbxFileName = FbxExporters.Editor.ModelExporter.ExportObjects (filename, new Object[]{m_toExport}) as string;
            m_stopwatch.Stop ();

            UnityEngine.Debug.unityLogger.logEnabled = true;

            Assert.IsNotNull (fbxFileName);
            Assert.LessOrEqual(m_stopwatch.ElapsedMilliseconds, PerformanceThresholdMilliseconds);
        }
    }
}
