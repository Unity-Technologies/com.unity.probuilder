using System;
using System.Collections;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using UnityEditor.ProBuilder;
using UnityEngine.TestTools;

namespace UnityEngine.ProBuilder.EditorTests.Object
{
    class MeshAssetManagement
    {
        EditorWindow m_SceneView;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
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
    }
}
