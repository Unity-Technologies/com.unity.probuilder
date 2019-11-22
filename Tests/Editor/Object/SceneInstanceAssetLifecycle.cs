using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.TestTools;
using UObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder.Tests
{
    public class SceneInstanceMeshAssetLifecycle
    {
        EditorWindow m_SceneView;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Experimental.meshesAreAssets = false;
            m_SceneView = EditorWindow.GetWindow<SceneView>();
        }

        [UnityTest]
        public IEnumerator DuplicateMesh_InstantiatesNewMeshAsset()
        {
            var original = ShapeGenerator.CreateShape(ShapeType.Cube);

            MeshSelection.SetSelection(original.gameObject);
            UnityEditor.Selection.activeGameObject = original.gameObject;
            yield return null;
            Assume.That(UnityEditor.Selection.activeGameObject, Is.EqualTo(original.gameObject));

            m_SceneView.SendEvent(new Event() { type = EventType.ValidateCommand, commandName = "Duplicate" });
            m_SceneView.SendEvent(new Event() { type = EventType.ExecuteCommand, commandName = "Duplicate" });

            yield return null;

            Assume.That(UnityEditor.Selection.activeGameObject, Is.Not.EqualTo(original.gameObject));

            var duplicate = UnityEditor.Selection.activeGameObject.GetComponent<ProBuilderMesh>();

            Assume.That(duplicate, Is.Not.Null);

            Assert.That(original.mesh, Is.Not.EqualTo(duplicate.mesh));

            UObject.DestroyImmediate(original.gameObject);
            UObject.DestroyImmediate(UnityEditor.Selection.activeGameObject);
        }

        [Test]
        public static void InstantiateFromCode_ReferencesOriginalMeshAsset()
        {
            var original = ShapeGenerator.CreateShape(ShapeType.Cube);
            var copy = UObject.Instantiate(original);

            try
            {
                Assert.AreNotEqual(copy, original, "GameObject references are equal");
                Assert.IsTrue(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
            }
            finally
            {
                UObject.DestroyImmediate(original.gameObject);
                UObject.DestroyImmediate(copy.gameObject);
            }
        }

        [Test]
        public static void DestroyGameObject_AlsoDestroysMesh()
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
            Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
            UObject.DestroyImmediate(pb.gameObject);
            // IsNull doesn't work due to c#/c++ goofiness
            Assert.IsTrue(mesh == null);
        }

        [Test]
        public static void Destroy_WithNoDeleteFlag_PreservesMesh()
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

            try
            {
                Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
                pb.preserveMeshAssetOnDestroy = true;
                UObject.DestroyImmediate(pb.gameObject);
                Assert.IsFalse(mesh == null);
            }
            finally
            {
                if (pb != null)
                    UObject.DestroyImmediate(pb.gameObject);
            }
        }

        [Test]
        public static void DestroyDoesNotDeleteMeshBackByAsset()
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
            string path = TestUtility.SaveAssetTemporary<Mesh>(pb.mesh);
            Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
            UObject.DestroyImmediate(pb.gameObject);
            Assert.IsFalse(mesh == null);
            AssetDatabase.DeleteAsset(path);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
