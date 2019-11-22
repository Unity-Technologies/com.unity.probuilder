using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.ProBuilder.Tests
{
    public class MeshCacheAssetLifecycle
    {
        ProBuilderMesh m_ProBuilderMeshA;
        ProBuilderMesh m_ProBuilderMeshB;

        [OneTimeSetUp]
        public void OnTimeSetUp()
        {
            Experimental.meshesAreAssets = true;
            Experimental.experimentalFeaturesEnabled = true;
            m_ProBuilderMeshA = ShapeGenerator.CreateShape(ShapeType.Cube);
            m_ProBuilderMeshB = ShapeGenerator.CreateShape(ShapeType.Sphere);
            AssetDatabase.Refresh();
        }

        [OneTimeTearDown]
        public void OnTimeTearDown()
        {
            if (m_ProBuilderMeshA != null)
                Object.DestroyImmediate(m_ProBuilderMeshA.gameObject);
            if (m_ProBuilderMeshB != null)
                Object.DestroyImmediate(m_ProBuilderMeshB.gameObject);
            FileUtility.CleanUpDataDirectory();
        }

        [Test]
        public void ProBuilderMesh_IsBackedBy_MeshAsset()
        {
            Assume.That(m_ProBuilderMeshA, Is.Not.Null);
            var asset = m_ProBuilderMeshA.filter.sharedMesh;
            Assume.That(asset, Is.Not.Null);
            var path = AssetDatabase.GetAssetPath(asset);
            Assert.That(path, Is.Not.Empty);
            var loaded = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            Assert.That(loaded, Is.Not.Null);
        }
    }
}
