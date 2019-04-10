using UnityEngine;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.EditorTests.Undo
{
    static class TestUndo
    {
        [Test]
        public static void RegisterComplete()
        {
            var cube = ShapeGenerator.CreateShape(ShapeType.Cube);
            var duplicate = UnityEngine.Object.Instantiate(cube.gameObject).GetComponent<ProBuilderMesh>();
            duplicate.MakeUnique();

            TestUtility.AssertAreEqual(cube.mesh, duplicate.mesh);

            UnityEditor.Undo.RegisterCompleteObjectUndo(new[] { cube }, "Merge Vertices");
            cube.MergeVertices(new int[] { 0, 1 }, true);
            cube.ToMesh();
            cube.Refresh();

            Assert.IsFalse(TestUtility.MeshesAreEqual(cube.mesh, duplicate.mesh));

            UnityEditor.Undo.PerformUndo();

            // this is usually caught by UndoUtility
            cube.InvalidateCaches();

            cube.ToMesh();
            cube.Refresh();

            TestUtility.AssertAreEqual(duplicate.mesh, cube.mesh);

            UnityEngine.Object.DestroyImmediate(cube.gameObject);
            UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
        }
    }
}
